using System.Diagnostics;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;

namespace SpaceChecker.Excel.Export
{
    /// <summary>
    /// Writes a clean, ready-to-fill import template: a bold header row and two
    /// example area blocks that show the fill-down convention (AreaName +
    /// RequiredAreaTotal + Levels only on a block's first row). Uses
    /// ProgramSheetSchema so the generated file matches what the importer expects.
    /// </summary>
    internal static class ProgramTemplateExporter
    {
        public static void Create(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add(ProgramSheetSchema.SheetName);

                // --- Header row ---
                ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.RoomNameCol].Value = ProgramSheetSchema.RoomNameHeader;
                ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.RequiredRoomAreaCol].Value = ProgramSheetSchema.RequiredRoomAreaHeader;
                ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.LevelCol].Value = ProgramSheetSchema.LevelHeader;
                ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.ApartmentCol].Value = ProgramSheetSchema.ApartmentHeader;
                ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.RequiredAptAreaCol].Value = ProgramSheetSchema.RequiredAptAreaHeader;

                var header = ws.Cells[ProgramSheetSchema.HeaderRow, ProgramSheetSchema.ApartmentCol,
                                      ProgramSheetSchema.HeaderRow, ProgramSheetSchema.RequiredRoomAreaCol];
                header.Style.Font.Bold = true;

                // --- Two example area blocks ---
                // Area name + total + levels go on each block's FIRST row; rooms
                // follow. The second block is a duplex spanning two levels.
                // NOTE: the template is left UN-merged on purpose — the user types
                // into it and may add rows inside a block, which merged cells break.
                int r = ProgramSheetSchema.FirstDataRow;

                WriteArea(ws, r, "3B", 49, "Level 1");
                WriteRoom(ws, r++, "Master Bedroom", 18);
                WriteRoom(ws, r++, "Kitchen", 9);
                WriteRoom(ws, r++, "Living Room", 22);

                WriteArea(ws, r, "Duplex_A", 29, "Level 1, Level 2");
                WriteRoom(ws, r++, "Master Bedroom", 16);
                WriteRoom(ws, r++, "Bedroom 1", 13);

                // --- Number formats + numbers-only validation on the area columns ---
                ws.Column(ProgramSheetSchema.RequiredRoomAreaCol).Style.Numberformat.Format = "0.00";
                ws.Column(ProgramSheetSchema.RequiredAptAreaCol).Style.Numberformat.Format = "0.00";

                // Data validation is a nicety. Some EPPlus builds (e.g. a different
                // version already loaded in the host) reject this API — never let it
                // abort template creation.
                try
                {
                    AddNonNegativeValidation(ws, ProgramSheetSchema.RequiredRoomAreaCol);
                    AddNonNegativeValidation(ws, ProgramSheetSchema.RequiredAptAreaCol);
                }
                catch { /* skip validation; the template is still valid */ }

                // --- Cosmetics ---
                ws.Column(ProgramSheetSchema.RoomNameCol).Width = 22;
                ws.Column(ProgramSheetSchema.RequiredRoomAreaCol).Width = 18;
                ws.Column(ProgramSheetSchema.LevelCol).Width = 12;
                ws.Column(ProgramSheetSchema.ApartmentCol).Width = 14;
                ws.Column(ProgramSheetSchema.RequiredAptAreaCol).Width = 16;
                ws.View.FreezePanes(ProgramSheetSchema.FirstDataRow, 1);

                pkg.SaveAs(new FileInfo(filePath));
            }

            // Best-effort open — file is already saved.
            try { Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); }
            catch { /* ignore */ }
        }

        // Area header cells (first row of a block): name, total, levels.
        private static void WriteArea(OfficeOpenXml.ExcelWorksheet ws, int row, string name, double total, string levels)
        {
            ws.Cells[row, ProgramSheetSchema.ApartmentCol].Value = name;
            ws.Cells[row, ProgramSheetSchema.RequiredAptAreaCol].Value = total;
            ws.Cells[row, ProgramSheetSchema.LevelCol].Value = levels;
        }

        // Room cells: name + required area.
        private static void WriteRoom(OfficeOpenXml.ExcelWorksheet ws, int row, string roomName, double roomArea)
        {
            ws.Cells[row, ProgramSheetSchema.RoomNameCol].Value = roomName;
            ws.Cells[row, ProgramSheetSchema.RequiredRoomAreaCol].Value = roomArea;
        }

        private static void AddNonNegativeValidation(OfficeOpenXml.ExcelWorksheet ws, int col)
        {
            string range = ExcelCellBase.GetAddress(ProgramSheetSchema.FirstDataRow, col, 1000, col);
            var dv = ws.DataValidations.AddDecimalValidation(range);
            dv.AllowBlank = true;
            dv.ShowErrorMessage = true;
            dv.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            dv.ErrorTitle = "Invalid area";
            dv.Error = "Area must be a number (m²).";
            dv.Operator = ExcelDataValidationOperator.greaterThanOrEqual;
            dv.Formula.Value = 0;
        }
    }
}
