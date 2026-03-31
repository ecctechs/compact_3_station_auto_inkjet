using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkjetOperator.Models
{
    public class BotStep
    {
        public string Name { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public Rectangle VerifyArea { get; set; }
    }
}
