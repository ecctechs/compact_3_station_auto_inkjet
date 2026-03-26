/**
 * Template resolver — ported from csv_extractor.py format() (lines 425-437)
 * and client.py process() DDDD/CCCC encoding (lines 155-166).
 *
 * Templates in text block strings:
 *   {yyyy}  -> current 4-digit year
 *   {mm}    -> current month (no leading zero, matches Python str(month))
 *   {dd}    -> current day (no leading zero)
 *   {s-N}   -> sequential counter: N + attempt
 *   CCCC    -> lot number (full)
 *   DDDD    -> encoded date from lot: chr(65 + year-2015) + chr(65 + month) + dd + rest
 */

/**
 * Encode DDDD from lot number.
 * Lot format example: "X2503120001" where digits [1:3]=year, [3:5]=month, [5:7]=day, [7:]=rest
 * From client.py lines 155-158:
 *   yy = chr(ord('A') + (int(lot_value[1:3]) - 15))
 *   mm = chr(ord('A') + int(lot_value[3:5]))
 *   dd = lot_value[5:7]
 *   DDDD = '{}{}{}'.format(yy, mm, dd) + lot_value[7:]
 */
function encodeDDDD(lotNumber) {
  if (lotNumber.length < 7) return lotNumber;

  const yearPart = parseInt(lotNumber.substring(1, 3), 10);
  const monthPart = parseInt(lotNumber.substring(3, 5), 10);
  const dayPart = lotNumber.substring(5, 7);
  const rest = lotNumber.substring(7);

  const yy = String.fromCharCode(65 + yearPart - 15); // 'A' + (year - 15)
  const mm = String.fromCharCode(65 + monthPart); // 'A' + month

  return `${yy}${mm}${dayPart}${rest}`;
}

/**
 * Resolve all template placeholders in a text string.
 * Ported from csv_extractor.py format() + client.py CCCC/DDDD replacement.
 */
function resolveTemplates(text, ctx) {
  const now = new Date();

  let output = text
    .replace(/\{yyyy\}/g, String(now.getFullYear()))
    .replace(/\{mm\}/g, String(now.getMonth() + 1))
    .replace(/\{dd\}/g, String(now.getDate()));

  // {s-N} -> sequential counter: N + attempt
  // From csv_extractor.py line 434: str(int(format_match[0].split('-')[1]) + attempt)
  output = output.replace(/\{s-(\d+)\}/g, (_match, n) => {
    return String(parseInt(n, 10) + ctx.attempt);
  });

  // CCCC -> lot number
  output = output.replace(/CCCC/g, ctx.lotNumber);

  // DDDD -> encoded date from lot
  output = output.replace(/DDDD/g, encodeDDDD(ctx.lotNumber));

  return output;
}

// /**
//  * Parse a raw barcode into lot number + pattern code.
//  * From client.py lines 216-218:
//  *   pattern = barcode_value.split('-')[-1]
//  *   index_dash = barcode_value.rfind('-')
//  *   lot = barcode_value[:index_dash]
//  */
// function parseBarcode(barcodeRaw) {
//   const lastDash = barcodeRaw.lastIndexOf("-");
//   if (lastDash === -1) {
//     return { lotNumber: "", patternCode: barcodeRaw };
//   }
//   return {
//     lotNumber: barcodeRaw.substring(0, lastDash),
//     patternCode: barcodeRaw.substring(lastDash + 1),
//   };
// }

/**
 * Parse a raw barcode into pattern code (prefix) + lot number (suffix).
 * Format: {pattern}-{lot}
 * Example: 3432432-C200521-001 -> pattern: 3432432, lot: C200521-001
 */
function parseBarcode(barcodeRaw) {
  // ใช้ indexOf เพื่อหาตำแหน่งขีดตัวแรกสุด
  const firstDash = barcodeRaw.indexOf("-");
  
  if (firstDash === -1) {
    // ถ้าไม่มีขีดเลย ให้ถือว่า barcode ทั้งหมดคือ pattern code และไม่มี lot
    return { lotNumber: "", patternCode: barcodeRaw };
  }

  return {
    // patternCode คือส่วนก่อนขีดแรก
    patternCode: barcodeRaw.substring(0, firstDash),
    // lotNumber คือส่วนหลังขีดแรกทั้งหมด
    lotNumber: barcodeRaw.substring(firstDash + 1),
  };
}

module.exports = { encodeDDDD, resolveTemplates, parseBarcode };
