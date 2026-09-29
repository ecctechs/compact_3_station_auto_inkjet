const { Op } = require("sequelize");
const sequelize = require("../database");
const ResponseManager = require("../middleware/ResponseManager");
const { MachineQueue } = require("../model/machineQueueModel");
const { PrintJob, PrintJobCommand } = require("../model/jobModel");
const { DISPATCH, withQueueLock, conflict, latestDispatch, assertNoUnresolved } = require("../services/queueGuard");

const reportError = (req, res, error) =>
  ResponseManager.ErrorResponse(req, res, error.statusCode || 500, error.message);

const PENDING = "pending";
const ACTIVE = "active";
const DONE = "done";

class MachineQueueController {
  static async getAll(req, res) {
    try {
      const where = { state: { [Op.in]: [PENDING, ACTIVE] } };
      if (req.query.machine) where.machine = req.query.machine;

      const rows = await MachineQueue.findAll({
        where,
        order: [
          ["machine", "ASC"],
          [sequelize.literal('COALESCE("queued_at", "created_at")'), "ASC"],
          ["id", "ASC"],
        ],
      });

      const attempts = rows.length ? await PrintJobCommand.findAll({
        where: { command: DISPATCH, payload: { queue_id: { [Op.in]: rows.map(r => r.id) } } },
        order: [["id", "ASC"]],
      }) : [];
      const latest = new Map(attempts.map(a => [a.payload.queue_id, a.payload.outcome]));
      return ResponseManager.SuccessResponse(req, res, 200, rows.map(row => ({
        ...row.toJSON(), dispatch_state: latest.get(row.id) || null,
      })));
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  static async enqueue(req, res) { // จองเครื่องและรอบ โดยกันรายการเดิมซ้ำ
    try {
      const created = await withQueueLock(async (t) => {
        const { print_jobs_id, items } = req.body;
        const job = await PrintJob.findByPk(print_jobs_id, { transaction: t });
        if (!job || !["Waiting", "Process"].includes(job.status))
          conflict("งานถูกยกเลิกหรือจบแล้ว จองคิวไม่ได้");

        const existing = await MachineQueue.findAll({
          where: { print_jobs_id },
          transaction: t,
        });

        const taken = new Set(existing.map((r) => `${r.machine}#${r.round}`)); // กันจองเครื่องและรอบเดิมซ้ำ รวมแถวที่ปล่อยแล้ว

        const toCreate = [];
        for (const item of items) {
          const round = Number(item.round) || 1;
          const key = `${item.machine}#${round}`;
          if (taken.has(key)) continue;
          taken.add(key);

          toCreate.push({
            print_jobs_id,
            machine: item.machine,
            round,
            program_name: item.program_name ?? null,
            state: PENDING,
          });
        }

        const created = await MachineQueue.bulkCreate(toCreate, {
          transaction: t,
          validate: true,
        });

        return created;
      });
      return ResponseManager.SuccessResponse(req, res, 201, created);
    } catch (err) {
      return reportError(req, res, err);
    }
  }

  static async claim(req, res) { // ให้สิทธิ์หัวคิวเมื่อเครื่องว่าง
    try {
      const result = await withQueueLock(async (t) => {
        const { machine, print_jobs_id } = req.body;

        const busy = await MachineQueue.findOne({
          where: { machine, state: ACTIVE },
          transaction: t,
          lock: t.LOCK.UPDATE,
        });

        if (busy) {
          return {
            claimed: null,
            reason: "busy",
            holder: busy,
          };
        }

        const next = await MachineQueue.findOne({
          where: { machine, state: PENDING },
          order: [
            [sequelize.literal('COALESCE("queued_at", "created_at")'), "ASC"],
            ["id", "ASC"],
          ],
          transaction: t,
          lock: t.LOCK.UPDATE,
        });

        if (!next) {
          return {
            claimed: null,
            reason: "empty",
          };
        }

        if (print_jobs_id && next.print_jobs_id !== print_jobs_id) // งานที่กดเริ่มทีหลังห้ามแซงหัวคิวของเครื่อง
          return { claimed: null, reason: "queued" };

        await next.update({ state: ACTIVE }, { transaction: t }); // ให้สิทธิ์ใช้เครื่อง ยังไม่ได้แปลว่าส่งข้อมูลแล้ว

        return { claimed: next };
      });
      return ResponseManager.SuccessResponse(req, res, 200, result);
    } catch (err) {
      return reportError(req, res, err);
    }
  }

  static async release(req, res) { // ปล่อยคิวเดิมแล้วเลื่อนงานถัดไป
    try {
      const result = await withQueueLock(async (t) => {
        const { machine, hold_for_next_round, expected_holder_id } = req.body; // รับเครื่องและเลขคิวที่ผู้กดตั้งใจปล่อย

        const holder = await MachineQueue.findOne({
          where: { machine, state: ACTIVE },
          transaction: t,
          lock: t.LOCK.UPDATE,
        });

        if ((holder?.id ?? null) !== expected_holder_id) // คำขอเดิมห้ามไปปล่อยคิวใหม่ที่เพิ่งรับเครื่อง
          conflict("คิวเปลี่ยนแล้ว กรุณาตรวจงานที่ถือเครื่องก่อนกดอีกครั้ง");
        if (holder) {
          await assertNoUnresolved([holder], t);
          if (!holder.sent_at) conflict("งานนี้ยังไม่ได้ส่งเข้าเครื่อง ปล่อยคิวไม่ได้"); // ยังไม่บันทึกส่งสำเร็จ จึงไม่ให้ข้ามไปงานถัดไป
        }

        if (holder) {
          await holder.update(
            { state: DONE, released_at: new Date() },
            { transaction: t }
          );
        }

        let next = null;

        const sameJobNextRound = holder
          ? await MachineQueue.findOne({
              where: {
                machine,
                state: PENDING,
                print_jobs_id: holder.print_jobs_id,
                round: holder.round + 1,
              },
              transaction: t,
              lock: t.LOCK.UPDATE,
            })
          : null;

        if (sameJobNextRound) {
          if (hold_for_next_round) {
            next = sameJobNextRound;
          } else {
            await sameJobNextRound.update(
              { queued_at: new Date() },
              { transaction: t }
            );
          }
        }

        if (!next) {
          next = await MachineQueue.findOne({
            where: { machine, state: PENDING },
            order: [
              [sequelize.literal('COALESCE("queued_at", "created_at")'), "ASC"],
              ["id", "ASC"],
            ],
            transaction: t,
            lock: t.LOCK.UPDATE,
          });
        }

        if (next) await next.update({ state: ACTIVE }, { transaction: t });

        return {
          released: holder,
          next,
        };
      });
      return ResponseManager.SuccessResponse(req, res, 200, result);
    } catch (err) {
      return reportError(req, res, err);
    }
  }

  static async update(req, res) {
    try {
      const row = await withQueueLock(async (t) => {
        const row = await MachineQueue.findByPk(req.params.id, { transaction: t });
        if (!row) conflict("Queue row not found");
        await assertNoUnresolved([row], t);

        const { state, program_name, sent } = req.body;
        if (sent !== undefined || state === ACTIVE || state === DONE)
          conflict("ใช้ขั้นตอนรับคิว ส่งงาน และปล่อยเครื่องแทนการแก้สถานะโดยตรง");
        if (row.state === DONE || row.sent_at)
          conflict("คิวนี้ส่งแล้วหรือปล่อยแล้ว แก้กลับไปรอส่งไม่ได้");
        const patch = {};

        if (state === PENDING) patch.state = PENDING;
        if (program_name !== undefined) patch.program_name = program_name || null;

        await row.update(patch, { omitNull: false, transaction: t });
        return row;
      });
      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return reportError(req, res, err);
    }
  }

  static async clearJob(req, res) {
    try {
      const removed = await withQueueLock(async (t) => {
        const rows = await MachineQueue.findAll({ where: { print_jobs_id: req.params.jobId }, transaction: t });
        await assertNoUnresolved(rows, t);
        if (req.query.only_unsent === "true" && rows.some(r => r.state !== PENDING || r.sent_at)) // ล้างแบบปลอดภัยได้เมื่อทุกแถวยังรอและไม่มีผลส่ง
          conflict("มีคิวที่ถูกหยิบหรือส่งแล้ว ห้ามล้างทั้งงาน");
        return MachineQueue.destroy({
          where: { print_jobs_id: req.params.jobId },
          transaction: t,
        });
      });

      return ResponseManager.SuccessResponse(req, res, 200, { removed });
    } catch (err) {
      return reportError(req, res, err);
    }
  }

  static async beginSend(req, res) { // จด token ก่อนให้ฝั่ง C# ส่งเครื่อง
    try {
      const result = await withQueueLock(async (t) => {
          const row = await MachineQueue.findByPk(req.params.id, { transaction: t });
          if (!row || row.state !== ACTIVE || row.sent_at) conflict("คิวนี้ยังไม่พร้อมส่งหรือส่งแล้ว");
          if (await PrintJobCommand.findOne({ where: { command: DISPATCH, payload: { token: req.body.token } }, transaction: t }))
            conflict("คำขอส่งนี้ถูกใช้แล้ว ให้ตรวจผลก่อนส่งใหม่");
          await assertNoUnresolved([row], t);
          const job = await PrintJob.findByPk(row.print_jobs_id, { transaction: t });
          if (!job || !["Waiting", "Process"].includes(job.status)) conflict("งานถูกยกเลิกหรือจบแล้ว");
          await PrintJobCommand.create({
            job_id: row.print_jobs_id, command: DISPATCH, success: false,
            payload: { queue_id: row.id, token: req.body.token, outcome: "sending" }, // จดเลขคิวและรอบส่งไว้ก่อนฝั่ง C# แตะเครื่อง
            sent_at: new Date(),
          }, { transaction: t });
          await job.update({ status: "Process" }, { transaction: t }); // เปลี่ยน Job พร้อมบันทึกเริ่มส่งใน transaction เดียวกัน
          return { token: req.body.token };
      });
      return ResponseManager.SuccessResponse(req, res, 200, result);
    } catch (err) { return reportError(req, res, err); }
  }

  static async finishSend(req, res) { // รับผลส่งแล้วบันทึกคิวกับประวัติ
    try {
      const result = await withQueueLock(async (t) => {
          const row = await MachineQueue.findByPk(req.params.id, { transaction: t });
          if (!row) conflict("ไม่พบคิวที่ส่ง");
          const attempt = await latestDispatch(row.id, t);
          const { token, outcome, detail, error } = req.body;
          if (!attempt || attempt.payload.token !== token) conflict("คำขอนี้ไม่ใช่รอบส่งปัจจุบัน"); // รับผลเฉพาะผู้ส่งที่เริ่มรอบนี้ไว้
          if (attempt.payload.outcome === outcome) return { recorded: true }; // บันทึกซ้ำด้วยผลเดิมได้ โดยไม่เพิ่มประวัติซ้ำ
          if (["sent", "not_sent"].includes(attempt.payload.outcome)) conflict("รอบส่งนี้จบแล้ว"); // รอบที่จบแล้วห้ามเปลี่ยนผลย้อนหลังผ่านคำขอนี้
          if (row.state !== ACTIVE || row.sent_at) conflict("คิวเปลี่ยนระหว่างส่ง");
          if (outcome === "not_sent" && attempt.payload.outcome !== "sending")
            conflict("ผลไม่แน่นอน ต้องตรวจเครื่องก่อนคืนคิว");
          if (outcome === "sent") { // เครื่องรับข้อมูลแล้ว บันทึกประวัติพร้อมเวลาของคิว
            await PrintJobCommand.create({
              job_id: row.print_jobs_id, command: row.machine, success: true,
              payload: detail ?? null, sent_at: new Date(),
            }, { transaction: t });
            await row.update({ sent_at: new Date() }, { transaction: t }); // กันรอบอ่านงานหยิบคิวนี้ไปส่งซ้ำ
          } else if (outcome === "not_sent") { // ยืนยันว่าไม่ได้ส่ง จึงให้กลับไปรอใหม่
            await row.update({ state: PENDING }, { transaction: t }); // คืนคิวโดยไม่สั่งอุปกรณ์ซ้ำใน Backend
            const successful = await PrintJobCommand.count({
              where: { job_id: row.print_jobs_id, success: true }, transaction: t,
            });
            const stillSending = await PrintJobCommand.count({
              where: {
                job_id: row.print_jobs_id, command: DISPATCH, id: { [Op.ne]: attempt.id },
                payload: { outcome: { [Op.in]: ["sending", "unknown"] } },
              }, transaction: t,
            });
            if (!successful && !stillSending) await PrintJob.update({ status: "Waiting" }, { // คืน Waiting ได้เมื่อไม่มีทั้งผลสำเร็จและรอบที่ยังไม่รู้ผล
              where: { id: row.print_jobs_id, status: "Process" }, transaction: t,
            });
          }
          await attempt.update({ payload: { ...attempt.payload, outcome, error: error || null } }, { transaction: t }); // เก็บผลรอบนี้พร้อมเหตุผิดพลาดในหลักฐานเดิม
          return { recorded: true };
      });
      return ResponseManager.SuccessResponse(req, res, 200, result);
    } catch (err) { return reportError(req, res, err); }
  }
}

module.exports = MachineQueueController;
