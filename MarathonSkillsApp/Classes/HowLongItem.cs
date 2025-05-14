using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarathonSkillsApp.Classes
{
    public class HowLongItem
    {
        public string Name { get; set; }
        public string ImageFile { get; set; }
        public string Type { get; set; } // "speed" или "length"
        public double Value { get; set; } // скорость или длина
    }
}
