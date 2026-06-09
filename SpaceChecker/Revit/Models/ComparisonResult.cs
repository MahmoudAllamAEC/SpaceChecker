using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceChecker.Revit.Models
{
    public enum ComplianceStatus { Met, Over, Under, Unmatched }

    public class ComparisonResult
    {
        public RoomItem Room { get; set; }
        public SpaceProgramEntry ProgramEntry { get; set; }

        public double Deviation => Room != null && ProgramEntry != null
                                  ? Room.ActualArea - ProgramEntry.RequiredArea : 0;

        public double DeviationPercent => ProgramEntry?.RequiredArea > 0
                                  ? (Deviation / ProgramEntry.RequiredArea) * 100 : 0;

        public ComplianceStatus Status { get; set; }
    }
}
