const ResponseManager = require("../middleware/ResponseManager");

/**
 * Size conversion dict — ported from rs232_connector.py lines 8-22.
 * Maps logical size (0-22 range) to device-specific encoding.
 * Served to C# app so it doesn't need to hardcode this.
 */
const SIZE_CONVERSION = {
  "1": "0",
  "2": "1",
  "3": "17",
  "4": "3",
  "5": "18",
  "6": "20",
  "7": "5",
  "8": "6",
  "9": "7",
  "10": "8",
  "11": "9",
  "12": "10",
  "13": "11",
};

/**
 * Bilingual translations — ported from miscellaneous.py LangDict (lines 24-58).
 */
const TRANSLATIONS = {
  ENG: "\u0E44\u0E17\u0E22",
  Auto: "\u0E2D\u0E31\u0E15\u0E42\u0E19\u0E21\u0E31\u0E15\u0E34",
  Manual: "\u0E04\u0E27\u0E1A\u0E04\u0E38\u0E21\u0E40\u0E2D\u0E07",
  "Pattern No: ":
    "\u0E25\u0E33\u0E14\u0E31\u0E1A\u0E42\u0E1B\u0E23\u0E41\u0E01\u0E23\u0E21:",
  Enter: "\u0E14\u0E36\u0E07\u0E02\u0E49\u0E2D\u0E21\u0E39\u0E25",
  SEND: "\u0E2A\u0E48\u0E07",
  Browse: "\u0E40\u0E25\u0E37\u0E2D\u0E01\u0E44\u0E1F\u0E25\u0E4C",
  TEXT: "\u0E02\u0E49\u0E2D\u0E04\u0E27\u0E32\u0E21",
  TYPE: "\u0E1B\u0E23\u0E30\u0E40\u0E20\u0E17\u0E40\u0E2B\u0E25\u0E47\u0E01",
  SERVO:
    "\u0E41\u0E01\u0E19\u0E2B\u0E31\u0E27\u0E2D\u0E34\u0E07\u0E04\u0E4C\u0E40\u0E08\u0E17",
  "Position:": "\u0E15\u0E33\u0E41\u0E2B\u0E19\u0E48\u0E07",
  "Post Act.:":
    "\u0E15\u0E33\u0E41\u0E2B\u0E19\u0E48\u0E07\u0E41\u0E01\u0E19",
  "Servo: Post Act.:":
    "\u0E15\u0E33\u0E41\u0E2B\u0E19\u0E48\u0E07\u0E2B\u0E31\u0E27\u0E1E\u0E34\u0E21\u0E1E\u0E4C:",
  "Delay:": "\u0E2B\u0E19\u0E48\u0E27\u0E07\u0E40\u0E27\u0E25\u0E32",
  "Trigger Delay:":
    "\u0E2B\u0E19\u0E48\u0E27\u0E07\u0E23\u0E30\u0E22\u0E30\u0E17\u0E32\u0E07",
  Conveyor: "\u0E2A\u0E32\u0E22\u0E1E\u0E32\u0E19",
  SPEED: "\u0E04\u0E27\u0E32\u0E21\u0E40\u0E23\u0E47\u0E27",
  "Conveyor 1:": "\u0E2A\u0E32\u0E22\u0E1E\u0E32\u0E19 1:",
  "Conveyor 2:": "\u0E2A\u0E32\u0E22\u0E1E\u0E32\u0E19 2:",
  "Conveyor 3:": "\u0E2A\u0E32\u0E22\u0E1E\u0E32\u0E19 3:",
  Size: "\u0E02\u0E19\u0E32\u0E14",
  Scale: "\u0E2A\u0E40\u0E01\u0E25",
  "Block 1:": "\u0E0A\u0E48\u0E2D\u0E07\u0E17\u0E35\u0E48 1:",
  "Block 2:": "\u0E0A\u0E48\u0E2D\u0E07\u0E17\u0E35\u0E48 2:",
  "Block 3:": "\u0E0A\u0E48\u0E2D\u0E07\u0E17\u0E35\u0E48 3:",
  "Block 4:": "\u0E0A\u0E48\u0E2D\u0E07\u0E17\u0E35\u0E48 4:",
  "Block 5:": "\u0E0A\u0E48\u0E2D\u0E07\u0E17\u0E35\u0E48 5:",
  Width: "\u0E04\u0E27\u0E32\u0E21\u0E2A\u0E39\u0E07",
  Height: "\u0E04\u0E27\u0E32\u0E21\u0E01\u0E27\u0E49\u0E32\u0E07",
  "No File Selected.":
    "\u0E22\u0E31\u0E07\u0E44\u0E21\u0E48\u0E44\u0E14\u0E49\u0E40\u0E25\u0E37\u0E2D\u0E01\u0E44\u0E1F\u0E25\u0E4C",
  PROGRAM: "\u0E42\u0E1B\u0E23\u0E41\u0E01\u0E23\u0E21",
  SWAP: "\u0E2A\u0E25\u0E31\u0E1A",
};

class SystemController {
  static async getSizeMap(req, res) {
    return ResponseManager.SuccessResponse(req, res, 200, SIZE_CONVERSION);
  }

  static async getTranslations(req, res) {
    return ResponseManager.SuccessResponse(req, res, 200, TRANSLATIONS);
  }
}

module.exports = SystemController;
