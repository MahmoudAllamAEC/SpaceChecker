using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpaceChecker.Revit.Models;

namespace SpaceChecker.Revit.Extraction
{
    internal static class RoomExtractor

    {
        public static List<RoomItem> Extract(Document doc)
        {
            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<SpatialElement>()
                .Where(r => r.Area > 0); // filter out rooms with zero area

            var result = new List<RoomItem>();

            foreach (var room in rooms)
            {
                double areaM2 = QuantityUtils.ToSquareMeters(room.get_Parameter(BuiltInParameter.ROOM_AREA)?.AsDouble() ?? 0);

                result.Add(new RoomItem
                {
                    Name = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString() ?? "", 
                    Number = room.get_Parameter(BuiltInParameter.ROOM_NUMBER)?.AsString() ?? "",
                    Department = room.LookupParameter("Department")?.AsString() ?? "", // custom parameter, may not exist in all models
                    Level = (doc.GetElement(room.LevelId) as Level)?.Name ?? "",
                    ActualArea = Math.Round(areaM2, 2), // round to 2 decimal places
                    RevitElement = room
                });
            }

            return result;
        }
    }
}
