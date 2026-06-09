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
using SpaceChecker.UI.Views;



namespace SpaceChecker.Revit.Entry
{
    [Transaction(TransactionMode.Manual)]
    public class ExtCmd : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                UIDocument uiDoc = uiApp.ActiveUIDocument;
                Document doc = uiDoc.Document;

                var service = new SpaceProgramService { Doc = doc };
                var window = new MainWindow(service); //inject
                window.ShowDialog();
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

