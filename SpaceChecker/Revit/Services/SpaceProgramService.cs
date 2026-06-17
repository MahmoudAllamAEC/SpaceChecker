using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Events;
using SpaceChecker.Revit.Models;

namespace SpaceChecker.Revit.Services
{
    //stores all data in this class to be used across the process and avoid passing data between methods, also allows to keep reference to the original document and elements if needed
    public class SpaceProgramService
    {
        // the active Revit Document
        public Document Doc { get; set; }
        //list of all extracted rooms with its properties
        public List<RoomItem> ExtractedRooms { get; set; } = new List<RoomItem>();
        //extracted apartment Areas and the apartment Units they form (rooms mapped in)
        public List<AreaItem> ExtractedAreas { get; set; } = new List<AreaItem>();
        public List<UnitItem> Units { get; set; } = new List<UnitItem>();
        //imported program, keyed by apartment type
        public List<ProgramApartment> Program { get; set; } = new List<ProgramApartment>();
        //apartment-level comparison results (drive the master-detail UI)
        public List<ApartmentCheck> ApartmentResults { get; set; } = new List<ApartmentCheck>();

        //this method deletes all list and data to be called at the end of the process as a backup plan
        public void Reset()
        {
            ExtractedRooms.Clear();
            ExtractedAreas.Clear();
            Units.Clear();
            Program.Clear();
            ApartmentResults.Clear();
        }
    }
}
