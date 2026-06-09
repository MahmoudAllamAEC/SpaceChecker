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
                switch (status)
                {
                    case ComplianceStatus.Met:
                        return new SolidColorBrush(Color.FromRgb(209, 250, 229));
                    case ComplianceStatus.Over:
                        return new SolidColorBrush(Color.FromRgb(254, 243, 199));
                    case ComplianceStatus.Under:
                        return new SolidColorBrush(Color.FromRgb(254, 226, 226));
                    case ComplianceStatus.Unmatched:
                        return new SolidColorBrush(Color.FromRgb(243, 244, 246));
                    default:
                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
