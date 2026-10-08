const ResponseManager = require("../middleware/ResponseManager");
const { PlcRegisterMap } = require("../model/plcModel");
const sequelize = require("../database");

class PlcController {
  // GET /plc-setting/getAll — all rows ordered by sort_order (no pagination).
  static async getAll(req, res) {
    try {
      const rows = await PlcRegisterMap.findAll({
        order: [["sort_order", "ASC"]],
      });
      return ResponseManager.SuccessResponse(req, res, 200, rows);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  // POST /plc-setting/bulkSave — replaces the ENTIRE table in one transaction:
  // delete all existing rows, then recreate from the request body { rows: [...] }.
  static async bulkSave(req, res) {
    const t = await sequelize.transaction();
    try {
      const incoming = Array.isArray(req.body && req.body.rows)
        ? req.body.rows
        : [];

      await PlcRegisterMap.destroy({ where: {}, transaction: t });

      const toCreate = incoming.map((r, i) => ({
        address_start: Number(r.address_start) || 0,
        address_stop: Number(r.address_stop) || 0,
        plc_start: r.plc_start || "",
        plc_stop: r.plc_stop || "",
        list_name: r.list_name || "",
        data_type: r.data_type || "Int",
        bit: Number(r.bit) || 32,
        sort_order: r.sort_order != null ? Number(r.sort_order) : i,
      }));

      const created = await PlcRegisterMap.bulkCreate(toCreate, {
        transaction: t,
        validate: true,
      });

      await t.commit();
      return ResponseManager.SuccessResponse(req, res, 200, created);
    } catch (err) {
      await t.rollback();
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = PlcController;
