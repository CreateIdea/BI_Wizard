using Xceed.Wpf.Toolkit;

namespace BI_Wizard.Helper
{
    class MgaXctkWizard : Wizard
    {
        public MgaXctkWizard()
        {
            Next += Wizard_Next;
        }

        private void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            MgaXctkWizardPage page = CurrentPage as MgaXctkWizardPage;
            if (page != null)
            {
                page.Wizard_Next(sender, e);
            }
        }
    }
}
