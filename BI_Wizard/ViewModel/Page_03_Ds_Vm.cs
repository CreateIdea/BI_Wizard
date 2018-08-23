using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using BI_Wizard.Helper;
using BI_Wizard.Model;

namespace BI_Wizard.ViewModel
{
    public class Page_03_Ds_Vm : Base_VM
    {
        public Analysis_M AnalysisM
        {
            get { return App_M.Instance.AnalysisM; }
        }


        private int _SelectedTableView=0;

        [XmlIgnore]
        public string FilterString
        {
            get { return _filterString; }
            set
            {
                if (SetProperty(ref _filterString, value))
                {
                    UpdateFilter();
                }
            }
        }

        public void UpdateFilter()
        {
            if (SelectedTableView == 0 && String.IsNullOrWhiteSpace(FilterString))
            {
                AnalysisM.TableViewListView.Filter = null;
            }
            else
            {
                if (SelectedTableView == 0)
                {
                    AnalysisM.TableViewListView.Filter = new Predicate<object>(item => (((TableView_M) item).Name.IndexOf(_filterString, StringComparison.OrdinalIgnoreCase) >= 0));
                }
                else
                {
                    bool isTable;
                    if (SelectedTableView == 1)
                    {
                        isTable = true;
                    }
                    else
                    {
                        isTable = false;
                    }

                    if (String.IsNullOrWhiteSpace(FilterString))
                    {
                        AnalysisM.TableViewListView.Filter = new Predicate<object>(item => ((((TableView_M) item).IsTable == isTable)));
                    }
                    else
                    {
                        AnalysisM.TableViewListView.Filter = new Predicate<object>(item => ((((TableView_M) item).Name.IndexOf(_filterString, StringComparison.OrdinalIgnoreCase) >= 0) &&
                                                                                            (((TableView_M) item).IsTable == isTable)));
                    }
                }
                //AnalysisM.TableViewListView.Refresh();
                //AnalysisM.NotifyDsTableViewList();
            }
            
        }

        [XmlIgnore]
        public int SelectedTableView
        {
            get { return _SelectedTableView; }
            set
            {
                if (SetProperty(ref _SelectedTableView, value))
                {
                    UpdateFilter();
                }
            }
        }
      

        private string _filterString;

        public void MoveSelectedToDw()
        {
            foreach (TableView_M aTableViewM in AnalysisM.DsTableViewList.Where(tv => tv.IsSelected).ToList())
            {
                AnalysisM.DsTableViewList.Remove(aTableViewM);
                aTableViewM.IsSelected = false;
                AnalysisM.DwTableViewList.Add(aTableViewM);
            }
        }

        public void Update()
        {
            NotifyAllPropertyChanged();
        }

        public void MoveSelectedToDs()
        {
            foreach (TableView_M aTableViewM in AnalysisM.DwTableViewList.Where(tv => tv.IsSelected).ToList())
            {
                AnalysisM.DwTableViewList.Remove(aTableViewM);
                aTableViewM.IsSelected = false;
                AnalysisM.DsTableViewList.Add(aTableViewM);
            }
        }

    }
}
