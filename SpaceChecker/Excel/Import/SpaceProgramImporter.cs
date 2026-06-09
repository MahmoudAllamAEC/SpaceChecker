using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using SpaceChecker.Revit.Models;


namespace SpaceChecker.Excel.Import
{
    internal static class SpaceProgramImporter
    {
        public static List<SpaceProgramEntry> Import (string filePath)
        {
            var entries = new List<SpaceProgramEntry>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = package.Workbook.Worksheets[0];
                int lastRow = sheet.Dimension?.End.Row ?? 0;

                for (int row = 2; row <= lastRow; row++) //row 1 = header
                {
                    string roomName = sheet.Cells[row, 1].Value?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(roomName)) continue; // skip empty rows

                    double.TryParse(sheet.Cells[row, 2].Value?.ToString(), out double reqArea);
                    string dept = sheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";

                    entries.Add(new SpaceProgramEntry
                    {
                        RoomName = roomName,
                        RequiredArea = Math.Round(reqArea, 2),
                        Department = dept
                    });

                }
            }
            return entries;
        }
    }
}
