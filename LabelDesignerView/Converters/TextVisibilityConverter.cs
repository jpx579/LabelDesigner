using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using LabelDesigner.Models;

namespace LabelDesigner.Converters
{
    ///// <summary>
    ///// 根据 Type 判断文字是否显示
    ///// </summary>
    //public class TextVisibilityConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is string type && type == "Text")
    //            return Visibility.Visible;
    //        return Visibility.Collapsed;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    /// <summary>
    /// 根据 Type 判断条码是否显示
    /// </summary>
    public class BarcodeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type && type == "Barcode")
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 根据 Type 判断二维码是否显示
    /// </summary>
    public class QRCodeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type && type == "QRCode")
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    ///// <summary>
    ///// 根据 ElementInfo 生成条码图片
    ///// </summary>
    //public class BarcodeImageConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is ElementInfo element && element.Type == "Barcode")
    //        {
    //            // 这里你可以用你自己的条码生成逻辑
    //            // 简单示例：使用 BitmapImage 加载本地图片
    //            // 你也可以使用 ZXing.Net 生成条码
    //            return new BitmapImage(new Uri("pack://application:,,,/Images/sample_barcode.png"));
    //        }
    //        return null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    ///// <summary>
    ///// 根据 ElementInfo 生成二维码图片
    ///// </summary>
    //public class QRCodeImageConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is ElementInfo element && element.Type == "QRCode")
    //        {
    //            // 这里你可以用你自己的二维码生成逻辑
    //            // 简单示例：使用 BitmapImage 加载本地图片
    //            // 你也可以使用 ZXing.Net 生成二维码
    //            return new BitmapImage(new Uri("pack://application:,,,/Images/sample_qrcode.png"));
    //        }
    //        return null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
