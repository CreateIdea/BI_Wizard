using System;
using System.Windows;
using System.Windows.Controls;

namespace BI_Wizard.Helper
{
    public class ComboBoxDropDownClosed :ComboBox
    {
        public object OnDropDownClosedSelectedItem
        {
            get
            {
                return (object)GetValue(OnDropDownClosedSelectedItemProperty);
            }
            set
            {
                if (value !=null)
                   SetValue(OnDropDownClosedSelectedItemProperty, value);
            }
        }

        public static readonly DependencyProperty OnDropDownClosedSelectedItemProperty =
            DependencyProperty.Register("OnDropDownClosedSelectedItem", typeof(object), typeof(ComboBoxDropDownClosed), new PropertyMetadata(null));


        public ComboBoxDropDownClosed()
        {
            this.DropDownClosed += ComboBox_EventHandler;
        }

        private void ComboBox_EventHandler(object sender, EventArgs e)
        {
            OnDropDownClosedSelectedItem = SelectedItem;
        }
    }
}
