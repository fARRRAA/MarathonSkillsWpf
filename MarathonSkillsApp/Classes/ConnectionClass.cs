using MarathonSkillsApp.DB_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarathonSkillsApp.Classes
{
    internal class ConnectionClass
    {
        public static mrthnskillsEntities connect = new mrthnskillsEntities();
    }
}
