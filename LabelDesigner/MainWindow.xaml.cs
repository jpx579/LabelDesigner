using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LabelDesigner.ViewModels;

namespace LabelDesigner;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new MainWindowViewModel();
        InitializeComponent();
    }

    private bool _isDragging=false;
    private Point _startMousePos = new Point(0,0);
    private ElementInfo _draggingElement=null;

    private void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Grid grid && grid.DataContext is ElementInfo el)
        {
            _draggingElement = el;
            _startMousePos = e.GetPosition(DesignCanvas);
            _isDragging = true;

            // 选中元素
            ((MainWindowViewModel)DataContext).SelectElementCommand.Execute(el);

            grid.CaptureMouse();
        }
    }

    private void Element_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && _draggingElement != null)
        {
            var pos = e.GetPosition(DesignCanvas);
            double dx = (pos.X - _startMousePos.X) * 25.4 / 96.0; // px -> mm
            double dy = (pos.Y - _startMousePos.Y) * 25.4 / 96.0;

            // 计算新位置（mm）
            double newX = _draggingElement.X + dx;
            double newY = _draggingElement.Y + dy;

            // 获取标签尺寸（mm）
            double labelWidth = ((MainWindowViewModel)DataContext).LabelWidthMm;
            double labelHeight = ((MainWindowViewModel)DataContext).LabelHeightMm;

            // 获取元素自身尺寸（mm）
            double elemWidth = _draggingElement.Width;
            double elemHeight = _draggingElement.Height;

            // 限制 X 范围：0 <= X <= labelWidth - elemWidth
            newX = Math.Max(0, Math.Min(newX, labelWidth - elemWidth));

            // 限制 Y 范围：0 <= Y <= labelHeight - elemHeight
            newY = Math.Max(0, Math.Min(newY, labelHeight - elemHeight));

            // 更新位置
            _draggingElement.X = newX;
            _draggingElement.Y = newY;

            _startMousePos = pos;
        }
    }

    private void Element_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_draggingElement != null)
        {
            _isDragging = false;
            if (sender is Grid grid) grid.ReleaseMouseCapture();
            _draggingElement = null;
        }
    }
}
