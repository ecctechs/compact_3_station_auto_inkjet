const ResponseManager = require("../middleware/ResponseManager");
const sequelize = require("../database");
const { PlcRegisterMap } = require("../model/plcSettingModel");

class PlcSettingController {
  /**
   * POST /plc-setting/create
   * Create new PLC register mapping row
   */
  static async create(req, res) {
    try {
      const { address_start, address_stop, plc_start, plc_stop, list_name, data_type, bit, sort_order } = req.body;

      const row = await PlcRegisterMap.create({
        address_start,
        address_stop,
        plc_start,
        plc_stop,
        list_name,
        data_type: data_type || "Int",
        bit: bit || 32,
        sort_order: sort_order || 0,
      });

      return ResponseManager.SuccessResponse(req, res, 201, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * GET /plc-setting/getAll
   * Get PLC register mapping rows.
   * Config table: returns ALL rows (ordered by sort_order) when page/limit
   * are not provided; paginates only when requested.
   */
  static async getAll(req, res) {
    try {
      const { page, limit } = req.query;
      const order = [
        ["sort_order", "ASC"],
        ["address_start", "ASC"],
      ];

      if (!page && !limit) {
        const rows = await PlcRegisterMap.findAll({ order });
        return ResponseManager.SuccessResponse(req, res, 200, {
          data: rows,
          total: rows.length,
          page: 1,
          limit: rows.length,
        });
      }

      const pageNum = parseInt(page) || 1;
      const limitNum = parseInt(limit) || 10;
      const offset = (pageNum - 1) * limitNum;

      const { count, rows } = await PlcRegisterMap.findAndCountAll({
        order,
        offset,
        limit: limitNum,
      });

      return ResponseManager.SuccessResponse(req, res, 200, {
        data: rows,
        total: count,
        page: pageNum,
        limit: limitNum,
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * GET /plc-setting/getById/:id
   * Get PLC register mapping row by ID
   */
  static async getById(req, res) {
    try {
      const row = await PlcRegisterMap.findByPk(req.params.id);

      if (!row) {
        return ResponseManager.ErrorResponse(req, res, 404, "PLC setting record not found");
      }

      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * PUT /plc-setting/update/:id
   * Update PLC register mapping row
   */
  static async update(req, res) {
    try {
      const row = await PlcRegisterMap.findByPk(req.params.id);

      if (!row) {
        return ResponseManager.ErrorResponse(req, res, 404, "PLC setting record not found");
      }

      const { address_start, address_stop, plc_start, plc_stop, list_name, data_type, bit, sort_order } = req.body;

      await row.update({
        address_start: address_start !== undefined ? address_start : row.address_start,
        address_stop: address_stop !== undefined ? address_stop : row.address_stop,
        plc_start: plc_start !== undefined ? plc_start : row.plc_start,
        plc_stop: plc_stop !== undefined ? plc_stop : row.plc_stop,
        list_name: list_name !== undefined ? list_name : row.list_name,
        data_type: data_type !== undefined ? data_type : row.data_type,
        bit: bit !== undefined ? bit : row.bit,
        sort_order: sort_order !== undefined ? sort_order : row.sort_order,
      });

      return ResponseManager.SuccessResponse(req, res, 200, row);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * DELETE /plc-setting/delete/:id
   * Delete PLC register mapping row
   */
  static async delete(req, res) {
    try {
      const row = await PlcRegisterMap.findByPk(req.params.id);

      if (!row) {
        return ResponseManager.ErrorResponse(req, res, 404, "PLC setting record not found");
      }

      await row.destroy();

      return ResponseManager.SuccessResponse(req, res, 200, {
        message: "PLC setting record deleted successfully",
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * POST /plc-setting/bulkSave
   * Replace the whole mapping table in one transaction.
   * Body: { rows: [{ address_start, address_stop, plc_start, plc_stop, list_name, data_type, bit }, ...] }
   * Used by the operator UI Save button (send the full table at once).
   */
  static async bulkSave(req, res) {
    const t = await sequelize.transaction();
    try {
      const { rows } = req.body;

      if (!Array.isArray(rows)) {
        await t.rollback();
        return ResponseManager.ErrorResponse(req, res, 400, "Body must contain 'rows' array");
      }

      await PlcRegisterMap.destroy({ where: {}, transaction: t });

      const created = await PlcRegisterMap.bulkCreate(
        rows.map((r, i) => ({
          address_start: r.address_start,
          address_stop: r.address_stop,
          plc_start: r.plc_start,
          plc_stop: r.plc_stop,
          list_name: r.list_name,
          data_type: r.data_type || "Int",
          bit: r.bit || 32,
          sort_order: r.sort_order !== undefined ? r.sort_order : i,
        })),
        { transaction: t, validate: true }
      );

      await t.commit();

      return ResponseManager.SuccessResponse(req, res, 200, {
        data: created,
        total: created.length,
      });
    } catch (err) {
      await t.rollback();
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = PlcSettingController;
