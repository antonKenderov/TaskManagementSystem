using System.Globalization;
using System.Windows.Data;

namespace TaskManagementSystem.Views
{
    public class EnumInitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return string.Empty;
            }

            var name = value.ToString() ?? string.Empty;
            var initials = new string(name.Where(char.IsUpper).ToArray());

            return initials.Length > 0 ? initials[..Math.Min(2, initials.Length)] : name[..1].ToUpperInvariant();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
