using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using LabelDesigner.Enums;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using Brushes = System.Windows.Media.Brushes;
using Size = System.Windows.Size;
using System.Text.Json.Serialization; //不能删除

public partial class ElementInfo : ObservableObject
{
    // 元素名，唯一id
    [ObservableProperty] private string name = string.Empty; 
    [ObservableProperty] private double x;//mm
    [ObservableProperty] private double y;//mm
    [ObservableProperty] private double width;//mm
    [ObservableProperty] private double height;//mm
    [ObservableProperty] private BarcodeType type;
    [ObservableProperty] private string content = string.Empty;
    [ObservableProperty] private double fontSize = 16;
    [ObservableProperty] private bool isSelected;

    [ObservableProperty]
    [property: JsonIgnore]
    private ImageSource imageSourceData = null;

    public double Dpi { get; set; } = 300;
    private const double WpfDpi = 96.0;

    public double WidthUnit => Width * WpfDpi / 25.4;
    public double HeightUnit => Height * WpfDpi / 25.4;

    public double XUnitPx
    {
        get => X * WpfDpi / 25.4;
        set => X = value * 25.4 / WpfDpi; // px -> mm
    }
    public double YUnitPx
    {
        get => Y * WpfDpi / 25.4;
        set => Y = value * 25.4 / WpfDpi;
    }

    partial void OnXChanged(double value) => OnPropertyChanged(nameof(XUnitPx));
    partial void OnYChanged(double value) => OnPropertyChanged(nameof(YUnitPx));
    partial void OnWidthChanged(double value)
    {
        OnPropertyChanged(nameof(WidthUnit)); 
        UpdateImage(Dpi);
    }
    partial void OnHeightChanged(double value)
    {
        OnPropertyChanged(nameof(HeightUnit)); 
        UpdateImage(Dpi);
    }

    partial void OnTypeChanged(BarcodeType oldValue, BarcodeType newValue) => UpdateImage(Dpi);
    partial void OnContentChanged(string value) => UpdateImage(Dpi);

    public void UpdateImage(double dpi)
    {
        if (string.IsNullOrWhiteSpace(Content) && Type != BarcodeType.Text)
        {
            ImageSourceData = null;
            return; 
        }
        if (Width <= 0 || Height <= 0)
        {
            return;
        }
        Dpi = dpi;

        if (Type == BarcodeType.Text)
        {
            ImageSourceData = RenderTextToImage(Content, Width, Height, FontSize, dpi);
            return;
        }

        int pxWidth = (int)Math.Round(Width * dpi / 25.4);
        int pxHeight = (int)Math.Round(Height * dpi / 25.4);
        var writer = new BarcodeWriter<Bitmap>
        {
            Format = Type switch
            {
                BarcodeType.QRCode => BarcodeFormat.QR_CODE,
                BarcodeType.Code128 => BarcodeFormat.CODE_128,
                BarcodeType.EAN13 => BarcodeFormat.EAN_13,
                BarcodeType.Code39 => BarcodeFormat.CODE_39,
                BarcodeType.PDF417 => BarcodeFormat.PDF_417,
                _ => BarcodeFormat.CODE_128
            },
            Options = new EncodingOptions
            {
                Width = pxWidth - 4,    
                Height = pxHeight - 4, 
                PureBarcode = true,
                Margin = 0            
            },
            Renderer = new BitmapRenderer()
        };

        if (Type == BarcodeType.Code39)
        {
            string allowed = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%";
            Content = new string(Content.ToUpperInvariant().Where(c => allowed.Contains(c)).ToArray());
        }

        var barcodeBmp = writer.Write(Content);

        // 创建新的 Bitmap，把条码放在中间，留 2px 内边距
        var finalBmp = new Bitmap(pxWidth, pxHeight);
        using (var g = Graphics.FromImage(finalBmp))
        {
            g.Clear(System.Drawing.Color.White); // 背景白色
            g.DrawImage(barcodeBmp, 2, 2, pxWidth - 4, pxHeight - 4);
        }

        finalBmp.SetResolution((float)dpi, (float)dpi);
        finalBmp.Save("111.png");
        ImageSourceData = BitmapToImageSource(finalBmp);
    }


    private ImageSource RenderTextToImage(string text, double widthMm, double heightMm, double fontSize, double dpi)
    {
        int pxWidth = (int)Math.Round(widthMm * dpi / 25.4);
        int pxHeight = (int)Math.Round(heightMm * dpi / 25.4);

        var tb = new TextBlock
        {
            Text = text,
            FontSize = fontSize,
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Background = Brushes.Transparent,
            Foreground = Brushes.Black,
            TextWrapping = TextWrapping.NoWrap, // 确保文本不会自动换行
            ClipToBounds = false // 确保文本不会因为超出设定区域而被裁剪
        };

        // 让 TextBlock 自动调整大小以适应文本内容
        tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        tb.Arrange(new Rect(0, 0, tb.DesiredSize.Width, tb.DesiredSize.Height));

        // 如果需要将图像固定在特定宽高，可以在此处创建一个更大的 RenderTargetBitmap
        // 并将 TextBlock 绘制在其左上角
        int renderWidth = Math.Max(pxWidth, (int)tb.DesiredSize.Width);
        int renderHeight = Math.Max(pxHeight, (int)tb.DesiredSize.Height);

        var rtb = new RenderTargetBitmap(renderWidth, renderHeight, dpi, dpi, PixelFormats.Pbgra32);
        rtb.Render(tb);
        rtb.Freeze();
        return rtb;
    }

    private ImageSource BitmapToImageSource(Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Position = 0;

        var imageSource = new BitmapImage();
        imageSource.BeginInit();
        imageSource.StreamSource = stream;
        imageSource.CacheOption = BitmapCacheOption.OnLoad;
        imageSource.EndInit();
        imageSource.Freeze();
        return imageSource;
    }
}
