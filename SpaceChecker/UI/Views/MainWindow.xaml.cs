using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpaceChecker.UI.ViewModels;

namespace SpaceChecker.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            TrySetWindowIcon();
        }

        // Use the small embedded floor-plan icon as the window's title-bar icon.
        // Loaded from the manifest stream (the icons are EmbeddedResource), so this
        // doesn't disturb how the ribbon button loads the same images.
        private void TrySetWindowIcon()
        {
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                using (var s = asm.GetManifestResourceStream("SpaceChecker.Resources.icons8-floor-plan-16.png"))
                {
                    if (s != null)
                        Icon = BitmapFrame.Create(s, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }
            catch { /* icon is cosmetic — ignore if it can't be loaded */ }
        }

        // The areas grid is grouped, so star (*) column widths collapse. Instead we
        // let the "Instance" column flex to fill whatever width is left after the
        // three fixed columns — keeping the table full-width as the window resizes.
        private void AreasGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (InstanceColumn == null) return;

            const double fixedColumns = 130 + 130 + 110; // Actual + Deviation + Pass/Fail
            const double chrome = 34;                     // v-scrollbar + borders/padding
            double available = AreasGrid.ActualWidth - fixedColumns - chrome;

            InstanceColumn.Width = new DataGridLength(System.Math.Max(InstanceColumn.MinWidth, available));
        }
    }
}
