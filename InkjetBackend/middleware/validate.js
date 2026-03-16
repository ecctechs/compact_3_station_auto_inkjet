const ResponseManager = require("./ResponseManager");

const validate = (schema, source = "body") => {
  return (req, res, next) => {
    const result = schema.safeParse(req[source]);
    if (!result.success) {
      return ResponseManager.ErrorResponse(req, res, 400, result.error.errors);
    }
    req[source] = result.data;
    next();
  };
};

module.exports = validate;
