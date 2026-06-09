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
        //list of all space inside the excel Sheet entries with its properties
        public List<SpaceProgramEntry> ProgramEntries { get; set; } = new List<SpaceProgramEntry>();
        //list if each rescult of deviation between  ExtractedRooms and ProgramEntries
        public List<ComparisonResult> Results { get; set; } = new List<ComparisonResult>();

        //this method deletes all list and data to be called at the end of the process as a backup plan
        public void Reset()
        {
            ExtractedRooms.Clear();
            ProgramEntries.Clear();
            Results.Clear();
        }
    }
}
