using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SpaceChecker.Revit.Entry
{
    /// <summary>
    /// Resolves SpaceChecker's bundled dependency DLLs (EPPlus and the assemblies
    /// EPPlus itself needs) at runtime.
    ///
    /// Why this is needed: in development the add-in is loaded through Revit's
    /// Add-In Manager, which loads SpaceChecker.dll as a raw byte array so it can
    /// hot-reload without locking the file. A byte-loaded assembly has an EMPTY
    /// Location, and the CLR will NOT probe any folder for its dependencies — so
    /// the moment we touch EPPlus we get "Could not load file or assembly
    /// 'EPPlus...' or one of its dependencies. The system cannot find the file
    /// specified." Add-In Manager DOES copy the dependency DLLs next to the
    /// byte-loaded copy (under %TEMP%\RevitAddins\&lt;name&gt;-Executing-...\), so we
    /// hook AssemblyResolve and load them from there — or from this assembly's own
    /// folder when it is loaded normally from bin.
    /// </summary>
    internal static class DependencyResolver
    {
        private static bool _registered;

        // The assemblies we ship and may have to resolve by hand.
        private static readonly string[] Bundled =
        {
            "EPPlus",
            "Microsoft.IO.RecyclableMemoryStream",
            "System.ComponentModel.Annotations",
        };

        /// <summary>Idempotent — safe to call from every entry point.</summary>
        public static void Register()
        {
            if (_registered) return;
            _registered = true;
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;

            bool ours = false;
            foreach (string b in Bundled)
                if (name.Equals(b, StringComparison.OrdinalIgnoreCase)) { ours = true; break; }
            if (!ours) return null;

            foreach (string dir in CandidateDirectories())
            {
                if (string.IsNullOrEmpty(dir)) continue;
                string dll = Path.Combine(dir, name + ".dll");
                if (File.Exists(dll))
                    return Assembly.LoadFrom(dll);
            }
            return null;
        }

        // Folders that may hold our bundled DLLs, most-likely first.
        private static IEnumerable<string> CandidateDirectories()
        {
            Assembly self = typeof(DependencyResolver).Assembly;

            // 1. Where this assembly physically lives (native/bin load, or any
            //    loader that set a real Location).
            string loc = self.Location;
            if (!string.IsNullOrEmpty(loc))
                yield return Path.GetDirectoryName(loc);

            // 2. Add-In Manager copies the add-in + its deps to
            //    %TEMP%\RevitAddins\<assembly>-Executing-<timestamp>\. When it
            //    byte-loads us, Location is empty, so look there ourselves —
            //    newest copy first. Filter to our own folders so we never pick up
            //    another add-in's bundled copy.
            string addinTemp = Path.Combine(Path.GetTempPath(), "RevitAddins");
            if (Directory.Exists(addinTemp))
            {
                string[] dirs;
                try { dirs = Directory.GetDirectories(addinTemp, self.GetName().Name + "*"); }
                catch { dirs = new string[0]; }
                Array.Sort(dirs, StringComparer.OrdinalIgnoreCase); // timestamped names sort chronologically
                for (int i = dirs.Length - 1; i >= 0; i--)
                    yield return dirs[i];
            }
        }
    }
}
