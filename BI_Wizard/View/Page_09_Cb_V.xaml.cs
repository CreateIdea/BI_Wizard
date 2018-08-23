using BI_Wizard.Helper;
using BI_Wizard.ViewModel;

namespace BI_Wizard.View
{
    /// <summary>
    /// Interaction logic for Page_05_CB_V.xaml
    /// </summary>
    public partial class Page_09_Cb_V : MgaXctkWizardPage
    {
        public  Page_09_Cb_Vm ViewModel;
        public Page_09_Cb_V()
        {
            InitializeComponent();
            ViewModel = (Page_09_Cb_Vm) DataContext;
        }

        public override void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {

        }
    }
}
