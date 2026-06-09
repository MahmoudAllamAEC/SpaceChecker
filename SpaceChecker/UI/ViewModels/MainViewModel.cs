using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using SpaceChecker.Excel.Export;
using SpaceChecker.Revit.Extraction;
using SpaceChecker.Revit.Services;
using SpaceChecker.UI.Commands;
using SpaceChecker.Revit.Models;
using SpaceChecker.Excel.Import;




namespace SpaceChecker.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly SpaceProgramService _service;

        public MainViewModel(SpaceProgramService service)
        {
            _service = service;
            ExtractCommand = new RelayCommand(DoExtract);
            ImportCommand = new RelayCommand(DoImport);
            RunComparisonCommand = new RelayCommand(DoRunComparison);
            ExportCommand = new RelayCommand(DoExport, () => Results.Any());
        }

        public ICommand ExtractCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand RunComparisonCommand { get; }
        public ICommand ExportCommand { get; }

        public ObservableCollection<RoomViewModel> Results { get; }
            = new ObservableCollection<RoomViewModel>();

        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private void DoExtract()
        {
            var rooms = RoomExtractor.Extract(_service.Doc);
            _service.ExtractedRooms = rooms;
            StatusMessage = $"{rooms.Count} rooms extracted";
        }

        private void DoImport()
        {
            var dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx" };
            if (dlg.ShowDialog() != true) return;
            _service.ProgramEntries = SpaceProgramImporter.Import(dlg.FileName);
            StatusMessage = $"Program loaded: {_service.ProgramEntries.Count} entries";
        }

        private void DoRunComparison()
        {
            var results = new SpaceProgramComparer()
                              .Compare(_service.ExtractedRooms, _service.ProgramEntries);
            _service.Results = results;
            Results.Clear();
            foreach (var r in results) Results.Add(new RoomViewModel(r));
            StatusMessage = $"Done — {results.Count(r => r.Status == ComparisonStatus.Met)}/{results.Count} rooms met target";
        }

        private void DoExport()
        {
            var dlg = new SaveFileDialog { Filter = "Excel Files|*.xlsx" };
            if (dlg.ShowDialog() != true) return;
            ComparisonExporter.Export(_service.Results, dlg.FileName);
            StatusMessage = "Export complete";
        }


    }
}
