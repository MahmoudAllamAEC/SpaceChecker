using System.Collections.Generic;
using System.Linq;

namespace SpaceChecker.Revit.Models
{
    /// <summary>One room's result inside an apartment: actual room vs. its spec.</summary>
    public class RoomCheck
    {
        public RoomItem Room { get; set; }       // null if a required room is missing
        public ProgramRoom Spec { get; set; }     // null if the room isn't in the spec (extra)

        public double RequiredArea => Spec?.RequiredArea ?? 0;
        public double ActualArea => Room?.ActualArea ?? 0;
        public double Deviation => ActualArea - RequiredArea;
        public double DeviationPercent => RequiredArea > 0 ? Deviation / RequiredArea * 100 : 0;

        public ComplianceStatus Status { get; set; }
    }

    /// <summary>
    /// One apartment instance's result: the unit (actual) measured against its
    /// type's spec, plus the per-room breakdown for the detail table.
    /// </summary>
    public class ApartmentCheck
    {
        public UnitItem Unit { get; set; }
        public ProgramApartment Spec { get; set; }

        public string Name => Unit?.Name ?? "—";
        public string Type => Unit?.Type ?? "";
        public string LevelsText => Unit != null ? string.Join(", ", Unit.Levels) : "";

        public double RequiredArea => Spec?.RequiredArea ?? 0;
        public double ActualArea => Unit?.ActualArea ?? 0;
        public double Deviation => ActualArea - RequiredArea;
        public double DeviationPercent => RequiredArea > 0 ? Deviation / RequiredArea * 100 : 0;

        public ComplianceStatus Status { get; set; }
        public List<RoomCheck> Rooms { get; set; } = new List<RoomCheck>();

        // The area's own total passed, but one or more rooms inside deviate — a
        // "pass, but worth checking the rooms" state (drives the warning marker).
        public bool HasRoomIssues => Spec != null && Rooms.Any(r => r.Status != ComplianceStatus.Met);
    }
}
