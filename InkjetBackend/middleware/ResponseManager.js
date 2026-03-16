class ResponseManager {
  static SuccessResponse(req, res, statusCode, data) {
    res.status(statusCode).json({
      statusCode: statusCode,
      data: data,
    });
  }

  static ErrorResponse(req, res, statusCode, data) {
    res.status(statusCode).json({
      statusCode: statusCode,
      data: data,
    });
  }

  static CatchResponse(req, res, errorMessage) {
    res.status(500).json({
      statusCode: 500,
      data: errorMessage,
    });
  }
}

module.exports = ResponseManager;
