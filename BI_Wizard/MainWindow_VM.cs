using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using BI_Wizard.Helper;
using BI_Wizard.Model;
using BI_Wizard.ViewModel;

namespace BI_Wizard
{
    public class MainWindow_VM:Base_VM
    {
        public App_M AppM
        {
            get { return App_M.Instance; }
        }

        ////NetDataContractSerializer
        //public void LoadProjectNetData(string aProjectFullFileName)
        //{
        //    AppM.LoadedProjectFileName = aProjectFullFileName;
        //    if (File.Exists(aProjectFullFileName))
        //    {
        //        NetDataContractSerializer aDeserializer = new NetDataContractSerializer();
        //        FileStream aFileStream = new FileStream(aProjectFullFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        //        AppM.AnalysisM = (Analysis_M) aDeserializer.Deserialize(aFileStream);
        //    }
        //}
        
        ////NetDataContractSerializer
        //public void SaveProjectNetData(string aProjectFullFileName)
        //{
        //    AppM.LoadedProjectFileName = aProjectFullFileName;
        //    NetDataContractSerializer aSerializer = new NetDataContractSerializer();
        //    FileStream aFileStream = new FileStream(aProjectFullFileName, FileMode.Create, FileAccess.Write, FileShare.None);
        //    aSerializer.Serialize(aFileStream, AppM.AnalysisM);
        //    aFileStream.Close();
        //}

        ////BinarySerializer
        //public void LoadProjectBinary(string aProjectFullFileName)
        //{
        //    AppM.LoadedProjectFileName = aProjectFullFileName;
        //    if (File.Exists(aProjectFullFileName))
        //    {
        //        IFormatter aFormatter = new BinaryFormatter();
        //        FileStream aFileStream = new FileStream(aProjectFullFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        //        AppM.AnalysisM = (Analysis_M) aFormatter.Deserialize(aFileStream);
        //    }
        //}

        ////BinarySerializer
        //public void SaveProjectBinary(string aProjectFullFileName)
        //{
        //    AppM.LoadedProjectFileName = aProjectFullFileName;
        //    IFormatter aFormatter = new BinaryFormatter();
        //    FileStream aFileStream = new FileStream(aProjectFullFileName, FileMode.Create, FileAccess.Write, FileShare.None);
        //    aFormatter.Serialize(aFileStream, AppM.AnalysisM);
        //    aFileStream.Close();
        //}

        //xmlserializer
        public void LoadProject(string aProjectFullFileName)
        {
            AppM.LoadedProjectFileName = aProjectFullFileName;
            if (File.Exists(aProjectFullFileName))
            {
                SerializableObject.ClearLists();
                //http://stackoverflow.com/questions/2209443/c-sharp-xmlserializer-bindingfailure
                XmlSerializer deserializer = XmlSerializer.FromTypes(new[] { typeof(Analysis_M) })[0];
                TextReader reader = new StreamReader(aProjectFullFileName);
                AppM.AnalysisM = (Analysis_M) deserializer.Deserialize(reader);
                reader.Close();
                SerializableObject.FixAllReferences();
            }
        }

        //xmlserializer
        public void SaveProject(string aProjectFullFileName)
        {
            AppM.LoadedProjectFileName = aProjectFullFileName;
            XmlSerializer serializer = XmlSerializer.FromTypes(new[] { typeof(Analysis_M) })[0];
            TextWriter writer = new StreamWriter(aProjectFullFileName);
            serializer.Serialize(writer, AppM.AnalysisM);
            writer.Close();
        }

        public void NewProject()
        {
            if (File.Exists(AppM.LoadedProjectFileName))
            {
                SaveProject(AppM.LoadedProjectFileName);
            }
            AppM.LoadedProjectFileName = "";
            AppM.AnalysisM = new Analysis_M();
        }
    }
}
