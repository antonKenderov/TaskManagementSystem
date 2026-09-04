using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Views
{
    /// <summary>
    /// Maps a task status to its colour. Pass "Background" as the converter
    /// parameter for the tinted fill, anything else gives the strong shade.
    /// </summary>
    public class StatusToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush OpenForeground = Freeze("#15803D");
        private static readonly SolidColorBrush OpenBackground = Freeze("#DCFCE7");

        private static readonly SolidColorBrush InProgressForeground = Freeze("#2563EB");
        private static readonly SolidColorBrush InProgressBackground = Freeze("#DBEAFE");

        private static readonly SolidColorBrush ClosedForeground = Freeze("#B91C1C");
        private static readonly SolidColorBrush ClosedBackground = Freeze("#FEE2E2");

        private static readonly SolidColorBrush UnknownForeground = Freeze("#475569");
        private static readonly SolidColorBrush UnknownBackground = Freeze("#F1F5F9");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var wantsBackground = string.Equals(parameter as string, "Background", StringComparison.OrdinalIgnoreCase);

            return value switch
            {
                Status.Open => wantsBackground ? OpenBackground : OpenForeground,
                Status.InProgress => wantsBackground ? InProgressBackground : InProgressForeground,
                Status.Closed => wantsBackground ? ClosedBackground : ClosedForeground,
                _ => wantsBackground ? UnknownBackground : UnknownForeground
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static SolidColorBrush Freeze(string hex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }
    }
}
