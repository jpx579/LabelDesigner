using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LabelDesigner.Models;
using System.Windows.Controls;

namespace LabelDesigner.Behaviors
{
    public class DragMoveBehavior : Behavior<FrameworkElement>
    {
        private Point _startPoint;
        private double _origX, _origY;
        private bool _isDragging;

        // 绑定 Canvas 宽高
        public double CanvasWidth
        {
            get => (double)GetValue(CanvasWidthProperty);
            set => SetValue(CanvasWidthProperty, value);
        }
        public static readonly DependencyProperty CanvasWidthProperty =
            DependencyProperty.Register(nameof(CanvasWidth), typeof(double), typeof(DragMoveBehavior), new PropertyMetadata(0.0));

        public double CanvasHeight
        {
            get => (double)GetValue(CanvasHeightProperty);
            set => SetValue(CanvasHeightProperty, value);
        }
        public static readonly DependencyProperty CanvasHeightProperty =
            DependencyProperty.Register(nameof(CanvasHeight), typeof(double), typeof(DragMoveBehavior), new PropertyMetadata(0.0));

        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove += OnMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove -= OnMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
            base.OnDetaching();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = FindParentCanvas(AssociatedObject);
            if (canvas == null) return;

            _isDragging = true;
            _startPoint = e.GetPosition(canvas);

            if (AssociatedObject.DataContext is ElementInfo element)
            {
                _origX = element.X;
                _origY = element.Y;
            }

            AssociatedObject.CaptureMouse();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            var canvas = FindParentCanvas(AssociatedObject);
            if (canvas == null) return;

            if (AssociatedObject.DataContext is ElementInfo element)
            {
                var pos = e.GetPosition(canvas);
                double dx = pos.X - _startPoint.X;
                double dy = pos.Y - _startPoint.Y;

                element.X = Math.Max(0, Math.Min(CanvasWidth - element.Width, _origX + dx));
                element.Y = Math.Max(0, Math.Min(CanvasHeight - element.Height, _origY + dy));
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            AssociatedObject.ReleaseMouseCapture();
        }

        private Canvas FindParentCanvas(DependencyObject obj)
        {
            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null && parent is not Canvas)
                parent = VisualTreeHelper.GetParent(parent);

            return parent as Canvas;
        }
    }
}
