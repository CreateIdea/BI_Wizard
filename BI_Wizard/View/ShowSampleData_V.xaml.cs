using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BI_Wizard.ViewModel;

namespace BI_Wizard.View
{
    /// <summary>
    /// Interaction logic for ShowSampleData.xaml
    /// </summary>
    public partial class ShowSampleData_V : Window
    {
        public ShowSampleData_Vm ViewModel;
        public ShowSampleData_V()
        {
            InitializeComponent();
            ViewModel = (ShowSampleData_Vm) DataContext;
        }

    }
}
