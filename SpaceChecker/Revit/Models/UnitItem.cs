using System.Collections.Generic;

namespace SpaceChecker.Revit.Models
{
    /// <summary>
    /// A physical apartment: every Area element sharing the same <see cref="Name"/>
    /// merged together, plus the rooms that fall inside them. May span multiple
    /// levels (a duplex). Each unit is evaluated on its own against its Type's
    /// program spec.
    /// </summary>
    public class UnitItem
    {
        public string Name { get; set; }   // "typeB-4"
        public string Type { get; set; }    // "typeB"

        // Sum of the unit's Area elements (m2) — the apartment's total area as
        // measured in Revit. For a duplex this adds up both levels.
        public double ActualArea { get; set; }

        public List<string> Levels { get; set; } = new List<string>();
        public List<RoomItem> Rooms { get; set; } = new List<RoomItem>();
    }
}
