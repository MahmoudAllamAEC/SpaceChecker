using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceChecker.Revit.Extraction
{
    internal static class UsageTypeMapper
    {
        private static readonly Dictionary<string, string> Rules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {

            { "office",      "Office" },
            { "meeting",     "Meeting Room" },
            { "conference",  "Meeting Room" },
            { "toilet",      "Wet Area" },
            { "bathroom",    "Wet Area" },
            { "reception",   "Reception" },
            { "lobby",       "Reception" },
            { "corridor",    "Circulation" },
            { "storage",     "Storage" },
            { "server",      "Technical" },
            { "plant",       "Technical" },
         };

        public static string GetUsageType(string roomName)
        {
            foreach (var kvp in Rules)
            {
                if (roomName.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return kvp.Value;
                }

            }
            return "Unclassified";

        }
    }
}
