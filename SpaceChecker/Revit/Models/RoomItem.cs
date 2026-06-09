using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace SpaceChecker.Revit.Models
{
    public class RoomItem
    {
        public string Name { get; set; }
        public string Number { get; set; }
        public string Department { get; set; }
        public string Level { get; set; }
        public double ActualArea { get; set; } // in m2
        public Element RevitElement { get; set; } // keep reference to the original Revit element for potential future use

    }
}
