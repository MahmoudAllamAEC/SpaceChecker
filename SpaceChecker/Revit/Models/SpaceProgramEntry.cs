using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceChecker.Revit.Models
{
    public class SpaceProgramEntry
    {
        public string RoomName { get; set; }
        public string Department { get; set; } 
        public double RequiredArea { get; set; } // in m2
        public string Notes { get; set; } // any additional notes or comments about the space requirement

    }
}
