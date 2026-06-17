using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using SpaceChecker.Excel;
using SpaceChecker.Revit.Extraction;
using SpaceChecker.Revit.Models;

namespace SpaceChecker.Excel.Import
{
    /// <summary>
    /// Reads the space-program sheet into nested apartment specs. Walks rows
    /// top-to-bottom: a non-empty Apartment cell starts a new block (apartment
    /// type + required area); every row with a room name adds a room to the
    /// current block (blank Apartment cell = fill-down, "same as above").
    /// </summary>
    internal static class SpaceProgramImporter
    {
        public static List<ProgramApartment> Import(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var apartments = new List<ProgramApartment>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = package.Workbook.Worksheets[0];
                int lastRow = sheet.Dimension?.End.Row ?? 0;

                ProgramApartment current = null;

                for (int row = ProgramSheetSchema.FirstDataRow; row <= lastRow; row++)
                {
                    string aptName = sheet.Cells[row, ProgramSheetSchema.ApartmentCol].Value?.ToString()?.Trim() ?? "";
                    string roomName = sheet.Cells[row, ProgramSheetSchema.RoomNameCol].Value?.ToString()?.Trim() ?? "";

                    // A filled AreaName cell opens a new area block.
                    if (!string.IsNullOrWhiteSpace(aptName))
                    {
                        double.TryParse(sheet.Cells[row, ProgramSheetSchema.RequiredAptAreaCol].Value?.ToString(), out double aptArea);
                        string levels = sheet.Cells[row, ProgramSheetSchema.LevelCol].Value?.ToString()?.Trim() ?? "";
                        current = new ProgramApartment
                        {
                            Name = aptName,
                            Type = AreaExtractor.ParseType(aptName),
                            RequiredArea = Math.Round(aptArea, 2),
                            Levels = levels
                        };
                        apartments.Add(current);
                    }

                    if (string.IsNullOrWhiteSpace(roomName)) continue; // spacer / empty row
                    if (current == null) continue;                    // rooms before any area header

                    double.TryParse(sheet.Cells[row, ProgramSheetSchema.RequiredRoomAreaCol].Value?.ToString(), out double roomArea);

                    current.Rooms.Add(new ProgramRoom
                    {
                        RoomName = roomName,
                        RequiredArea = Math.Round(roomArea, 2)
                    });
                }
            }

            return apartments;
        }
    }
}
