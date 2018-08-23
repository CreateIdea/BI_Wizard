using System;
using System.Windows;
using System.Windows.Controls;

namespace BI_Wizard.Helper
{
 
    public class GridSelectingItem
    {
        public static readonly DependencyProperty SelectingItemProperty = DependencyProperty.RegisterAttached(
            "SelectingItem",
            typeof(Object),
            typeof(GridSelectingItem),
            new PropertyMetadata(default(Object), OnSelectingItemChanged));

        public static Object GetSelectingItem(DependencyObject target)
        {
            return (Object)target.GetValue(SelectingItemProperty);
        }

        public static void SetSelectingItem(DependencyObject target, GridSelectingItem value)
        {
            target.SetValue(SelectingItemProperty, value);
        }

        static void OnSelectingItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || grid.SelectedItem == null)
                return;

            // Works with .Net 4.5
            grid.Dispatcher.InvokeAsync(() =>
            {
                grid.UpdateLayout();
                if (grid.SelectedItem!=null) grid.ScrollIntoView(grid.SelectedItem, null);
            });

            // Works with .Net 4.0
            //grid.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    grid.UpdateLayout();
            //    grid.ScrollIntoView(grid.SelectedItem, null);
            //}));
        }
    }
}
