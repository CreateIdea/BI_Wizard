using BI_Wizard.Helper;
using BI_Wizard.Model;

namespace BI_Wizard.ViewModel
{
    public class Page_09_Cb_Vm : Base_VM
    {
        public Analysis_M AnalysisM
        {
            get { return App_M.Instance.AnalysisM; }
        }

        private CbMeasureGroup_M _SelectedDwCbMeasureGroupMap;
        public CbMeasureGroup_M SelectedDwCbMeasureGroupMap
        {
            get { return _SelectedDwCbMeasureGroupMap; }
            set { SetProperty(ref _SelectedDwCbMeasureGroupMap, value); }
        }

        private CbDimension_M _SelectedDwCbDimensionMap;
        public CbDimension_M SelectedDwCbDimensionMap
        {
            get { return _SelectedDwCbDimensionMap; }
            set { SetProperty(ref _SelectedDwCbDimensionMap, value); }
        }

        public void Update()
        {
            NotifyAllPropertyChanged();
        }

    }
}
