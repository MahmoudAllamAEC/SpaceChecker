//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SpaceChecker.Revit.Models
//{
//    public enum ComparisonStatus
//    {
//        Met,// the actual area meets the required area
//        Over,// the actual area exceeds the required area
//        Under,// the actual area is less than the required area
//        Unmatched// the actual area does not match the required area
//    }
//    public class ComparisonResult
//    {
//        public RoomItem Room { get; set; }
//        public SpaceProgramEntry ProgramEntry { get; set; }

//        public double Deviation => Room != null && ProgramEntry != null ? Room.ActualArea - ProgramEntry.RequiredArea : 0;

//        public double DeviationPercentage => ProgramEntry?.RequiredArea > 0 ? (Deviation / ProgramEntry.RequiredArea) * 100 : 0;

//        public ComparisonStatus Status { get; set; }
//    }
//}
