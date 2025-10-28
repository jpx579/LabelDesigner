using System.Windows;
using System.Windows.Controls;
using LabelDesigner.ViewModels;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LabelDesigner.Views
{
    /// <summary>
    /// LabelDesignView.xaml 的交互逻辑
    /// </summary>
    public partial class LabelDesignView : UserControl
    {
        public LabelDesignView(LabelDesignViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private bool _isDragging = false;
        private Point _startMousePos = new Point(0, 0);
        private ElementInfo _draggingElement = null;

        private static T FindVisualAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FindVisualAncestor<Thumb>((DependencyObject)e.OriginalSource) != null) return;
            if (sender is Grid grid && grid.DataContext is ElementInfo el)
            {
                _draggingElement = el;
                _startMousePos = e.GetPosition(DesignCanvas);
                _isDragging = true;

                ((LabelDesignViewModel)DataContext).SelectElementCommand.Execute(el);

                grid.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _draggingElement == null) return;

            if (e.OriginalSource is DependencyObject dep &&
                FindVisualAncestor<Thumb>(dep) != null)
                return;

            var pos = e.GetPosition(DesignCanvas);
            double dx_mm = (pos.X - _startMousePos.X) * 25.4 / 96.0;
            double dy_mm = (pos.Y - _startMousePos.Y) * 25.4 / 96.0;

            double newX = _draggingElement.X + dx_mm;
            double newY = _draggingElement.Y + dy_mm;

            double labelWidth = ((LabelDesignViewModel)DataContext).LabelWidthMm;
            double labelHeight = ((LabelDesignViewModel)DataContext).LabelHeightMm;

            double elemWidth = _draggingElement.Width;
            double elemHeight = _draggingElement.Height;

            newX = Math.Max(0, Math.Min(newX, labelWidth - elemWidth));
            newY = Math.Max(0, Math.Min(newY, labelHeight - elemHeight));

            _draggingElement.X = newX;
            _draggingElement.Y = newY;

            _startMousePos = pos;
        }

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_draggingElement != null)
            {
                _isDragging = false;
                if (sender is Grid grid) grid.ReleaseMouseCapture();
                _draggingElement = null;
            }
        }

        private void ResizeThumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Thumb thumb && thumb.DataContext is ElementInfo el)
            {
                ((LabelDesignViewModel)DataContext).SelectElementCommand.Execute(el);
            }
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is not Thumb thumb || thumb.DataContext is not ElementInfo item)
                return;

            var vm = (LabelDesignViewModel)DataContext;


            double dx_mm = e.HorizontalChange * 25.4 / item.Dpi;
            double dy_mm = e.VerticalChange * 25.4 / item.Dpi;

            double newWidth = item.Width + dx_mm;
            double newHeight = item.Height + dy_mm;

            const double minSize = 5.0;
            if (newWidth < minSize) newWidth = minSize;
            if (newHeight < minSize) newHeight = minSize;

            newWidth = Math.Min(newWidth, vm.LabelWidthMm - item.X);
            newHeight = Math.Min(newHeight, vm.LabelHeightMm - item.Y);

            item.Width = newWidth;
            item.Height = newHeight;
        }
    }
}
