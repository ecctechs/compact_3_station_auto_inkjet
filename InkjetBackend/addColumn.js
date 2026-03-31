const sequelize = require("./database");

async function addColumn() {
  try {
    await sequelize.query('ALTER TABLE print_jobs ADD COLUMN pattern_no_erp VARCHAR(255);');
    console.log("Column pattern_no_erp added successfully");
    process.exit(0);
  } catch (err) {
    console.error("Error:", err.message);
    process.exit(1);
  }
}

addColumn();
