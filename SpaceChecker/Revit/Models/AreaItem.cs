using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace SpaceChecker.Revit.Models
{
    /// <summary>
    /// One Revit Area element — an apartment boundary on a single level. Several
    /// AreaItems that share the same <see cref="Name"/> form a single apartment
    /// Unit (e.g. a duplex whose boundary is drawn on two levels).
    /// </summary>
    public class AreaItem
    {
        public string Name { get; set; }       // full area name, e.g. "typeB-4"
        public string Type { get; set; }        // prefix before the first dash, e.g. "typeB"
        public string Number { get; set; }
        public double ActualArea { get; set; }  // in m2
        public string Level { get; set; }
        public ElementId LevelId { get; set; }

        // Tessellated XY boundary loops in Revit internal units (feet). Used for
        // the point-in-polygon test that maps rooms to this area.
        public List<List<UV>> Loops { get; set; } = new List<List<UV>>();

        public Element RevitElement { get; set; }
    }
}
