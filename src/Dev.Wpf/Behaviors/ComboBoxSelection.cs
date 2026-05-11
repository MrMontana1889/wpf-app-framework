using Dev.Core.Toolbar;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dev.Wpf.Behaviors
{
    public static class ComboBoxSelection
    {
        public static readonly DependencyProperty EnableSelectionForwardingProperty =
            DependencyProperty.RegisterAttached(
                "EnableSelectionForwarding",
                typeof(bool),
                typeof(ComboBoxSelection),
                new PropertyMetadata(false, OnEnableSelectionForwardingChanged));

        public static void SetEnableSelectionForwarding(DependencyObject d, bool value)
            => d.SetValue(EnableSelectionForwardingProperty, value);

        public static bool GetEnableSelectionForwarding(DependencyObject d)
            => (bool)d.GetValue(EnableSelectionForwardingProperty);

        private static void OnEnableSelectionForwardingChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ComboBox comboBox)
                return;

            comboBox.SelectionChanged -= OnSelectionChanged;

            if ((bool)e.NewValue)
                comboBox.SelectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox)
                return;

            if (comboBox.DataContext is not ToolbarItem toolbarItem)
                return;

            if (e.AddedItems.Count != 1)
                return;

            object? selectedValue = e.AddedItems[0];

            // ✅ This is the semantic execution point
            toolbarItem.SetSelectedValue(selectedValue);
        }
    }
}
