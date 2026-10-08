const { Op } = require("sequelize");
const sequelize = require("../database");
const ResponseManager = require("../middleware/ResponseManager");
const { MachineQueue } = require("../model/machineQueueModel");
const { PrintJob, PrintJobCommand } = require("../model/jobModel");
const { DISPATCH, withQueueLock, conflict, latestDispatch, assertNoUnresolved } = require("../services/queueGuard");

const reportError = (req, res, error) =>
  ResponseManager.ErrorResponse(req, res, error.statusCode || 500, error.message);

// สถานะของแถวในคิว
const PENDING = "pending";
const ACTIVE = "active";
const DONE = "done";

class MachineQueueController {
  /**
   * GET /machine-queue/getAll?machine=UV2
   *
   * คืนทุกแถวที่ยังไม่ปล่อยเครื่อง (pending กับ active) เรียงเก่าก่อนใหม่
   * ฝั่งโปรแกรมใช้ดูว่าเครื่องไหนว่าง และมีอะไรรออยู่บ้าง
   */
  static async getAll(req, res) {
    try {
      const where = { state: { [Op.in]: [PENDING, ACTIVE] } };
      if (req.query.machine) where.machine = req.query.machine;

      // เรียงชุดเดียวกับที่ release ใช้เลือกคิวถัดไป
      //
      // เดิมเรียงด้วย created_at ซึ่งไม่เท่ากับลำดับจริงเมื่อมีการดันแถวไปต่อท้ายคิว
      // (เลือก "ปล่อยเครื่อง" ของงานที่เข้าเครื่องเดิมหลายรอบ จะเลื่อน queued_at)
      // ผลคือเลข Q ที่หน้า Order Detail บอก จะไม่ตรงกับตัวที่ได้เครื่องไปจริง
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

  /**
   * POST /machine-queue/enqueue
   * body: { print_jobs_id, items: [{ machine, round?, program_name? }] }
   *
   * จองเครื่องให้งานหนึ่ง — เรียกตอนกดเริ่มงาน จองครบทุกเครื่องที่ marking ต้องใช้
   *
   * เรียกซ้ำด้วยงานเดิมไม่สร้างของซ้ำ แถวที่มีอยู่แล้วและยังไม่ปล่อยเครื่องจะถูก
   * ข้ามไป กันกรณีกดเริ่มงานรัว ๆ หรือกดซ้ำหลังส่งไปแล้วบางเครื่อง
   */
  static async enqueue(req, res) {
    try {
      const created = await withQueueLock(async (t) => {
        const { print_jobs_id, items } = req.body;
        const job = await PrintJob.findByPk(print_jobs_id, { transaction: t });
        if (!job || !["Waiting", "Process"].includes(job.status))
          conflict("งานถูกยกเลิกหรือจบแล้ว จองคิวไม่ได้");

        // ดูทุกสถานะรวม done ด้วย ไม่ใช่เฉพาะที่ยังค้างอยู่
        //
        // เครื่องที่พิมพ์ให้งานนี้จบไปแล้วห้ามถูกจองซ้ำ ไม่งั้นกดเริ่มงานอีกครั้ง
        // จะเข้าคิวใหม่แล้วพ่นซ้ำลงชิ้นงานเดิม
        const existing = await MachineQueue.findAll({
          where: { print_jobs_id },
          transaction: t,
        });

        const taken = new Set(existing.map((r) => `${r.machine}#${r.round}`));

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

  /**
   * POST /machine-queue/claim
   * body: { machine }
   *
   * หยิบงานถัดไปของเครื่องนั้นมาถือเครื่อง — คืน null ถ้าเครื่องไม่ว่างหรือคิวว่าง
   *
   * เครื่องไม่ว่าง (มีแถว active ค้างอยู่) จะไม่หยิบให้ เพราะโปรแกรมที่อยู่ในเครื่อง
   * ตอนนี้ยังพิมพ์ไม่เสร็จ การเปลี่ยนโปรแกรมทับคือพิมพ์ผิดแบบลงชิ้นงานจริง
   *
   * ทำในทรานแซกชันเดียวและล็อกแถวไว้ กันสองเครื่องที่ poll พร้อมกันหยิบงานใบเดียวกัน
   */
  static async claim(req, res) {
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

        // ดูหัวคิวของเครื่องก่อนเสมอ งานจาก ST1 ห้ามแซงงานที่ ST3 จองไว้ก่อน
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

        // ผู้เรียกมีข้อมูลของงานที่ขอเท่านั้น ห้ามคืนงานอื่นไปส่งด้วยข้อมูลผิดใบ
        if (print_jobs_id && next.print_jobs_id !== print_jobs_id)
          return { claimed: null, reason: "queued" };

        // ไม่ตั้ง sent_at ตรงนี้ — active แปลว่า "ถึงคิวแล้ว รอ ST1 ส่ง" เท่านั้น
        // ฝั่งที่ส่งสำเร็จจริงเป็นคนประทับเวลาเอง แถวที่ยังไม่มี sent_at คือแถวที่
        // ST1 ต้องหยิบไปส่ง จึงไม่มีทางส่งซ้ำแถวที่ส่งไปแล้ว
        await next.update({ state: ACTIVE }, { transaction: t });

        return { claimed: next };
      });
      return ResponseManager.SuccessResponse(req, res, 200, result);
    } catch (err) {
      return reportError(req, res, err);
    }
  }

  /**
   * POST /machine-queue/release
   * body: { machine }
   *
   * ปล่อยเครื่อง — คนกดปุ่มหน้างานที่เครื่องนั้น แปลว่าพิมพ์ชิ้นเดิมเสร็จแล้ว
   * แถวที่ถือเครื่องอยู่กลายเป็น done เครื่องจึงว่างให้คิวถัดไป
   */
  static async release(req, res) {
    try {
      const result = await withQueueLock(async (t) => {
        const { machine, hold_for_next_round, expected_holder_id } = req.body;

        const holder = await MachineQueue.findOne({
          where: { machine, state: ACTIVE },
          transaction: t,
          lock: t.LOCK.UPDATE,
        });

        // คำขอเก่าหรือกดซ้ำต้องไม่ไปปล่อยแถวใหม่ที่เพิ่งได้เครื่อง
        if ((holder?.id ?? null) !== expected_holder_id)
          conflict("คิวเปลี่ยนแล้ว กรุณาตรวจงานที่ถือเครื่องก่อนกดอีกครั้ง");
        if (holder) {
          await assertNoUnresolved([holder], t);
          if (!holder.sent_at) conflict("งานนี้ยังไม่ได้ส่งเข้าเครื่อง ปล่อยคิวไม่ได้");
        }

        if (holder) {
          await holder.update(
            { state: DONE, released_at: new Date() },
            { transaction: t }
          );
        }

        // ยกเครื่องให้คิวถัดไปในจังหวะเดียวกับที่ปล่อย
        //
        // ทำตรงนี้เพราะการกดปุ่มหน้างานคือจังหวะเดียวที่งานใหม่มีสิทธิ์เข้าเครื่อง
        // ถ้าปล่อยให้ฝั่งโปรแกรมไล่หยิบคิวเองเป็นรอบ ๆ งานที่ไม่มีใครกดก็จะถูกส่ง
        // ออกไปเงียบ ๆ ได้ ซึ่งเคยเกิดมาแล้วและกลายเป็นพิมพ์งานที่ไม่มีใครสั่ง
        // งานที่เข้าเครื่องเดิมหลายรอบ (marking 22) เลือกได้ว่าจะถือเครื่องไว้ไหม
        //
        // ถือไว้ = รอบถัดไปของงานเดิมได้เครื่องต่อทันที งานใบอื่นที่รอคิวแทรกไม่ได้
        // จนกว่าชิ้นงานจะกลับมาจากการติด shim นอกไลน์แล้วพ่นรอบสองเสร็จ
        //
        // ไม่ถือ = ใครรอมาก่อนได้ก่อนตามปกติ รอบสองไปต่อท้ายคิว
        let next = null;

        // รอบถัดไปของงานเดิม ถ้ามี
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
            // ดันไปต่อท้ายคิวจริง ๆ
            //
            // แถวของรอบสองถูกสร้างพร้อมรอบแรกตั้งแต่ตอนกดเริ่มงาน มันจึงเก่ากว่าทุกใบ
            // ที่เข้าคิวมาทีหลังเสมอ ถ้าไม่เลื่อนเวลา การเรียงแบบใครมาก่อนได้ก่อนจะยก
            // เครื่องให้รอบสองอยู่ดี กลายเป็นว่าเลือก "ปล่อยเครื่อง" แล้วไม่มีอะไรต่างเลย
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

  /**
   * PATCH /machine-queue/:id
   * body: { state?, program_name? }
   *
   * แก้แถวเดียว — ใช้ตอนเลือกรุ่นย่อยโปรแกรม UV เสร็จ หรือตอนส่งไม่ผ่านแล้วต้อง
   * คืนแถวกลับเป็น pending ให้ลองใหม่
   */
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

        // omitNull ถูกเปิดไว้ทั้งโปรเจค การล้าง program_name เป็นค่าว่างจึงต้องปิดตรงนี้
        await row.update(patch, { omitNull: false, transaction: t });
        return row;
      });
      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return reportError(req, res, err);
    }
  }

  /**
   * DELETE /machine-queue/job/:jobId
   *
   * ล้างคิวของงานหนึ่งทิ้งทั้งหมด — ใช้ตอนยกเลิกงาน จบงาน หรือสั่งพิมพ์ใหม่
   *
   * ลบรวม done ด้วย ไม่ใช่เฉพาะที่ยังค้าง เพราะแถว done คือตัวกันไม่ให้จองเครื่องซ้ำ
   * งานที่กดพิมพ์ใหม่จึงต้องล้างประวัติตรงนี้ก่อน ไม่งั้นจองเครื่องไม่ได้อีกเลย
   *
   * คิวที่เริ่มส่งแล้วแต่ยังไม่รู้ผลจะลบไม่ได้ ต้องตรวจเครื่องก่อน
   */
  static async clearJob(req, res) {
    try {
      const removed = await withQueueLock(async (t) => {
        const rows = await MachineQueue.findAll({ where: { print_jobs_id: req.params.jobId }, transaction: t });
        await assertNoUnresolved(rows, t);
        if (req.query.only_unsent === "true" && rows.some(r => r.state !== PENDING || r.sent_at))
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

  // จดก่อนส่งจริง ถ้าโปรแกรมดับ แถวนี้จะไม่ถูกหยิบส่งซ้ำตอนเปิดใหม่
  static async beginSend(req, res) {
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
            payload: { queue_id: row.id, token: req.body.token, outcome: "sending" },
            sent_at: new Date(),
          }, { transaction: t });
          await job.update({ status: "Process" }, { transaction: t });
          return { token: req.body.token };
      });
      return ResponseManager.SuccessResponse(req, res, 200, result);
    } catch (err) { return reportError(req, res, err); }
  }

  // เก็บประวัติกับผลคิวพร้อมกัน เรียกซ้ำด้วย token เดิมไม่เพิ่มประวัติซ้ำ
  static async finishSend(req, res) {
    try {
      const result = await withQueueLock(async (t) => {
          const row = await MachineQueue.findByPk(req.params.id, { transaction: t });
          if (!row) conflict("ไม่พบคิวที่ส่ง");
          const attempt = await latestDispatch(row.id, t);
          const { token, outcome, detail, error } = req.body;
          if (!attempt || attempt.payload.token !== token) conflict("คำขอนี้ไม่ใช่รอบส่งปัจจุบัน");
          if (attempt.payload.outcome === outcome) return { recorded: true };
          if (["sent", "not_sent"].includes(attempt.payload.outcome)) conflict("รอบส่งนี้จบแล้ว");
          if (row.state !== ACTIVE || row.sent_at) conflict("คิวเปลี่ยนระหว่างส่ง");
          if (outcome === "not_sent" && attempt.payload.outcome !== "sending")
            conflict("ผลไม่แน่นอน ต้องตรวจเครื่องก่อนคืนคิว");
          if (outcome === "sent") {
            await PrintJobCommand.create({
              job_id: row.print_jobs_id, command: row.machine, success: true,
              payload: detail ?? null, sent_at: new Date(),
            }, { transaction: t });
            await row.update({ sent_at: new Date() }, { transaction: t });
          } else if (outcome === "not_sent") {
            await row.update({ state: PENDING }, { transaction: t });
            const successful = await PrintJobCommand.count({
              where: { job_id: row.print_jobs_id, success: true }, transaction: t,
            });
            const stillSending = await PrintJobCommand.count({
              where: {
                job_id: row.print_jobs_id, command: DISPATCH, id: { [Op.ne]: attempt.id },
                payload: { outcome: { [Op.in]: ["sending", "unknown"] } },
              }, transaction: t,
            });
            if (!successful && !stillSending) await PrintJob.update({ status: "Waiting" }, {
              where: { id: row.print_jobs_id, status: "Process" }, transaction: t,
            });
          }
          await attempt.update({ payload: { ...attempt.payload, outcome, error: error || null } }, { transaction: t });
          return { recorded: true };
      });
      return ResponseManager.SuccessResponse(req, res, 200, result);
    } catch (err) { return reportError(req, res, err); }
  }
}

module.exports = MachineQueueController;
