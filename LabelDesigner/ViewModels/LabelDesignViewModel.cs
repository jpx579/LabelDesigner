using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LabelDesigner.Enums;
using LabelDesigner.Models;
using LabelDesigner.Services;
using LabelDesigner.Views;

namespace LabelDesigner.ViewModels
{
    public partial class LabelDesignViewModel : ObservableObject
    {
        public IEnumerable<BarcodeType> AvailableBarcodeTypes => Enum.GetValues<BarcodeType>();
        private readonly string _labelFolderPath = "Configs//PrintLabels";
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

        public LabelDesignViewModel()
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
        partial void OnLabelWidthMmChanged(double value)
        {
            UpdateCanvasSize();
            UpdateLabelPoint();
        }
        partial void OnLabelHeightMmChanged(double value)
        {
            UpdateCanvasSize();
            UpdateLabelPoint();
        }
        private void UpdateCanvasSize()
        {
            CanvasWidth = Math.Round(LabelWidthMm * WpfDpi / 25.4, 2);
            CanvasHeight = Math.Round(LabelHeightMm * WpfDpi / 25.4, 2);
        }
        private void UpdateLabelPoint()
        {
            foreach (var element in LabelElements)
            {
                element.X = 10;
                element.Y = 10;
                element.Width = 40;
                element.Height = 20;
                element.Dpi = SelectedDpi;
            }
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
                foreach (var name in files.Where(s => !string.IsNullOrWhiteSpace(s)))
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
                Dpi = SelectedDpi
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
        private void SaveLayout()
        {
            LabelModel labelModel = new LabelModel();
            labelModel.Width = LabelWidthMm;
            labelModel.Height = LabelHeightMm;
            labelModel.Dpi = SelectedDpi;
            labelModel.LabelElements = LabelElements.ToList();
            LabelService.SaveLabel($"{_labelFolderPath}\\{SelectedLabel}.json", labelModel);
        }

        [RelayCommand]
        private void PrintLayout()
        {
            if (string.IsNullOrWhiteSpace(SelectedPrinter))
            {                
                return;
            }
            LabelModel labelModel = new LabelModel();
            labelModel.Width = LabelWidthMm;
            labelModel.Height = LabelHeightMm;
            labelModel.Dpi = SelectedDpi;
            labelModel.LabelElements = LabelElements.ToList();

            var dic = new Dictionary<string, string>();

            LabelService.ApplyDataToLabelAndPrint(labelModel, dic, SelectedPrinter ?? throw new Exception(), 2);
        }
    }
}
