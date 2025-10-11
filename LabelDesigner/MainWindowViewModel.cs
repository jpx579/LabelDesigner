using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LabelDesigner.Enums;
using LabelDesigner.Models;
using LabelDesigner.Views;

namespace LabelDesigner.ViewModels;
public partial class MainWindowViewModel : ObservableObject
{
    public IEnumerable<BarcodeType> AvailableBarcodeTypes => Enum.GetValues<BarcodeType>();
    private readonly string _labelFolderPath = "PrintLabels";
    private const double WpfDpi = 96.0;

    [ObservableProperty] private int selectedDpi = 300;
    [ObservableProperty] private int[] dpiList = { 200, 300, 600 };
    //画布宽高mm实时值
    [ObservableProperty] private double labelWidthMm = 100;
    [ObservableProperty] private double labelHeightMm = 70;
   
    //画布宽高像素实时值
    [ObservableProperty] private double canvasWidth;
    [ObservableProperty] private double canvasHeight;

    //标签列表
    [ObservableProperty] private ObservableCollection<string> labelNames = new();
    [ObservableProperty] private string? selectedLabel;

    //打印机列表
    [ObservableProperty] private ObservableCollection<string> printerList = new();
    [ObservableProperty] private string? selectedPrinter;

    //标签内部-元素
    public ObservableCollection<ElementInfo> LabelElements { get; set; } = new();
    [ObservableProperty] private ElementInfo selectedElement = null;
   
    public MainWindowViewModel()
    {
        LoadLabels();
        UpdateCanvasSize();
        LoadPrinters();

        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SelectedLabel))
            {
                LoadSelectedLabel();
            }
        };
    }

    partial void OnSelectedDpiChanged(int value) => UpdateCanvasSize();
    partial void OnLabelWidthMmChanged(double value) => UpdateCanvasSize();
    partial void OnLabelHeightMmChanged(double value) => UpdateCanvasSize();
    private void UpdateCanvasSize()
    {
        CanvasWidth = Math.Round(LabelWidthMm * WpfDpi / 25.4, 2);
        CanvasHeight = Math.Round(LabelHeightMm * WpfDpi / 25.4, 2);
    }
    private void LoadSelectedLabel()
    {
        LabelElements.Clear();       

        if (string.IsNullOrWhiteSpace(SelectedLabel))
            return;

        string path = Path.Combine(_labelFolderPath, SelectedLabel + ".json");
        if (!File.Exists(path))
            return;

        try
        {
            var elements = LabelService.LoadLabel(path);

            if (elements != null)
            {
                LabelWidthMm = elements.Width;
                LabelHeightMm = elements.Height;
                SelectedDpi = (int)elements.Dpi;
                foreach (var e in elements.LabelElements)
                    LabelElements.Add(e);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载标签内容失败：{ex.Message}");
        }
    }
    private void LoadPrinters()
    {
        var printers = LabelService.GetInstalledPrinters();
        PrinterList.Clear();
        foreach (var p in printers)
            PrinterList.Add(p);
    }
    private void LoadLabels()
    {
        try
        {
            if (!Directory.Exists(_labelFolderPath))
                Directory.CreateDirectory(_labelFolderPath);

            var files = Directory.GetFiles(_labelFolderPath, "*.json", SearchOption.TopDirectoryOnly)
                                 .Select(Path.GetFileNameWithoutExtension)
                                 .OrderBy(name => name)
                                 .ToList();

            LabelNames.Clear();
            foreach (var name in files)
                LabelNames.Add(name);

            if (LabelNames.Count > 0)
                SelectedLabel = LabelNames[0];
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载标签列表失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void AddPrintLabel()
    {
        try
        {
            var addWindow = new AddLabelWindow();
            addWindow.Owner = Application.Current.MainWindow;
            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                LoadLabels();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"打开添加窗口失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteLabel()
    {
        if (string.IsNullOrWhiteSpace(SelectedLabel))
        {
            MessageBox.Show("请先选择要删除的标签！");
            return;
        }

        string path = Path.Combine(_labelFolderPath, SelectedLabel + ".json");
        if (File.Exists(path))
        {
            if (MessageBox.Show($"确定要删除标签 '{SelectedLabel}' 吗？", "确认删除",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                File.Delete(path);
                LoadLabels();
            }
        }
    }

    [RelayCommand]
    private void AddElement()
    {
        var el = new ElementInfo
        {
            Type = BarcodeType.Text,
            Content = "579579",
            X = 10,
            Y = 10,
            Width = 40,
            Height = 20,
            FontSize = 16,
            IsSelected = true,
            Dpi=SelectedDpi
        };
        el.UpdateImage(SelectedDpi);
        LabelElements.Add(el);
        SelectElement(el);
    }

    [RelayCommand]
    private void DeleteElement(ElementInfo element)
    {
        if (element != null)
            LabelElements.Remove(element);
    }

    [RelayCommand]
    private void SelectElement(ElementInfo element)
    {
        foreach (var e in LabelElements)
            e.IsSelected = false;
        element.IsSelected = true;
        SelectedElement = element;
    }

    [RelayCommand]
    private void SaveLayout() {
        LabelModel labelModel = new LabelModel();
        labelModel.Width = LabelWidthMm;
        labelModel.Height = LabelHeightMm;
        labelModel.Dpi = SelectedDpi;
        labelModel.LabelElements = LabelElements.ToList();
        LabelService.SaveLabel($"{_labelFolderPath}\\{SelectedLabel}.json",labelModel);
    }

    [RelayCommand]
    private void PrintLayout()
    {
        LabelModel labelModel = new LabelModel();
        labelModel.Width = LabelWidthMm;
        labelModel.Height = LabelHeightMm;
        labelModel.Dpi = SelectedDpi;
        labelModel.LabelElements = LabelElements.ToList();

        var dic = new Dictionary<string, string> {
            {"1","123" },
            {"2","456" }
        };

        LabelService.ApplyDataToLabelAndPrint(labelModel,dic,SelectedPrinter??throw new Exception(),2);

        return;


        //// 1. 构建 Canvas
        //double canvasWidth = LabelWidthMm * WpfDpi / 25.4;
        //double canvasHeight = LabelHeightMm * WpfDpi / 25.4;

        //var canvas = new Canvas
        //{
        //    Width = canvasWidth,
        //    Height = canvasHeight,
        //    Background = Brushes.White
        //};

        //// 2. 添加元素
        //foreach (var el in LabelElements)
        //{
        //    UIElement element;

        //    if (el.Type == BarcodeType.Text)
        //    {
        //        element = new TextBlock
        //        {
        //            Text = el.Content,
        //            FontSize = el.FontSize,
        //            Width = el.WidthUnit,
        //            Height = el.HeightUnit,
        //            TextAlignment = TextAlignment.Left,
        //            VerticalAlignment = VerticalAlignment.Top
        //        };
        //    }
        //    else
        //    {
        //        element = new Image
        //        {
        //            Source = el.ImageSourceData,
        //            Width = el.WidthUnit,
        //            Height = el.HeightUnit
        //        };
        //    }

        //    Canvas.SetLeft(element, el.XUnitPx);
        //    Canvas.SetTop(element, el.YUnitPx);
        //    canvas.Children.Add(element);
        //}

        //// 3. 打印对话框
        //var printDialog = new PrintDialog();
        //if (printDialog.ShowDialog() != true) return;

        //// 可打印区域（单位 DIPs）
        //double printableWidth = printDialog.PrintableAreaWidth;
        //double printableHeight = printDialog.PrintableAreaHeight;

        //// 4. 内收 1mm
        //double marginMm = 1.0;
        //double marginDIP = marginMm * WpfDpi / 25.4;

        //// 5. 计算缩放比例（保持比例，不裁切）
        //double scaleX = (printableWidth - 2 * marginDIP) / canvas.Width;
        //double scaleY = (printableHeight - 2 * marginDIP) / canvas.Height;
        //double scale = Math.Min(scaleX, scaleY);

        //// 6. 应用缩放
        //canvas.LayoutTransform = new ScaleTransform(scale, scale);

        //// 7. 平移 Canvas，让内容内收 1mm
        //canvas.RenderTransform = new TranslateTransform(marginDIP, marginDIP);

        //// 8. 重新布局 Canvas
        //Size scaledSize = new Size(canvas.Width * scale, canvas.Height * scale);
        //canvas.Measure(scaledSize);
        //canvas.Arrange(new Rect(new Point(0, 0), scaledSize));
        //canvas.UpdateLayout();

        //// 9. 打印
        //printDialog.PrintVisual(canvas, "打印标签");

        //// 10. 可选：保存预览图片，方便调试
        //SaveCanvasToPng(canvas, "LabelPreview.png");
    }

    private void SaveCanvasToPng(Canvas canvas, string filePath)
    {
        int width = (int)canvas.Width;
        int height = (int)canvas.Height;

        var rtb = new RenderTargetBitmap(width, height, WpfDpi, WpfDpi, PixelFormats.Pbgra32);
        rtb.Render(canvas);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));

        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            encoder.Save(fs);
        }
    }
}
