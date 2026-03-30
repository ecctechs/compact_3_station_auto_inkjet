using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkjetOperator.Models
{
    public class UvRow
    {
        public int Id { get; set; }
        public string Inkjet { get; set; } = "";
        public string Lot { get; set; } = "";
        public string Name { get; set; } = "";
        public string UpdateAt { get; set; } = "";

        public string Program { get; set; } = "";
    }
}
