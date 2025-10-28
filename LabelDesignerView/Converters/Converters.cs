using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using LabelDesigner.Enums;

namespace LabelDesigner.Converters;

public class TextTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is BarcodeType type && type == BarcodeType.Text
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BarcodeTypeConverter : IValueConverter
{
    private static readonly HashSet<BarcodeType> BarcodeTypes = new()
    {
        BarcodeType.QRCode,
        BarcodeType.Code128,
        BarcodeType.EAN13,
        BarcodeType.Code39,
        BarcodeType.PDF417
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is BarcodeType type && BarcodeTypes.Contains(type)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (value is bool b && b) ? Brushes.Green : Brushes.Red;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class SelectedBorderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
            return Brushes.Red;
        return Brushes.Green;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
