const { Op } = require("sequelize");
const sequelize = require("../database");
const ResponseManager = require("../middleware/ResponseManager");
const { MachineQueue } = require("../model/machineQueueModel");

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

      return ResponseManager.SuccessResponse(req, res, 200, rows);
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
    const t = await sequelize.transaction();
    try {
      const { print_jobs_id, items } = req.body;

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

      await t.commit();
      return ResponseManager.SuccessResponse(req, res, 201, created);
    } catch (err) {
      await t.rollback();
      return ResponseManager.CatchResponse(req, res, err.message);
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
    const t = await sequelize.transaction();
    try {
      const { machine, print_jobs_id } = req.body;

      const busy = await MachineQueue.findOne({
        where: { machine, state: ACTIVE },
        transaction: t,
        lock: t.LOCK.UPDATE,
      });

      if (busy) {
        await t.commit();
        return ResponseManager.SuccessResponse(req, res, 200, {
          claimed: null,
          reason: "busy",
          holder: busy,
        });
      }

      // ระบุงานมาด้วย = หยิบเฉพาะแถวของงานใบนั้น ไม่ใช่ใบที่เข้าคิวมาก่อน
      //
      // ปุ่มเริ่มงานใช้ทางนี้ คนกดเริ่มงานใบไหนต้องได้ใบนั้น ไม่ใช่ไปส่งใบอื่น
      // ที่บังเอิญรออยู่ในคิวเครื่องเดียวกัน ซึ่งเท่ากับสั่งพิมพ์งานที่ไม่มีใครกด
      const where = { machine, state: PENDING };
      if (print_jobs_id) where.print_jobs_id = print_jobs_id;

      const next = await MachineQueue.findOne({
        where,
        order: [
          [sequelize.literal('COALESCE("queued_at", "created_at")'), "ASC"],
          ["id", "ASC"],
        ],
        transaction: t,
        lock: t.LOCK.UPDATE,
      });

      if (!next) {
        await t.commit();
        return ResponseManager.SuccessResponse(req, res, 200, {
          claimed: null,
          reason: "empty",
        });
      }

      // ไม่ตั้ง sent_at ตรงนี้ — active แปลว่า "ถึงคิวแล้ว รอ ST1 ส่ง" เท่านั้น
      // ฝั่งที่ส่งสำเร็จจริงเป็นคนประทับเวลาเอง แถวที่ยังไม่มี sent_at คือแถวที่
      // ST1 ต้องหยิบไปส่ง จึงไม่มีทางส่งซ้ำแถวที่ส่งไปแล้ว
      await next.update({ state: ACTIVE }, { transaction: t });

      await t.commit();
      return ResponseManager.SuccessResponse(req, res, 200, { claimed: next });
    } catch (err) {
      await t.rollback();
      return ResponseManager.CatchResponse(req, res, err.message);
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
    const t = await sequelize.transaction();
    try {
      const { machine, hold_for_next_round } = req.body;

      const holder = await MachineQueue.findOne({
        where: { machine, state: ACTIVE },
        transaction: t,
        lock: t.LOCK.UPDATE,
      });

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

      await t.commit();
      return ResponseManager.SuccessResponse(req, res, 200, {
        released: holder,
        next,
      });
    } catch (err) {
      await t.rollback();
      return ResponseManager.CatchResponse(req, res, err.message);
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
      const row = await MachineQueue.findByPk(req.params.id);
      if (!row) {
        return ResponseManager.ErrorResponse(req, res, 404, "Queue row not found");
      }

      const { state, program_name, sent } = req.body;
      const patch = {};

      if ([PENDING, ACTIVE, DONE].includes(String(state))) patch.state = String(state);
      if (program_name !== undefined) patch.program_name = program_name || null;

      // ส่งเข้าเครื่องสำเร็จแล้ว — ประทับเวลาไว้กันหยิบไปส่งซ้ำ
      if (sent === true) patch.sent_at = new Date();

      // omitNull ถูกเปิดไว้ทั้งโปรเจค การล้าง program_name เป็นค่าว่างจึงต้องปิดตรงนี้
      await row.update(patch, { omitNull: false });
      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
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
   * แถว active ก็โดนลบด้วย ซึ่งเท่ากับปล่อยเครื่องให้คิวถัดไปทันที — ถูกต้องแล้ว
   * เพราะงานที่ถูกยกเลิกไม่ควรถือเครื่องไว้ต่อ
   */
  static async clearJob(req, res) {
    try {
      const removed = await MachineQueue.destroy({
        where: { print_jobs_id: req.params.jobId },
      });

      return ResponseManager.SuccessResponse(req, res, 200, { removed });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = MachineQueueController;
