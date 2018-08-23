using BI_Wizard.Helper;
using BI_Wizard.Model;

namespace BI_Wizard.ViewModel
{
    public class Page_01_Ds_Vm:Base_VM
    {
        public Analysis_M AnalysisM
        {
            get { return App_M.Instance.AnalysisM; }
        }

        public void Update()
        {
            NotifyAllPropertyChanged();
        }
    }
}
