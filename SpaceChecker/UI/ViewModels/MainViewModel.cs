using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using SpaceChecker.Excel.Export;
using SpaceChecker.Excel.Import;
using SpaceChecker.Revit.Extraction;
using SpaceChecker.Revit.Models;
using SpaceChecker.Revit.Services;
using SpaceChecker.UI.Commands;

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
            CreateTemplateCommand = new RelayCommand(DoCreateTemplate);
            RunComparisonCommand = new RelayCommand(DoRunComparison);
            ExportDataCommand = new RelayCommand(DoExportData);

            // Grouped view: one centered header (area name + required) per type.
            var view = new CollectionViewSource { Source = Apartments };
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ApartmentRowVM.GroupHeader)));
            ApartmentsView = view.View;

            // Read-only full extraction on open so the apartment list is ready immediately.
            InitializeFromModel();
        }

        // Grouped-by-type view bound by the master grid.
        public ICollectionView ApartmentsView { get; }

        public ICommand ExtractCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand RunComparisonCommand { get; }
        public ICommand CreateTemplateCommand { get; }
        public ICommand ExportDataCommand { get; }

        // Master list (apartments) + the selected apartment's rooms (detail).
        public ObservableCollection<ApartmentRowVM> Apartments { get; }
            = new ObservableCollection<ApartmentRowVM>();

        private ApartmentRowVM _selectedApartment;
        public ApartmentRowVM SelectedApartment
        {
            get => _selectedApartment;
            set { _selectedApartment = value; OnPropertyChanged(); }
        }

        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Acceptable area deviation entered by the user as a percentage (5 == ±5%).
        private double _acceptableDeviationPercent = 5;
        public double AcceptableDeviationPercent
        {
            get => _acceptableDeviationPercent;
            set { _acceptableDeviationPercent = value; OnPropertyChanged(); }
        }

        private int ApartmentCount =>
            _service.Units?.Count(u => u.Name != RoomUnitMapper.Unassigned) ?? 0;

        private void InitializeFromModel()
        {
            try
            {
                ExtractModel();
                StatusMessage = $"{ApartmentCount} area(s), {_service.ExtractedRooms.Count} room(s) found";
            }
            catch (Exception ex)
            {
                StatusMessage = "Could not read the model on open: " + ex.Message;
            }
        }

        // Full read-only extraction: rooms + apartment Areas, then spatial mapping.
        private void ExtractModel()
        {
            var rooms = RoomExtractor.Extract(_service.Doc);
            var areas = AreaExtractor.Extract(_service.Doc);
            var units = RoomUnitMapper.Build(rooms, areas);

            _service.ExtractedRooms = rooms;
            _service.ExtractedAreas = areas;
            _service.Units = units;
        }

        private void DoExtract()
        {
            // Re-extract wipes the (now stale) results.
            Apartments.Clear();
            SelectedApartment = null;
            try
            {
                ExtractModel();
                StatusMessage = $"{ApartmentCount} area(s) extracted ({_service.ExtractedRooms.Count} rooms)";
            }
            catch (Exception ex)
            {
                StatusMessage = "Extraction failed: " + ex.Message;
            }
        }

        private void DoImport()
        {
            var dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                _service.Program = SpaceProgramImporter.Import(dlg.FileName);
                int rooms = _service.Program.Sum(a => a.Rooms.Count);
                StatusMessage = $"Program loaded: {_service.Program.Count} area(s), {rooms} room(s)";
            }
            catch (Exception ex)
            {
                StatusMessage = "Import failed: " + ex.Message;
            }
        }

        private void DoCreateTemplate()
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "SpaceProgramTemplate.xlsx"
            };
            if (dlg.ShowDialog() != true) return;
            try
            {
                ProgramTemplateExporter.Create(dlg.FileName);
                StatusMessage = "Template created — fill it in, then Import Program.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Could not create template: " + ex.Message;
            }
        }

        private void DoRunComparison()
        {
            if (_service.Units == null || !_service.Units.Any())
            {
                StatusMessage = "Extract the model first.";
                return;
            }
            if (_service.Program == null || !_service.Program.Any())
            {
                StatusMessage = "Import a space program first.";
                return;
            }

            try
            {
                double tolerance = AcceptableDeviationPercent / 100.0;
                var results = new SpaceProgramComparer()
                                  .CompareUnits(_service.Units, _service.Program, tolerance);
                _service.ApartmentResults = results;

                Apartments.Clear();
                // Number each instance within its type (1st, 2nd, 3rd "3B_B"…).
                var perType = new Dictionary<string, int>();
                foreach (var r in results)
                {
                    string t = r.Type ?? "";
                    perType.TryGetValue(t, out int n);
                    perType[t] = ++n;
                    Apartments.Add(new ApartmentRowVM(r, n));
                }
                SelectedApartment = Apartments.FirstOrDefault();

                int passed = results.Count(r => r.Status == ComplianceStatus.Met);
                StatusMessage = $"Done — {passed}/{results.Count} area(s) met target";
            }
            catch (Exception ex)
            {
                StatusMessage = "Comparison failed: " + ex.Message;
            }
        }

        // Exports the extracted apartments + rooms (actual areas) into the import
        // template layout, so the model can be dumped to Excel and reused.
        private void DoExportData()
        {
            if (_service.Units == null || !_service.Units.Any())
            {
                StatusMessage = "Extract the model first.";
                return;
            }

            var dlg = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "SpaceChecker_ProjectData.xlsx"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                ProjectDataExporter.Export(dlg.FileName, _service.Units);
                StatusMessage = $"Project data exported — {_service.Units.Count} area(s).";
            }
            catch (Exception ex)
            {
                // The most common cause is the target file already being open in Excel.
                StatusMessage = "Export failed (is the file open in Excel? close it and retry): " + ex.Message;
            }
        }
    }
}
