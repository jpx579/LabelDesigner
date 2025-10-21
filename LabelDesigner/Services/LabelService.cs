using System.Drawing.Printing;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LabelDesigner.Enums;
using LabelDesigner.Models;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

public static class LabelService
{
    // 保存 JSON
    public static void SaveLabel(string filePath, LabelModel label)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(label, options);
        File.WriteAllText(filePath, json);
    }

    // 读取 JSON
    public static LabelModel LoadLabel(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<LabelModel>(json) ?? throw new Exception("读取失败");
    }
    /// <summary>
    /// 如果需要热更新标签中的内容
    /// 1.先调用GetLabelElementDic获取标签需要热更新的键值对
    /// 2.替换实际的数据
    /// 3.发送PrintLabel打印
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetLabelElementDic(string filePath) 
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        LabelModel labelModel = LoadLabel(filePath);
        foreach (var item in labelModel.LabelElements)
        {
            if (!string.IsNullOrWhiteSpace(item.Name))
            {
                dic.Add(item.Name, "123456");
            }
        }
        return dic;
    }

    public static bool PrintLabel(string filePath, Dictionary<string, string> data, string printerName, out string message, double marginMm = 1.0)
    {
        try
        {
            LabelModel labelModel = LoadLabel(filePath);
            ApplyDataToLabelAndPrint(labelModel, data, printerName, marginMm);
            message = "打印成功";
            return true;
        }
        catch (Exception ex)
        {
            message = $"打印失败：{ex}";
            return false;
        }      
    }

    /// <summary>
    ///  打印标签
    /// </summary>
    /// <param name="label"></param>
    /// <param name="data"></param>
    /// <param name="printerName"></param>
    /// <param name="marginMm"></param>
    public static void ApplyDataToLabelAndPrint(LabelModel label, Dictionary<string, string> data, string printerName, double marginMm = 1.0)
    {
        if (label == null || label.LabelElements == null || !label.LabelElements.Any())
            return;

        // 1️⃣ 更新内容并生成条码图片
        foreach (var kv in data)
        {
            var elem = label.FindElement(kv.Key);
            if (elem != null)
            {
                elem.Content = kv.Value;
                if (elem.Type != BarcodeType.Text)
                    elem.UpdateImage(label.Dpi);
            }
        }

        // 2️⃣ 构建 Canvas
        double canvasWidth = label.Width * 96.0 / 25.4;
        double canvasHeight = label.Height * 96.0 / 25.4;

        var canvas = new Canvas
        {
            Width = canvasWidth,
            Height = canvasHeight,
            Background = Brushes.White
        };

        foreach (var el in label.LabelElements)
        {
            UIElement element;

            if (el.Type == BarcodeType.Text)
            {
                element = new TextBlock
                {
                    Text = el.Content,
                    FontSize = el.FontSize,
                    Width = el.WidthUnit - 2,   // 留 2px 内边距
                    Height = el.HeightUnit - 2, // 留 2px 内边距
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
            }
            else
            {
                element = new Image
                {
                    Source = el.ImageSourceData,
                    Width = el.WidthUnit - 2, // 留 2px 内边距
                    Height = el.HeightUnit - 2,
                    Stretch = Stretch.Fill
                };
            }

            // 设置位置时加上元素内边距
            Canvas.SetLeft(element, el.XUnitPx + 1);
            Canvas.SetTop(element, el.YUnitPx + 1);

            canvas.Children.Add(element);
        }

        // 3️⃣ 打印 Canvas
        var pd = new PrintDialog();
        pd.PrintQueue = new System.Printing.PrintQueue(new System.Printing.PrintServer(), printerName);

        // 可打印区域
        double printableWidth = pd.PrintableAreaWidth;
        double printableHeight = pd.PrintableAreaHeight;

        double marginDIP = marginMm * 96.0 / 25.4;

        // 计算缩放比例，保证不裁切
        double scaleX = (printableWidth - 2 * marginDIP) / canvas.Width;
        double scaleY = (printableHeight - 2 * marginDIP) / canvas.Height;
        double scale = Math.Min(scaleX, scaleY);

        canvas.LayoutTransform = new ScaleTransform(scale, scale);
        canvas.RenderTransform = new TranslateTransform(marginDIP, marginDIP);

        Size scaledSize = new Size(canvas.Width * scale, canvas.Height * scale);
        canvas.Measure(scaledSize);
        canvas.Arrange(new Rect(new Point(0, 0), scaledSize));
        canvas.UpdateLayout();

        pd.PrintVisual(canvas, "打印标签");

        SaveCanvasToImage(canvas,"1.png");
    }
  
    /// <summary>
    /// 保存打印机图片
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="filePath"></param>
    /// <param name="dpi"></param>
    public static void SaveCanvasToImage(Canvas canvas, string filePath, double dpi = 300)
    {
        if (canvas == null) return;

        canvas.Measure(new Size(canvas.Width, canvas.Height));
        canvas.Arrange(new Rect(new Size(canvas.Width, canvas.Height)));
        canvas.UpdateLayout();

        var rtb = new RenderTargetBitmap(
            (int)Math.Round(canvas.Width * dpi / 96.0),
            (int)Math.Round(canvas.Height * dpi / 96.0),
            dpi, dpi,
            PixelFormats.Pbgra32);

        rtb.Render(canvas);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));

        using (var fs = new FileStream(filePath, FileMode.Create))
        {
            encoder.Save(fs);
        }
    }

    /// <summary>
    /// 获取打印机队列
    /// </summary>
    /// <returns></returns>
    public static List<string> GetInstalledPrinters()
    {
        var printers = new List<string>();
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            printers.Add(printer);
        }
        return printers;
    }
}


