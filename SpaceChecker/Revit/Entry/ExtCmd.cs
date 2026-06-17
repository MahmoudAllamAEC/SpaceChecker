using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SpaceChecker.Revit.Services;
using SpaceChecker.UI.ViewModels;
using SpaceChecker.UI.Views;



namespace SpaceChecker.Revit.Entry
{
    [Transaction(TransactionMode.Manual)]
    public class ExtCmd : IExternalCommand
    {
        // Held so the modeless window isn't garbage-collected, and so a second
        // click just re-activates the existing window instead of opening another.
        private static MainWindow _window;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Add-In Manager can run this command without calling OnStartup,
                // so ensure our EPPlus dependency resolver is hooked up before any
                // Excel call. Idempotent.
                DependencyResolver.Register();

                UIApplication uiApp = commandData.Application;
                UIDocument uiDoc = uiApp.ActiveUIDocument;
                Document doc = uiDoc.Document;

                // Already open -> bring it forward.
                if (_window != null)
                {
                    _window.Activate();
                    return Result.Succeeded;
                }

                var service = new SpaceProgramService { Doc = doc };
                var viewModel = new MainViewModel(service);

                _window = new MainWindow(viewModel); //inject
                // Own the window to Revit's main window so it stays on top of the
                // document (and minimizes/restores with Revit).
                new System.Windows.Interop.WindowInteropHelper(_window).Owner = uiApp.MainWindowHandle;
                _window.Closed += (s, e) => _window = null;

                // Modeless: keeps Revit interactive so the user can move around the
                // document while Space Checker is open.
                _window.Show();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

}

