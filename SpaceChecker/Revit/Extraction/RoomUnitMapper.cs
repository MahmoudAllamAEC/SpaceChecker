using System;

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using SpaceChecker.Revit.Models;

namespace SpaceChecker.Revit.Extraction
{
    /// <summary>
    /// Maps each room to the apartment Area whose boundary encloses it (a
    /// point-in-polygon test on the same level), then groups Areas into
    /// <see cref="UnitItem"/> apartments:
    ///   - same name across DIFFERENT levels  -> one unit (a duplex; Levels = "1, 2")
    ///   - same name twice on the SAME level   -> separate units (can't be merged)
    /// Rooms outside every Area collect under a synthetic "Unassigned" unit.
    /// Type-matching against the Excel spec happens later in the comparer.
    /// </summary>
    internal static class RoomUnitMapper
    {
        public const string Unassigned = "Unassigned";

        public static List<UnitItem> Build(List<RoomItem> rooms, List<AreaItem> areas)
        {
            rooms = rooms ?? new List<RoomItem>();
            areas = areas ?? new List<AreaItem>();

            // Test each room only against areas on its own level (duplex halves
            // live on separate levels and merge later by name).
            var byLevel = areas
                .GroupBy(a => a.LevelId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Remember which specific area each room landed in, so split units
            // (same name, same level) still get the right rooms.
            var roomsByArea = new Dictionary<AreaItem, List<RoomItem>>();

            foreach (var room in rooms)
            {
                var pt = (room.RevitElement?.Location as LocationPoint)?.Point;
                var levelId = (room.RevitElement as SpatialElement)?.LevelId ?? ElementId.InvalidElementId;

                AreaItem hit = null;
                if (pt != null && byLevel.TryGetValue(levelId, out var candidates))
                    hit = candidates.FirstOrDefault(a => Contains(a.Loops, pt.X, pt.Y));

                if (hit != null)
                {
                    room.Unit = hit.Name;
                    room.Type = hit.Type;
                    if (!roomsByArea.TryGetValue(hit, out var list))
                        roomsByArea[hit] = list = new List<RoomItem>();
                    list.Add(room);
                }
                else
                {
                    room.Unit = Unassigned;
                    room.Type = "";
                }
            }

            List<RoomItem> RoomsOf(IEnumerable<AreaItem> aa) =>
                aa.SelectMany(a => roomsByArea.TryGetValue(a, out var l) ? l : Enumerable.Empty<RoomItem>())
                  .ToList();

            UnitItem BuildUnit(string name, List<AreaItem> aa) => new UnitItem
            {
                Name = name,
                Type = aa[0].Type,
                ActualArea = Math.Round(aa.Sum(a => a.ActualArea), 2),
                Levels = aa.Select(a => a.Level)
                           .Where(l => !string.IsNullOrWhiteSpace(l))
                           .Distinct()
                           .ToList(),
                Rooms = RoomsOf(aa)
            };

            var units = new List<UnitItem>();

            foreach (var grp in areas.GroupBy(a => a.Name))
            {
                bool collidesOnLevel = grp.GroupBy(a => a.LevelId).Any(g => g.Count() > 1);

                if (!collidesOnLevel)
                {
                    // One apartment — single-level or a duplex spanning levels.
                    units.Add(BuildUnit(grp.Key, grp.ToList()));
                }
                else
                {
                    // Same name more than once on a level: can't safely pair, so
                    // each area is its own instance, labelled by Number (or index).
                    int i = 1;
                    foreach (var a in grp)
                    {
                        string suffix = string.IsNullOrWhiteSpace(a.Number) ? "#" + i++ : "#" + a.Number;
                        units.Add(BuildUnit(grp.Key + " " + suffix, new List<AreaItem> { a }));
                    }
                }
            }

            // Orphan rooms (corridors, shafts, anything outside every boundary).
            var orphans = rooms.Where(r => r.Unit == Unassigned).ToList();
            if (orphans.Count > 0)
                units.Add(new UnitItem { Name = Unassigned, Type = "", Rooms = orphans });

            return units;
        }

        /// <summary>
        /// Even-odd ray-cast point-in-polygon across all boundary loops, so holes
        /// (inner loops) correctly read as "outside". Coordinates are feet.
        /// </summary>
        private static bool Contains(List<List<UV>> loops, double x, double y)
        {
            bool inside = false;
            foreach (var loop in loops)
            {
                int n = loop.Count;
                for (int i = 0, j = n - 1; i < n; j = i++)
                {
                    double xi = loop[i].U, yi = loop[i].V;
                    double xj = loop[j].U, yj = loop[j].V;

                    bool crosses = ((yi > y) != (yj > y)) &&
                                   (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
                    if (crosses) inside = !inside;
                }
            }
            return inside;
        }
    }
}
