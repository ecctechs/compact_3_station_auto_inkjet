const express = require("express");
const sequelize = require("./database");
const cors = require("cors");
const bodyParser = require("body-parser");

// Load all models (registers associations before sync)
require("./model/patternModel");
require("./model/jobModel");
require("./model/plcModel");
require("./model/uvJobDataModel");
require("./model/lastSentJobModel");
require("./model/iaiSettingModel");

sequelize.sync();

const patternRoute = require("./routes/Pattern");
const jobRoute = require("./routes/Job");
const systemRoute = require("./routes/System");
const plcRoute = require("./routes/Plc");

const app = express();
app.use(bodyParser.json());
app.use(express.json());
app.use(cors());

app.use(patternRoute);
app.use(jobRoute);
app.use(systemRoute);
app.use(plcRoute);

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => console.log(`server is running on port ${PORT}`));
