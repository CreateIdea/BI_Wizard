using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BI_Wizard.Helper
{
    public class OneClickDataGrid : DataGrid
    {
        private HashSet<ComboBox> _comboBoxSet;
        private HashSet<DataGridColumn> _dataGridColumnSet;
        static OneClickDataGrid()
        {
            //do not activate line below, otherwise the grid will not show any more. MGA
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(OneClickDataGrid), new FrameworkPropertyMetadata(typeof(OneClickDataGrid)));

        }

        public OneClickDataGrid()
        {
            PreviewMouseLeftButtonDown += DataGridCell_PreviewMouseLeftButtonDown;
            PreparingCellForEdit += DataGrid_PreparingCellForEdit;
            GotFocus += DataGrid_GotFocus;
            CellEditEnding += HandleMainDataGridCellEditEnding;
            _comboBoxSet = new HashSet<ComboBox>();
            _dataGridColumnSet = new HashSet<DataGridColumn>();

            var dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            if (dpd != null)
            {
                dpd.AddValueChanged(this, ThisIsCalledWhenPropertyIsChanged);
            }
        }

        private void ThisIsCalledWhenPropertyIsChanged(object sender, EventArgs e)
        {
            _dataGridColumnSet = new HashSet<DataGridColumn>();
        }


        private void DataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            // Lookup for the source to be DataGridCell
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
                // Starts the Edit on the row;
                ((DataGrid)sender).BeginEdit(e);
        }

        private void ComboBox_EventHandler(object sender, EventArgs e)
        {
            HandleMainDataGridCellEditEnding(null, null);
        }

        /// <summary>
        /// This method opens the DropDown on a combobox on first entry. When we ommit this method, the
        /// user must perform an additional mouse click after selecting the cell to open the DropDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            ComboBox comboBox = StaticClassExtensions.FindFirstChild<ComboBox>(e.EditingElement);
            if (comboBox != null)
            {
                if (comboBox.IsEnabled)
                {
                    if (!_comboBoxSet.Contains(comboBox))
                    {
                        _comboBoxSet.Add(comboBox);
                        comboBox.DropDownClosed += ComboBox_EventHandler;
                            //when the user selects a item from the drop down, via this event we will commit the row.
                    }
                    if (!comboBox.IsDropDownOpen)
                    {
                        comboBox.IsDropDownOpen = true;
                    }
                }
                else
                {
                    CancelEdit(DataGridEditingUnit.Cell);
                }
            }
            _dataGridColumnSet.Add(e.Column);
        }

        //private void HandleMainDataGridCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        //{
        //    if ((e == null) || _dataGridColumnSet.Contains(e.Column))
        //    {
        //        if (e != null)
        //        {
        //            _dataGridColumnSet.Remove(e.Column);
        //        }
        //        CommitEdit(DataGridEditingUnit.Row, true);
        //    }
        //}

        private void HandleMainDataGridCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e != null && e.Column != null && _dataGridColumnSet.Contains(e.Column))
                {
                    _dataGridColumnSet.Remove(e.Column);
                    CommitEdit(DataGridEditingUnit.Row, true);
                }
                else if ((e == null) && (sender == null))
                {
                    CommitEdit(DataGridEditingUnit.Row, true);
                }
            }
            catch (Exception ex)
            {
            }
        }

        //http://wpf.codeplex.com/wikipage?title=Single-Click%20Editing
        //http://stackoverflow.com/questions/10027182/how-to-set-an-evenhandler-in-wpf-to-all-windows-entire-application
        //http://www.scottlogic.com/blog/2008/12/02/wpf-datagrid-detecting-clicked-cell-and-row.html

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid == null) return;

                if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                {
                    if (!cell.IsSelected)
                        cell.IsSelected = true;
                }
                else
                {
                    DataGridRow row = FindVisualParent<DataGridRow>(cell);
                    if (row != null && !row.IsSelected)
                    {
                        row.IsSelected = true;
                    }
                }
            }
        }

        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
    }
}
