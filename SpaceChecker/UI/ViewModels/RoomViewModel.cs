using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceChecker.Revit.Models;


namespace SpaceChecker.UI.ViewModels
{
    public class RoomViewModel : ViewModelBase
    {
        private readonly ComparisonResult _result;

        public RoomViewModel(ComparisonResult result) { _result = result; }
        
        public string RoomName => _result.Room?.Name ?? "—";
        public string RoomNumber => _result.Room?.Number ?? "—";
        public string Level => _result.Room?.Level ?? "—";
        public string Department => _result.Room?.Department ?? "—";
        public double ActualArea => _result.Room?.ActualArea ?? 0;
        public double RequiredArea => _result.ProgramEntry?.RequiredArea ?? 0;
        public double Deviation => _result.Deviation;
        public string DeviationText =>$"{_result.DeviationPercent:+0.0;-0.0}%";
        public ComplianceStatus Status => _result.Status;


    }
}
