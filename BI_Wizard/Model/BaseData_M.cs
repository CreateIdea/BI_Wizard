using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Common;
using BI_Wizard.Helper;
using Microsoft.SqlServer.Management.Smo;


namespace BI_Wizard.Model
{
    [Serializable]
    public class BaseData_M : Base_VM
    {
        //public delegate void AppendLogLineDelegate(string aNewLogLine);
        //public AppendLogLineDelegate AppendLogLineEvent;
        //protected void AppendLogLine(string aNewLogLine)
        //{
        //    if (AppendLogLineEvent != null)
        //    {
        //        AppendLogLineEvent(aNewLogLine);
        //    }
        //}

        public void LoadSampleData(string tableName, string connectionString, Boolean isTable, out DataView TableViewDataView, out string Title)
        {
            //using (new Impersonator("w2k8r2ent", "Marian", "Iuliana10"))
            //{
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string sqlComand;
                    if (isTable)
                    {
                        sqlComand = string.Format("select top 100 * from {0} TABLESAMPLE (200 ROWS)", tableName);
                    }
                    else
                    {
                        sqlComand =
                            string.Format(
                                "select top 100 * from {0} ", tableName);
                        //sqlComand =
                        //    string.Format(
                        //        "select top 100 * from {0} WHERE (ABS(CAST((BINARY_CHECKSUM(*) * RAND()) as int)) % 2) < 1", tableName);
                    }
                    SqlCommand cmd = new SqlCommand(sqlComand, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable(tableName);
                    sda.Fill(dt);
                    TableViewDataView = dt.DefaultView;
                    string tv = isTable ? "table" : "view";
                    string rm = isTable ? "random" : "first";
                    Title = string.Format("Showing {2} maximum 100 records from {0} {1}", tv, tableName, rm);
                }
            //}
        }

        private StringBuilder _LogStrings=new StringBuilder();

        public string LogStrings
        {
            get { return _LogStrings.ToString(); }
        }

        protected void AppendLogLine(string aNewLogLine)
        {
            _LogStrings.AppendLine(aNewLogLine);
            NotifyPropertyChanged("LogStrings");
        }


        public string MakeConnectionString(string dbServerName, string dbName, bool integratedSecurity, string userId, string password)
        {
            //server=(local);database=pubs;trusted_connection=yes

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder["Data Source"] = dbServerName;  //can be as well (local)
            builder["Integrated Security"] = integratedSecurity;
            builder["Initial Catalog"] = dbName;
            builder["User Id"] = userId;
            builder["Password"] = password;

            return builder.ConnectionString;
        }

        private void OnSqlServerInfoMessage(Object sender, SqlInfoMessageEventArgs args)
        {
            AppendLogLine(string.Format("SQL Info Message:{0} Source{1}", args.Message, args.Source));
        }

        private void OnServerMessage(Object sender, ServerMessageEventArgs e)
        {
            AppendLogLine(string.Format("SQL Server Message:{0}", e.Error));
        }

        public bool ConnectToSqlServer(string dbServerName, string dbName, bool integratedSecurity, string userId, string password, 
                                       ref Server aServer, out string aConnectionString)
        {
            aServer = null;
            aConnectionString = null;

            if (string.IsNullOrWhiteSpace(dbName))
            {
                AppendLogLine(string.Format("Error: no database name specified."));
                return false;
            }
            string connStr = MakeConnectionString(dbServerName, dbName, integratedSecurity, userId, password);
            aConnectionString = connStr;
            AppendLogLine(string.Format("Connecting to {0} ...", connStr));
            SqlConnection sqlcon = new SqlConnection(connStr);
            ServerConnection srvcon = new ServerConnection(sqlcon);
            aServer = new Server(srvcon);
            try
            {
                aServer.Information.Initialize(true);
                //Display sql server info
                AppendLogLine("Connected!");
                AppendLogLine("  Name       = " + aServer.Name);
                AppendLogLine("  NetName    = " + aServer.Information.NetName);
                AppendLogLine("  Product    = " + aServer.Information.Product);
                AppendLogLine("  Edition    = " + aServer.Information.Edition);
                AppendLogLine("  Version    = " + aServer.Information.Version);
                AppendLogLine("  Platform   = " + aServer.Information.Platform);
                AppendLogLine("  Processors = " + aServer.Information.Processors);
                AppendLogLine("  Memory     = " + aServer.Information.PhysicalMemory);
                AppendLogLine("");
                //aServer.ConnectionContext.InfoMessage += OnSqlServerInfoMessage;
                //aServer.ConnectionContext.ServerMessage += OnServerMessage;
                return true;
            }
            catch (Exception e)
            {
                AppendLogLine("Error :" + e.Message);
                if (e.InnerException != null)
                {
                    string strInnerEx = e.InnerException.ToString();
                    //remove unneccessary error text
                    strInnerEx = strInnerEx.Remove(strInnerEx.IndexOf('('));
                    AppendLogLine("Error :" + strInnerEx);
                }
                aServer = null;
                return false;
            }
        }
    }
}
