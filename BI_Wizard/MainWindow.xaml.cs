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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;
using Microsoft.AnalysisServices;
using System.Data.SqlClient;
using System.Data.OleDb;
using Ookii.Dialogs.Wpf;
using BI_Wizard.Model;
using BI_Wizard.ViewModel;
using System.IO;
using BI_Wizard.Helper;
using BI_Wizard.View;

namespace BI_Wizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow_VM ViewModel;
        
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = (MainWindow_VM) DataContext;
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "Data warehouse and cube wizard (*.dsdwcb)|*.dsdwcb";
            dialog.InitialDirectory = ViewModel.AppM.DataDirectory;
            dialog.FileName = ViewModel.AppM.DataDirectory;
            dialog.RestoreDirectory = true;
            dialog.AddExtension = true;
            dialog.DefaultExt = "dsdwcb";
            if ((bool) dialog.ShowDialog())
            {
                WizardMain.CurrentPage = Page01DsV;
                ViewModel.LoadProject(dialog.FileName);
                Page01DsV.ViewModel.Update();
                Page02DwV.ViewModel.Update();
                Page03DsV.ViewModel.Update();
                Page04DsDwV.ViewModel.Update();
                Page05DwV.ViewModel.Update();
                Page06DwV.ViewModel.Update();
                Page08CbV.ViewModel.Update();
                Page09CbV.ViewModel.Update();
                Page10CbV.ViewModel.Update();
            }
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(ViewModel.AppM.LoadedProjectFileName))
            {
                ViewModel.SaveProject(ViewModel.AppM.LoadedProjectFileName);
            }
            else
            {
                MenuItemSaveAs_Click(sender, e);
            }
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            VistaSaveFileDialog dialog = new VistaSaveFileDialog();
            dialog.Filter = "Data warehouse and cube wizard (*.dsdwcb)|*.dsdwcb";
            dialog.InitialDirectory = ViewModel.AppM.DataDirectory;
            dialog.FileName = ViewModel.AppM.DataDirectory;
            dialog.RestoreDirectory = true;
            dialog.AddExtension = true;
            dialog.DefaultExt = "dsdwcb";
            if ((bool) dialog.ShowDialog())
            {
                ViewModel.SaveProject(dialog.FileName);
            }
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            WizardMain.CurrentPage = Page01DsV;
            ViewModel.NewProject();
        }

        private void WizardMain_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            if (WizardMain.CurrentPage == Page02DwV && ViewModel.AppM.AnalysisM.DwSkipSelectSourceTables)
            {
                WizardMain.CurrentPage = Page03DsV;
            }
        }

        private void tbLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbLog.ScrollToEnd();
        }
    }

}
