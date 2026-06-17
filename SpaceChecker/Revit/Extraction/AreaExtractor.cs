using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using SpaceChecker.Revit.Models;

namespace SpaceChecker.Revit.Extraction
{
    /// <summary>
    /// Reads the Revit Area elements that represent apartment boundaries. Each
    /// placed, enclosed Area becomes an <see cref="AreaItem"/> carrying its name,
    /// parsed type, total area and tessellated boundary polygon (for the
    /// room-to-apartment containment test in <see cref="RoomUnitMapper"/>).
    /// </summary>
    internal static class AreaExtractor
    {
        public static List<AreaItem> Extract(Document doc)
        {
            var opts = new SpatialElementBoundaryOptions();

            var areas = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType()
                .Cast<Area>()
                .Where(a => a.Area > 0); // skip unplaced / not-enclosed areas

            var result = new List<AreaItem>();

            foreach (var area in areas)
            {
                var loops = BuildLoops(area, opts);
                if (loops.Count == 0) continue; // no usable boundary -> can't contain rooms

                string name = GetAreaName(area);
                double areaM2 = QuantityUtils.ToSquareMeters(area.Area);

                result.Add(new AreaItem
                {
                    Name = name,
                    Type = ParseType(name),
                    Number = area.get_Parameter(BuiltInParameter.ROOM_NUMBER)?.AsString() ?? "",
                    ActualArea = Math.Round(areaM2, 2),
                    Level = (doc.GetElement(area.LevelId) as Level)?.Name ?? "",
                    LevelId = area.LevelId,
                    Loops = loops,
                    RevitElement = area
                });
            }

            return result;
        }

        /// <summary>
        /// Apartment type = the part before the FIRST dash, e.g. "typeB-4" -> "typeB",
        /// "A-1-101" -> "A". A name with no dash is its own type.
        /// </summary>
        public static string ParseType(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            int dash = fullName.IndexOf('-');
            return (dash > 0 ? fullName.Substring(0, dash) : fullName).Trim();
        }

        // Area name lives on the ROOM_NAME spatial parameter; fall back to the
        // element name. (Confirm against a real model when one is available.)
        private static string GetAreaName(Area area)
        {
            string name = area.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
            if (string.IsNullOrWhiteSpace(name)) name = area.Name;
            return (name ?? "").Trim();
        }

        // Builds tessellated XY polygons (feet) for every boundary loop of the area.
        private static List<List<UV>> BuildLoops(Area area, SpatialElementBoundaryOptions opts)
        {
            var loops = new List<List<UV>>();
            var boundary = area.GetBoundarySegments(opts);
            if (boundary == null) return loops;

            foreach (var loop in boundary)
            {
                var pts = new List<UV>();
                foreach (var seg in loop)
                {
                    // Tessellate so arcs/splines become line segments we can test.
                    foreach (var p in seg.GetCurve().Tessellate())
                        pts.Add(new UV(p.X, p.Y));
                }
                if (pts.Count >= 3) loops.Add(pts);
            }

            return loops;
        }
    }
}
