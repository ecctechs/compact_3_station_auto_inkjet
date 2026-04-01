const express = require("express");
const sequelize = require("./database");
const cors = require("cors");
const bodyParser = require("body-parser");

sequelize.sync();

const patternRoute = require("./routes/Pattern");
const jobRoute = require("./routes/Job");
const systemRoute = require("./routes/System");
const uvInkjetRoute = require("./routes/UVInkjet");

const app = express();
app.use(bodyParser.json());
app.use(express.json());
app.use(cors());

app.use(patternRoute);
app.use(jobRoute);
app.use(systemRoute);
app.use(uvInkjetRoute);

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => console.log(`server is running on port ${PORT}`));
