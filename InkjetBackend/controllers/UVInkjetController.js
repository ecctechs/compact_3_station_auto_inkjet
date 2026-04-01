const ResponseManager = require("../middleware/ResponseManager");
const { UVInkjet } = require("../model/uvInkjetModel");

class UVInkjetController {
  /**
   * POST /uv-inkjet/create
   * Create new UV Inkjet record
   */
  static async create(req, res) {
    try {
      const { print_jobs_id, inkjet_name, lot, name, program_name, status, station } = req.body;

      const uvInkjet = await UVInkjet.create({
        print_jobs_id,
        inkjet_name,
        lot,
        name,
        program_name,
        status: status || "idle",
        station,
        update_at: new Date(),
      });

      return ResponseManager.SuccessResponse(req, res, 201, uvInkjet);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * GET /uv-inkjet/getAll
   * Get all UV Inkjet records with optional filters
   */
  static async getAll(req, res) {
    try {
      const { status, station, inkjet_name, page, limit } = req.query;
      const where = {};

      if (status) {
        where.status = status;
      }
      if (station) {
        where.station = station;
      }
      if (inkjet_name) {
        where.inkjet_name = inkjet_name;
      }

      const pageNum = parseInt(page) || 1;
      const limitNum = parseInt(limit) || 10;
      const offset = (pageNum - 1) * limitNum;

      const { count, rows } = await UVInkjet.findAndCountAll({
        where,
        order: [["updated_at", "DESC"]],
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
   * GET /uv-inkjet/getById/:id
   * Get UV Inkjet record by ID
   */
  static async getById(req, res) {
    try {
      const uvInkjet = await UVInkjet.findByPk(req.params.id);

      if (!uvInkjet) {
        return ResponseManager.ErrorResponse(req, res, 404, "UV Inkjet record not found");
      }

      return ResponseManager.SuccessResponse(req, res, 200, uvInkjet);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * PUT /uv-inkjet/update/:id
   * Update UV Inkjet record
   */
  static async update(req, res) {
    try {
      const uvInkjet = await UVInkjet.findByPk(req.params.id);

      if (!uvInkjet) {
        return ResponseManager.ErrorResponse(req, res, 404, "UV Inkjet record not found");
      }

      const { print_jobs_id, inkjet_name, lot, name, program_name, status, station } = req.body;

      await uvInkjet.update({
        print_jobs_id: print_jobs_id !== undefined ? print_jobs_id : uvInkjet.print_jobs_id,
        inkjet_name: inkjet_name !== undefined ? inkjet_name : uvInkjet.inkjet_name,
        lot: lot !== undefined ? lot : uvInkjet.lot,
        name: name !== undefined ? name : uvInkjet.name,
        program_name: program_name !== undefined ? program_name : uvInkjet.program_name,
        status: status !== undefined ? status : uvInkjet.status,
        station: station !== undefined ? station : uvInkjet.station,
        update_at: new Date(),
      });

      return ResponseManager.SuccessResponse(req, res, 200, uvInkjet);
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }

  /**
   * DELETE /uv-inkjet/delete/:id
   * Delete UV Inkjet record
   */
  static async delete(req, res) {
    try {
      const uvInkjet = await UVInkjet.findByPk(req.params.id);

      if (!uvInkjet) {
        return ResponseManager.ErrorResponse(req, res, 404, "UV Inkjet record not found");
      }

      await uvInkjet.destroy();

      return ResponseManager.SuccessResponse(req, res, 200, {
        message: "UV Inkjet record deleted successfully",
      });
    } catch (err) {
      return ResponseManager.CatchResponse(req, res, err.message);
    }
  }
}

module.exports = UVInkjetController;
