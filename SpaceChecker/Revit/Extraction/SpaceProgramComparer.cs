using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpaceChecker.Revit.Models;

namespace SpaceChecker.Revit.Extraction
{
    internal class SpaceProgramComparer
    {
        // ===== CHANGED: summary rewritten to document the full pipeline =====
        // (previously only mentioned lowercase / trim / remove-spaces / zeros,
        //  omitting separator conversion, abbreviation expansion and marker removal)
        ///<summary>
        ///Builds a comparison key for a room name so cosmetically-different
        ///names (Revit vs. Excel) compare equal. Steps:
        ///- convert to lower case
        ///- convert separators (- _ # / \ ( ) [ ] .) to spaces
        ///- split letter/digit runs apart (so "wc1" tokenizes like "wc 1")
        ///- expand abbreviations: m -> master, wc -> toilet, kit -> kitchen
        ///- remove number markers (no / num / number)
        ///- remove leading zeros from numeric tokens
        ///- strip all whitespace to form the final key
        ///</summary>


        private static string NormalizeNameVersion03(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            string normalized = name.ToLowerInvariant();

            // 1. Convert separators into spaces (preserve tokens)
            normalized = Regex.Replace(
                normalized,
                @"[-_#/\\\(\)\[\]\.]",
                " ");

            // 2. Split letter/digit runs so attached tokens normalize like
            //    spaced ones ("wc1" -> "wc 1", "room01" -> "room 01").
            //
            // WHY: every rule below (steps 4-9) relies on word boundaries (\b),
            // which only exist when letters and digits are separated. Without
            // this split, "WC1" stayed "wc1" while "WC 1" became "toilet1", so
            // two names that should match produced different keys (false misses).
            // The zero-width lookarounds insert a space at each letter<->digit
            // edge without consuming characters.
            normalized = Regex.Replace(normalized, @"(?<=[a-z])(?=\d)|(?<=\d)(?=[a-z])", " ");

            // 3. Normalize whitespace
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            // CHANGED: steps below renumbered 4-10 (were 3-9) after the insert above

            // 4. Expand M to Master
            normalized = Regex.Replace(
                normalized,
                @"\bm\b",
                "master");

            // 5. WC normalization (token-based AFTER splitting)
            normalized = Regex.Replace(
                normalized,
                @"\bw\s*c\b",
                "toilet");

            // 6. KIT normalization
            normalized = Regex.Replace(
                normalized,
                @"\bkit\b",
                "kitchen");

            // 7. Remove number markers
            normalized = Regex.Replace(
                normalized,
                @"\b(no|num|number)\b",
                "");

            // 8. Normalize whitespace again (post-deletion cleanup)
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            // 9. Remove leading zeros in numeric tokens
            normalized = Regex.Replace(
                normalized,
                @"\b0+(\d+[a-z]*)\b",
                "$1");

            // 10. Final collapse for comparison key
            normalized = string.Concat(
                normalized.Where(c => !char.IsWhiteSpace(c)));

            return normalized;
        }
        

        // ===================== Apartment / type comparison =====================

        ///<summary>
        ///Compare each extracted apartment (UnitItem) against its TYPE's spec from
        ///the imported program. Matching is by apartment type (the name prefix);
        ///rooms inside an apartment match by normalized name. Produces the nested
        ///apartment+rooms results that drive the master-detail UI.
        ///</summary>
        public List<ApartmentCheck> CompareUnits(List<UnitItem> units, List<ProgramApartment> program, double tolerance = 0.05)
        {
            var results = new List<ApartmentCheck>();
            units = units ?? new List<UnitItem>();
            program = program ?? new List<ProgramApartment>();

            foreach (var unit in units)
            {
                var spec = MatchSpec(unit, program);

                var check = new ApartmentCheck { Unit = unit, Spec = spec };

                if (spec == null)
                {
                    // No matching type (e.g. "Unassigned") -> nothing to measure against.
                    foreach (var room in unit.Rooms)
                        check.Rooms.Add(new RoomCheck { Room = room, Spec = null, Status = ComplianceStatus.Unmatched });
                    check.Status = ComplianceStatus.Unmatched;
                    results.Add(check);
                    continue;
                }

                // Match each spec room to an actual room by normalized name.
                var remaining = new List<RoomItem>(unit.Rooms);
                foreach (var specRoom in spec.Rooms)
                {
                    string key = NormalizeNameVersion03(specRoom.RoomName);
                    var match = remaining.FirstOrDefault(rm => NormalizeNameVersion03(rm.Name) == key);
                    if (match != null) remaining.Remove(match);

                    check.Rooms.Add(new RoomCheck
                    {
                        Room = match,            // null => required room is missing
                        Spec = specRoom,
                        Status = RoomStatus(match, specRoom, tolerance)
                    });
                }

                // Actual rooms with no spec counterpart -> extras.
                foreach (var extra in remaining)
                    check.Rooms.Add(new RoomCheck { Room = extra, Spec = null, Status = ComplianceStatus.Unmatched });

                check.Status = RollUp(check, tolerance);
                results.Add(check);
            }

            return results;
        }

        // Match an extracted area to its program spec: exact instance name first
        // (so an exported-then-reimported model lines up 1:1), then fall back to
        // the area TYPE (so every instance of a type checks against one block).
        private static ProgramApartment MatchSpec(UnitItem unit, List<ProgramApartment> program)
        {
            var byName = program.FirstOrDefault(
                p => NormalizeNameVersion03(p.Name) == NormalizeNameVersion03(unit.Name));
            if (byName != null) return byName;

            if (string.IsNullOrEmpty(unit.Type)) return null;
            return program.FirstOrDefault(
                p => NormalizeNameVersion03(p.Type) == NormalizeNameVersion03(unit.Type));
        }

        private static ComplianceStatus RoomStatus(RoomItem room, ProgramRoom spec, double tolerance)
        {
            if (room == null) return ComplianceStatus.Unmatched; // required room missing
            double req = spec.RequiredArea;
            if (req > 0 && Math.Abs(room.ActualArea - req) <= req * tolerance) return ComplianceStatus.Met;
            if (req <= 0) return ComplianceStatus.Met;
            return room.ActualArea > req ? ComplianceStatus.Over : ComplianceStatus.Under;
        }

        // An area passes/fails on its OWN total area vs. the deviation tolerance.
        // Failing rooms inside it do NOT fail the area — they're surfaced as a
        // warning (HasRoomIssues) so the user knows to open and check the rooms.
        private static ComplianceStatus RollUp(ApartmentCheck c, double tolerance)
        {
            double req = c.RequiredArea;
            bool areaOk = req <= 0 || Math.Abs(c.ActualArea - req) <= req * tolerance;

            if (areaOk) return ComplianceStatus.Met;
            return c.ActualArea > req ? ComplianceStatus.Over : ComplianceStatus.Under;
        }
    }
}
