using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace MahApps.Metro.Actions
{
    public class SetIsPinnedAction : TargetedTriggerAction<FrameworkElement>
    {
        public static readonly DependencyProperty PinValueProperty = DependencyProperty.Register("PinValue", typeof(bool), typeof(SetFlyoutOpenAction), new PropertyMetadata(default(bool)));

        public bool PinValue
        {
            get { return (bool)GetValue(PinValueProperty); }
            set { SetValue(PinValueProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            var toggleButton = (parameter as RoutedEventArgs).OriginalSource as ToggleButton;
            if (toggleButton != null)
            {
                ((Flyout)TargetObject).IsPinned = (bool)toggleButton.IsChecked;
            }
        }
    }
}
