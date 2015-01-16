using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
namespace MahApps.Metro.Behaviours
{
    /// <summary>
    /// Allows an <see cref="ItemsControl"/> to scroll horizontally by listening to the
    /// </summary>
    public class HorizontalScrollBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// A reference to the internal ScrollViewer.
        /// </summary>
        private ScrollViewer ScrollViewer { get; set; }

        /// <summary>
        /// By default, scrolling down on the wheel translates to right, and up to left.
        /// Set this to true to invert that translation.
        /// </summary>
        public bool IsInverted { get; set; }

        /// <summary>
        /// The ScrollViewer is not available in the visual tree until the control is loaded.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= OnLoaded;
            ScrollViewer = VisualTreeHelpers.FindChild<ScrollViewer>(AssociatedObject);

            if (ScrollViewer != null)
            {
                ScrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (ScrollViewer != null)
            {
                ScrollViewer.PreviewMouseWheel -= OnPreviewMouseWheel;
            }
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var newOffset = IsInverted ?
                ScrollViewer.HorizontalOffset + e.Delta :
                ScrollViewer.HorizontalOffset - e.Delta;

            ScrollViewer.ScrollToHorizontalOffset(newOffset);
        }
    }
}
