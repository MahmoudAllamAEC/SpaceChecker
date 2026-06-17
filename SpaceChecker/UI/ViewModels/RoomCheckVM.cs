using SpaceChecker.Revit.Models;

namespace SpaceChecker.UI.ViewModels
{
    /// <summary>Detail-row view model: one room's result within the selected apartment.</summary>
    public class RoomCheckVM : ViewModelBase
    {
        private readonly RoomCheck _check;

        public RoomCheckVM(RoomCheck check) { _check = check; }

        public string RoomName => _check.Room?.Name ?? _check.Spec?.RoomName ?? "—";
        public double RequiredArea => _check.RequiredArea;
        public double ActualArea => _check.ActualArea;
        // No spec row for this room (an extra room) -> nothing was required.
        public string RequiredAreaText => _check.Spec == null ? "Unassigned" : $"{_check.RequiredArea:N2}";
        public string DeviationText => (_check.Spec != null && _check.Room != null)
                                        ? $"{_check.DeviationPercent:+0.0;-0.0}%" : "—";
        public ComplianceStatus Status => _check.Status;
        public string PassFail => _check.Status == ComplianceStatus.Met ? "✅" : "❌";
    }
}
