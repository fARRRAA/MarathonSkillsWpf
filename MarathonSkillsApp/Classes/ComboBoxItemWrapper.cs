using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarathonSkillsApp.Classes
{
    public class ComboBoxItemWrapper<T>
    {
        public string Display { get; set; }
        public T Value { get; set; }

        public override string ToString()
        {
            return Display;
        }
    }
}
