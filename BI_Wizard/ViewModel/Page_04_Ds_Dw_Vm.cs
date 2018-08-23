using BI_Wizard.Helper;
using BI_Wizard.Model;

namespace BI_Wizard.ViewModel
{
    public class Page_04_Ds_Dw_Vm : Base_VM
    {
        private DsDwTableMap_M _SelectedDsDwTableMap;

        public Analysis_M AnalysisM
        {
            get { return App_M.Instance.AnalysisM; }
        }
        public void Update()
        {
            NotifyAllPropertyChanged();
        }

        public DsDwTableMap_M SelectedDsDwTableMap
        {
            get { return _SelectedDsDwTableMap; }
            set { SetProperty(ref _SelectedDsDwTableMap, value); }
        }

        public void IncludeAllDw()
        {
            SetIncludeAllDw(true);
        }

        public void SetIncludeAllDw(bool value)
        {
            if (SelectedDsDwTableMap != null && SelectedDsDwTableMap.DsDwColumnMapList != null && SelectedDsDwTableMap.DsDwColumnMapList.Count > 0)
            {
                foreach (DsDwColumnMap_M aDsDwColumnMap in SelectedDsDwTableMap.DsDwColumnMapList)
                {
                    if (aDsDwColumnMap != null)
                    {
                        if (aDsDwColumnMap.IsIncludeEditable)
                        {
                            aDsDwColumnMap.Include = value;
                        }
                    }
                }
            }
        }

        public void IncludeNoneDw()
        {
            SetIncludeAllDw(false);
        }
    }
}