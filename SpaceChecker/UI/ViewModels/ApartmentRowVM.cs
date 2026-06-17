using System.Collections.ObjectModel;
using System.Windows.Media;
using SpaceChecker.Revit.Models;

namespace SpaceChecker.UI.ViewModels
{
    /// <summary>Master-row view model: one apartment instance and its room breakdown.</summary>
    public class ApartmentRowVM : ViewModelBase
    {
        private readonly ApartmentCheck _check;

        public ApartmentRowVM(ApartmentCheck check, int instanceIndex)
        {
            _check = check;
            InstanceIndex = instanceIndex;
            foreach (var r in check.Rooms) Rooms.Add(new RoomCheckVM(r));
        }

        // 1-based index of this instance WITHIN its type (1st, 2nd, 3rd "3B_B"…).
        public int InstanceIndex { get; }

        public string Name => _check.Name;
        public string AreaType => string.IsNullOrEmpty(_check.Type) ? "—" : _check.Type;

        // Group key for the master grid: all instances of a type share it, so the
        // grid shows one centered header (area name + required) per type.
        public string GroupHeader => string.IsNullOrEmpty(_check.Type)
            ? "Unassigned"
            : $"{AreaType}      ·      Required: {RequiredAreaText} m²";

        // Identifies the specific instance, e.g. "3B_B #1 — 01-Ground Floor F.F.L".
        // Areas with no type (the Unassigned group) just show their name.
        public string InstanceLabel
        {
            get
            {
                if (string.IsNullOrEmpty(_check.Type)) return _check.Name;
                string levels = string.IsNullOrEmpty(_check.LevelsText) ? "" : $" — {_check.LevelsText}";
                return $"{_check.Type} #{InstanceIndex}{levels}";
            }
        }

        public string LevelsText => string.IsNullOrEmpty(_check.LevelsText) ? "—" : _check.LevelsText;
        public double RequiredArea => _check.RequiredArea;
        public double ActualArea => _check.ActualArea;
        // No matching type spec -> there is no required area to show.
        public string RequiredAreaText => _check.Spec == null ? "Unassigned" : $"{_check.RequiredArea:N2}";
        public string DeviationText => _check.Spec != null ? $"{_check.DeviationPercent:+0.0;-0.0}%" : "—";
        public ComplianceStatus Status => _check.Status;

        // The area passes on its own total. If it passed but some rooms deviate,
        // add a ⚠ marker prompting the user to open and check the rooms.
        public string PassFail
        {
            get
            {
                if (_check.Status != ComplianceStatus.Met) return "❌";
                return _check.HasRoomIssues ? "✅ ⚠" : "✅";
            }
        }

        public string PassFailTip => _check.HasRoomIssues && _check.Status == ComplianceStatus.Met
            ? "Area total is within tolerance, but some rooms deviate — open to check."
            : null;

        // Row colour: green = clean pass, amber = pass but rooms deviate, red = fail.
        public Brush RowBackground
        {
            get
            {
                if (_check.Status != ComplianceStatus.Met) return new SolidColorBrush(Color.FromRgb(254, 226, 226)); // red
                if (_check.HasRoomIssues) return new SolidColorBrush(Color.FromRgb(254, 243, 199));                  // amber
                return new SolidColorBrush(Color.FromRgb(209, 250, 229));                                            // green
            }
        }

        public ObservableCollection<RoomCheckVM> Rooms { get; } = new ObservableCollection<RoomCheckVM>();
    }
}
