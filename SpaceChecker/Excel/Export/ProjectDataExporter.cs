using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SpaceChecker.Revit.Models;

namespace SpaceChecker.Excel.Export
{
    /// <summary>
    /// Exports the extracted project (apartment Units + their rooms) into the
    /// SAME layout as the import template — so the model can be dumped to Excel
    /// and re-imported. Area columns use fill-down + vertical merge per block.
    /// </summary>
    internal static class ProjectDataExporter
    {
        public static void Export(string filePath, List<UnitItem> units)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add(ProgramSheetSchema.SheetName);
                WriteHeader(ws);

                int row = ProgramSheetSchema.FirstDataRow;
                foreach (var unit in units ?? new List<UnitItem>())
                {
                    int startRow = row;

                    if (unit.Rooms == null || unit.Rooms.Count == 0)
                    {
                        // Area with no rooms still emits a header row.
                        WriteAptCells(ws, row, unit);
                        row++;
                    }
                    else
                    {
                        bool first = true;
                        foreach (var rm in unit.Rooms)
                        {
                            ws.Cells[row, ProgramSheetSchema.RoomNameCol].Value = rm.Name;
                            ws.Cells[row, ProgramSheetSchema.RequiredRoomAreaCol].Value = rm.ActualArea;

                            if (first) { WriteAptCells(ws, row, unit); first = false; }
                            row++;
                        }
                    }

                    int endRow = row - 1;
                    MergeAreaCells(ws, startRow, endRow);
                }

                ws.Column(ProgramSheetSchema.RequiredRoomAreaCol).Style.Numberformat.Format = "0.00";
                ws.Column(ProgramSheetSchema.RequiredAptAreaCol).Style.Numberformat.Format = "0.00";

                // Auto-fit; merged area columns are ignored by AutoFit, so give them a minimum.
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                if (ws.Column(ProgramSheetSchema.ApartmentCol).Width < 18) ws.Column(ProgramSheetSchema.ApartmentCol).Width = 18;
                if (ws.Column(ProgramSheetSchema.LevelCol).Width < 22) ws.Column(ProgramSheetSchema.LevelCol).Width = 22;

                ws.View.FreezePanes(ProgramSheetSchema.FirstDataRow, 1);

                pkg.SaveAs(new FileInfo(filePath));
            }

            // Saved OK; opening it is best-effort.
            try { Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); }
            catch { /* ignore */ }
        }

        // Vertically merge + center the area columns (name, total, levels) across a block.
        private static void MergeAreaCells(ExcelWorksheet ws, int startRow, int endRow)
        {
            if (endRow < startRow) return;
            foreach (int col in new[] { ProgramSheetSchema.ApartmentCol,
                                        ProgramSheetSchema.RequiredAptAreaCol,
                                        ProgramSheetSchema.LevelCol })
            {
                var range = ws.Cells[startRow, col, endRow, col];
                if (endRow > startRow) range.Merge = true;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.WrapText = true;
            }
        }

        private static void WriteAptCells(ExcelWorksheet ws, int row, UnitItem unit)
        {
            ws.Cells[row, ProgramSheetSchema.ApartmentCol].Value = unit.Name;
            // Leave the total-area cell blank when we don't have a real area
            // (e.g. the synthetic "Unassigned" group).
            if (unit.ActualArea > 0)
                ws.Cells[row, ProgramSheetSchema.RequiredAptAreaCol].Value = unit.ActualArea;
            ws.Cells[row, ProgramSheetSchema.LevelCol].Value =
                string.Join(", ", unit.Levels ?? new List<string>());
        }

        private static void WriteHeader(ExcelWorksheet ws)
        {
            ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.RoomNameCol].Value = ProgramSheetSchema.RoomNameHeader;
            ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.RequiredRoomAreaCol].Value = ProgramSheetSchema.RequiredRoomAreaHeader;
            ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.LevelCol].Value = ProgramSheetSchema.LevelHeader;
            ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.ApartmentCol].Value = ProgramSheetSchema.ApartmentHeader;
            ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.RequiredAptAreaCol].Value = ProgramSheetSchema.RequiredAptAreaHeader;

            var header = ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.ApartmentCol,
                                  ProgramSheetSchema.HeaderRow, ProgramSheetSchema.RequiredRoomAreaCol];
            header.Style.Font.Bold = true;
        }
    }
}
