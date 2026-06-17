using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;  // Reference WindowsBase.dll
using System.Windows.Media.Imaging;  // Reference PresentationCore.dll

using Autodesk.Revit.UI;


namespace SpaceChecker.Revit.Entry
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalApplication
    /// </summary>
    class ExtApp : IExternalApplication
    {
        /// <summary>
        /// The Add-in name to be shown on the form caption and on the push button.
        /// </summary>
        public static Assembly ThisAssembly { get; } = Assembly.GetExecutingAssembly();
        


        /// <summary>
        /// Implements the OnStartup event.
        /// </summary>
        /// <param name="uicApp"></param>
        /// <returns></returns>
        public Result OnStartup(UIControlledApplication uicApp)
        {
            // Make sure our bundled EPPlus (and its deps) can be found at runtime,
            // regardless of how Revit loaded this add-in. See DependencyResolver.
            DependencyResolver.Register();

            string tabName = "AACD Architect";
            string panelName = "General";
            string addinName = "Space Checker";
            string toolTip = "Cheack Rooms And Spaces for the Write Area ";

            // In case of creating a tab with the name of an existing one, an Exception will be raised.
            try
            {
                uicApp.CreateRibbonTab(tabName);
            }
            catch { }

            // Create or get panel
            RibbonPanel panel = uicApp.GetRibbonPanels(tabName)
                                      .FirstOrDefault(p => p.Name == panelName)
                                    ?? uicApp.CreateRibbonPanel(tabName, panelName);

           

            string assemblyName = ThisAssembly.GetName().Name;
            string classPath = $"{assemblyName}.Revit.Entry.ExtCmd";

            //Add-in Button:
            PushButtonData pbData_ExtCmd = new PushButtonData(
                $"{assemblyName}_btn",
                addinName,
                ThisAssembly.Location,
                classPath
                );

            PushButton pb_ExtCmd = panel.AddItem(pbData_ExtCmd) as PushButton;
            pb_ExtCmd.ToolTip = toolTip;


           

            // Button Image: use the big 32px icon for both the large and small
            // ribbon slots so the Revit UI always shows the larger icon.
            string image_EmbeddedPath = $"{assemblyName}.Resources.icons8-floor-plan-32.png";
            pb_ExtCmd.LargeImage = getImageSource($"{assemblyName}.Resources.icons8-floor-plan-24.png");
            pb_ExtCmd.Image = getImageSource(image_EmbeddedPath);


            return Result.Succeeded;
        }

        /// <summary>
        /// Implements the OnShutdown event.
        /// </summary>
        /// <param name="uicApp"></param>
        /// <returns></returns>
        public Result OnShutdown(UIControlledApplication uicApp)
        {
            return Result.Succeeded;
        }



        /// <summary>
        /// Returns ImageSource of the passed png Image Embedded Path.
        /// </summary>
        /// <param name="image_EmbeddedPath">The embedded path of the image, e.g., namespace_Name.Resources.Image_Name</param>
        /// <returns></returns>
        private ImageSource getImageSource(string image_EmbeddedPath)
        {
            Stream stream = ThisAssembly.GetManifestResourceStream(image_EmbeddedPath);
            if (stream == null) return null; // Prevent null crash

            BitmapDecoder decoder = new PngBitmapDecoder(
                    stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad
                );


            return decoder?.Frames[0];
        }


        

    }
}
