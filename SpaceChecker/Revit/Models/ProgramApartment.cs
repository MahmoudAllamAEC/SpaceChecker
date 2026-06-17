using System.Collections.Generic;

namespace SpaceChecker.Revit.Models
{
    /// <summary>
    /// One block in the imported Excel = the spec for a single apartment TYPE.
    /// Every Revit apartment instance of this type is checked against it.
    /// </summary>
    public class ProgramApartment
    {
        public string Name { get; set; }   // area name = the type, e.g. "3B_B"
        public string Type { get; set; }    // parsed type (prefix before a dash, else the whole name)
        public double RequiredArea { get; set; } // required total area for the area/type (m2)
        public string Levels { get; set; }  // levels the area spans (descriptive)
        public List<ProgramRoom> Rooms { get; set; } = new List<ProgramRoom>();
    }

    /// <summary>One expected room within an area type.</summary>
    public class ProgramRoom
    {
        public string RoomName { get; set; }
        public double RequiredArea { get; set; } // required room area (m2)
    }
}
