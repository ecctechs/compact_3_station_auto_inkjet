using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkjetOperator.Models
{
    public class JobRow
    {
        public int Id { get; set; }
        public string BarcodeRaw { get; set; }
        public string LotNumber { get; set; }
        public string Status { get; set; }
        public int Attempt { get; set; }
    }
}
