using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using SpaceChecker.Revit.Models;

namespace SpaceChecker.UI.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ComplianceStatus status)
            {
                // Pass (within tolerance) -> green; anything else
                // (Over / Under / Unmatched) counts as a fail -> red.
                return status == ComplianceStatus.Met
                    ? new SolidColorBrush(Color.FromRgb(209, 250, 229))   // light green
                    : new SolidColorBrush(Color.FromRgb(254, 226, 226));  // light red
            }
            return Brushes.Transparent;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
