using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using BI_Wizard.Helper;
using BI_Wizard.Properties;

namespace BI_Wizard.Model
{
    //Application Global Data
    //This is a singleton
    public sealed class App_M : Base_VM
    {
        public static readonly App_M Instance = new App_M();
        private string _LoadedProjectFileName;
        private Analysis_M _AnalysisM;

        private App_M()
        {
            AnalysisM = new Analysis_M();
        }

        public string CommonAppData
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); }
        }

        public string DataDirectory
        {
            get
            {
                string _dataDirectory = String.Empty;
                if (String.IsNullOrEmpty(Settings.Default.DataDirectory) ||
                    !Directory.Exists(Settings.Default.DataDirectory))
                {
                    _dataDirectory = Path.Combine(CommonAppData, @"DsDwCb\");
                    if (!Directory.Exists(_dataDirectory))
                    {
                        try
                        {
                            Directory.CreateDirectory(_dataDirectory);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(
                                string.Format("Error bij het aanmaken van Data Directory:" + Environment.NewLine +
                                              "{0}" + Environment.NewLine +
                                              "De applicatie zal nu sluiten." + Environment.NewLine +
                                              "Neem contact op met uw systeem beheerder!", _dataDirectory),
                                "Data Directory", MessageBoxButton.OK, MessageBoxImage.Error);
                            throw;
                        }
                    }
                }
                else
                {
                    _dataDirectory = Settings.Default.DataDirectory;
                }
                return _dataDirectory;
            }
        }

        public string LoadedProjectFileName
        {
            get { return _LoadedProjectFileName; }
            set { SetProperty(ref _LoadedProjectFileName, value); }
        }

        
        public Analysis_M AnalysisM
        {
            get { return _AnalysisM; }
            set
            {
                if (SetProperty(ref _AnalysisM, value))
                {
                    //NotifyAllPropertyChanged();
                }
            }
        }
    }
}