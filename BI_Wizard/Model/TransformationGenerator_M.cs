using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Agent;
using BI_Wizard.Helper;

namespace BI_Wizard.Model
{
    [Serializable]
    public class TransformationGenerator_M : DataWarehouse_M
    {
        public string TgStoredProcedureETL;
        public string TgExecStoredProcedureETL;
        public string TgJobNameExecStoredProcedureETL;
        private Job _TgJob;

        public bool ExecuteETL()
        {
            try
            {
                AppendLogLine("");
                string aSpName=string.Format("[{0}].[{1}]", DwDb.DefaultSchema, TgStoredProcedureETL);

                DataSet aDataSet= DwDb.ExecuteWithResults(string.Format("EXEC {0}", aSpName));
                int totRecordsChanged=0;
                foreach (DataTable t in aDataSet.Tables)
                {
                    foreach (DataRow r in t.Rows)
                    {
                        totRecordsChanged = (int) r["NumRowsChanged"];
                        break;
                    }
                    break;
                }
                AppendLogLine(string.Format("Successfully executed ETL stored procedure: {0}.", aSpName));
                AppendLogLine(string.Format("Total nr of records affected: {0}", totRecordsChanged));
                return true;
            }
            catch (Exception ex)
            {
                AppendLogLine(string.Format("Exception occured when executing stored procedure: [{0}].[{1}]", DwDb.DefaultSchema, TgStoredProcedureETL));
                AppendLogLine(string.Format("Exception: {0}", ex.Message));
                if (ex.InnerException != null)
                    AppendLogLine(string.Format("Inner Exception:{0}", ex.InnerException.Message));
                return false;
            }

        }

        private void TgGenerateETL()
        {
            StoredProcedure sp;
            TgStoredProcedureETL = string.Format("ETL_{0}_{1}", DsDbServerName, DsDbName);
            TgExecStoredProcedureETL = string.Format("[{0}].[{1}]", DwDb.DefaultSchema, TgStoredProcedureETL);
            TgJobNameExecStoredProcedureETL = string.Format("Execute {0}", TgStoredProcedureETL);
            sp = new StoredProcedure(DwDb, TgStoredProcedureETL);
            //Set the TextMode property to false and then set the other object properties. 
            sp.TextMode = false;
            sp.AnsiNullsStatus = true;
            sp.QuotedIdentifierStatus = true;

            //Set the TextBody property to define the stored procedure. 
            string stmt= Environment.NewLine + "DECLARE @NumRowsChanged int;" + Environment.NewLine + "Set @NumRowsChanged = 0;";
            if (DsDwMap.DwTableMapTopologicalSortList != null)
            {
                foreach (ICollection<DsDwTableMap_M> dsDwTableMapLevelList in DsDwMap.DwTableMapTopologicalSortList)
                {
                    foreach (DsDwTableMap_M dsDwTableMap in dsDwTableMapLevelList)
                    {
                        stmt = stmt + GenerateScd1(dsDwTableMap) + Environment.NewLine +
                               "Set @NumRowsChanged = @NumRowsChanged  + @@ROWCOUNT" + Environment.NewLine;
                    }
                }
            }
            sp.TextBody = stmt + "Select @NumRowsChanged as NumRowsChanged" + Environment.NewLine;
            //Create the stored procedure on the instance of SQL Server. 
            sp.Create();
        }

        private void TgGenerateJob()
        {
            if (_DwServer.JobServer.Jobs.Contains(TgJobNameExecStoredProcedureETL))
            {
                _TgJob = _DwServer.JobServer.Jobs[TgJobNameExecStoredProcedureETL];
                _TgJob.Drop();
            }
            _TgJob = new Job(_DwServer.JobServer, TgJobNameExecStoredProcedureETL);
            _TgJob.Create();
            JobStep aJobStep = new JobStep(_TgJob, "Execute ETL");
            aJobStep.DatabaseName = DwDbName;
            aJobStep.SubSystem = AgentSubSystem.TransactSql;
            aJobStep.Command = string.Format("Exec {0}", TgExecStoredProcedureETL);
            aJobStep.OnSuccessAction = StepCompletionAction.QuitWithSuccess;
            aJobStep.OnFailAction = StepCompletionAction.QuitWithFailure;
            aJobStep.Create();
            JobSchedule SQLSchedule = new JobSchedule(_TgJob, "Execute ETL daily at night");
            SQLSchedule.FrequencyTypes = FrequencyTypes.Daily;
            SQLSchedule.FrequencySubDayTypes = FrequencySubDayTypes.Once;
            SQLSchedule.ActiveStartTimeOfDay = new TimeSpan(0, 16, 0);
            SQLSchedule.FrequencyInterval = 1;
            SQLSchedule.ActiveStartDate = DateTime.Now;
            SQLSchedule.ActiveEndDate = new DateTime(9999, 12, 30);
            SQLSchedule.ActiveEndTimeOfDay = new TimeSpan(23, 59, 0);
            SQLSchedule.Create();
            _TgJob.ApplyToTargetServer(DwDbServerName);
            _TgJob.Refresh();
        }

        public bool DwGenerateDbJobETL()
        {
            try
            {
                AppendLogLine("");
                DwGenerateDb();
                DwGenerateTables();
                TgGenerateETL();
                TgGenerateJob();
                return true;
            }
            catch (Exception ex)
            {
                AppendLogLine("Exception occured when generating data warehouse database.");
                AppendLogLine(string.Format("Exception:{0}", ex.Message));
                if (ex.InnerException != null)
                    AppendLogLine(string.Format("Inner Exception:{0}", ex.InnerException.Message));
                return false;
            }
        }


        private Boolean _TgFixDates;

        public bool TgFixDates
        {
            get { return _TgFixDates; }
            set { SetProperty(ref _TgFixDates, value); }
        }

        private string GenerateScd1Target(DsDwTableMap_M aDsDwTableMap)
        {
            string aResultString=String.Empty;
            aResultString = string.Format("[{0}].[{1}].[{2}]",
                                           DwDbName,
                                           aDsDwTableMap.DwSchemaName,
                                           aDsDwTableMap.DwTableName);
            return aResultString;
        }

        private string GenerateScd1Source(DsDwTableMap_M aDsDwTableMap)
        {
            string aResultString=String.Empty;
            aResultString = string.Format("[{0}].[{1}].[{2}].[{3}]",
                                            aDsDwTableMap.DsServerName,
                                            aDsDwTableMap.DsDatabaseName,
                                            aDsDwTableMap.DsSchemaName,
                                            aDsDwTableMap.DsTableName);
            return aResultString;
        }

        //MATCHED AND

        private string GenerateScd1MatchedAnd(DsDwTableMap_M aDsDwTableMap)
        {
            string aResultString=String.Empty;
            foreach (DsDwColumnMap_M aDsDwColumnMap in aDsDwTableMap.DsDwColumnMapList.Where(cm => cm.Include &&
                                                                                                   (cm.Transformation == DsDwColumnTransformation.Scd1 ||
                                                                                                    cm.Transformation == DsDwColumnTransformation.ForeignKey)))
            {
                if (aDsDwColumnMap.Transformation == DsDwColumnTransformation.Scd1)
                {
                    aResultString = aResultString +
                                    string.Format(
                                        "    ([source].[{0}] <> [target].[{1}] OR{2}    ([source].[{0}] IS NULL AND [target].[{1}] IS NOT NULL) OR{2}    ([source].[{0}] IS NOT NULL AND [target].[{1}] IS NULL)) OR{2}",
                                        aDsDwColumnMap.DsColumn.Name, aDsDwColumnMap.DwColumn.Name, Environment.NewLine);
                }
                else
                { 

                    //ForeignKey
                    aResultString = aResultString +
                                    string.Format(
                                        "    ([source].[{0}] <> [target].[{1}] OR{2}    ([source].[{0}] IS NULL AND [target].[{1}] IS NOT NULL) OR{2}    ([source].[{0}] IS NOT NULL AND [target].[{1}] IS NULL)) OR{2}",
                                        aDsDwColumnMap.DwColumn.Name, aDsDwColumnMap.DwColumn.Name, Environment.NewLine);

                }
            }
            aResultString = RemoveAtEnd(aResultString, " OR" + Environment.NewLine);
            return aResultString;
        }

        private string RemoveAtEnd(string aString, string aRemoveString)
        {
            if (aString.EndsWith(aRemoveString))
            {
                aString = aString.Remove(aString.Length - aRemoveString.Length);
            }
            return aString;
        }

        private string GenerateScd1UpdateSet(DsDwTableMap_M aDsDwTableMap)
        {
            string aResultString=String.Empty;
            foreach (DsDwColumnMap_M aDsDwColumnMap in aDsDwTableMap.DsDwColumnMapList.Where(cm => cm.Include &&
                                                                                                   (cm.Transformation == DsDwColumnTransformation.Scd1 ||
                                                                                                    cm.Transformation == DsDwColumnTransformation.ForeignKey)))
            {
                if (aDsDwColumnMap.Transformation == DsDwColumnTransformation.Scd1)
                {
                    aResultString = aResultString +
                                    string.Format("    [target].[{0}] = [source].[{1}],{2}",
                                        aDsDwColumnMap.DwColumn.Name, aDsDwColumnMap.DsColumn.Name, Environment.NewLine);
                }
                else
                {
                    aResultString = aResultString +
                                    string.Format("    [target].[{0}] = [source].[{1}],{2}",
                                        aDsDwColumnMap.DwColumn.Name, aDsDwColumnMap.DwColumn.Name, Environment.NewLine);
                }
            }
            aResultString = RemoveAtEnd(aResultString, "," + Environment.NewLine);
            return aResultString;
        }

        private string GenerateScd1Insert(DsDwTableMap_M aDsDwTableMap)
        {
            string aResultString = "(" + Environment.NewLine;
            foreach (DsDwColumnMap_M aDsDwColumnMap in aDsDwTableMap.DsDwColumnMapList.Where(cm => cm.Include &&
                                                                                            (cm.Transformation == DsDwColumnTransformation.Scd1 ||
                                                                                             cm.Transformation == DsDwColumnTransformation.ForeignKey ||
                                                                                             cm.Transformation == DsDwColumnTransformation.BusinesKey)))
            {
                aResultString = aResultString + string.Format("    [{0}],{1}", aDsDwColumnMap.DwColumn.Name, Environment.NewLine);
            }
            DsDwColumnMap_M aDsDwColumnMapIsDeleted = aDsDwTableMap.DsDwColumnMapList.FirstOrDefault(cm => cm.Include && cm.Transformation == DsDwColumnTransformation.IsDeleted);
            if (aDsDwColumnMapIsDeleted != null)
            {
                aResultString = aResultString + string.Format("    [{0}],{1}", aDsDwColumnMapIsDeleted.DwColumn.Name, Environment.NewLine);
            }
            aResultString = RemoveAtEnd(aResultString, "," + Environment.NewLine);
            aResultString = aResultString + string.Format("{0}){0}VALUES{0}({0}", Environment.NewLine);
            foreach (DsDwColumnMap_M aDsDwColumnMap in aDsDwTableMap.DsDwColumnMapList.Where(cm => cm.Include &&
                                                                                            (cm.Transformation == DsDwColumnTransformation.Scd1 ||
                                                                                             cm.Transformation == DsDwColumnTransformation.ForeignKey ||
                                                                                             cm.Transformation == DsDwColumnTransformation.BusinesKey)))
            {
                if (aDsDwColumnMap.Transformation == DsDwColumnTransformation.ForeignKey)
                {
                    aResultString = aResultString +
                                    string.Format("    [source].[{0}],{1}", aDsDwColumnMap.DwColumn.Name,
                                        Environment.NewLine);
                }
                else
                {
                    aResultString = aResultString +
                                    string.Format("    [source].[{0}],{1}", aDsDwColumnMap.DsColumn.Name,
                                        Environment.NewLine);
                }
            }
            if (aDsDwColumnMapIsDeleted != null)
            {
                aResultString = aResultString + "    0," + Environment.NewLine;
            }
            aResultString = RemoveAtEnd(aResultString, "," + Environment.NewLine);
            aResultString = aResultString + Environment.NewLine + ")";
            if (aDsDwColumnMapIsDeleted != null)
            {
                aResultString = aResultString + Environment.NewLine + string.Format(@"WHEN NOT MATCHED BY SOURCE AND
	([{0}] = 0 OR [{0}] IS NULL)
THEN UPDATE
SET
	[{0}] = 1
", aDsDwColumnMapIsDeleted.DwColumn.Name);
            }
            aResultString = aResultString + ";" + Environment.NewLine;
            return aResultString;
        }


        private string GenerateScd1SourceSelect(DsDwTableMap_M aDsDwTableMap)
        {
            string aResultString=String.Empty;
            //add normal columns 
            foreach (DsDwColumnMap_M aDsDwColumnMap in aDsDwTableMap.DsDwColumnMapList.Where(cm => cm.Include &&
                                                                                                  (cm.Transformation == DsDwColumnTransformation.Scd1 ||
                                                                                                   cm.Transformation == DsDwColumnTransformation.BusinesKey)))
            {
                aResultString = aResultString + string.Format("        [{0}],", aDsDwColumnMap.DsColumn.Name) + Environment.NewLine;
            }

            DsDwTableMap_M aDsDwTableMapPrimaryKey;
            DsDwColumnMap_M aDsDwColumnMapReferencedSurogatePrimaryKey;
            DsDwColumnMap_M aDsDwColumnMapReferencedPrimaryKey;

            //Add foreign keys
            foreach (DsDwColumnMap_M aDsDwColumnMapForeignKey in aDsDwTableMap.DsDwColumnMapList.Where(cm => cm.Include &&
                                                                                                       cm.Transformation == DsDwColumnTransformation.ForeignKey))
            {
                aDsDwTableMapPrimaryKey = aDsDwColumnMapForeignKey.DwForeignKeyReferencedTableMap;
                if (aDsDwTableMapPrimaryKey != null)
                {
                    aDsDwColumnMapReferencedSurogatePrimaryKey = aDsDwTableMapPrimaryKey.DsDwColumnMapList.FirstOrDefault(
                        cm => cm.Include && cm.Transformation == DsDwColumnTransformation.SurrogateKey);

                    //for views
                    aDsDwColumnMapReferencedPrimaryKey = aDsDwTableMapPrimaryKey.DsDwColumnMapList.FirstOrDefault(
                        cm => cm.Include && cm.DsReferencedColumn != null && cm.DsReferencedColumn.InPrimaryKey);
                    if (aDsDwColumnMapReferencedPrimaryKey == null)
                    {
                        aDsDwColumnMapReferencedPrimaryKey = aDsDwTableMapPrimaryKey.DsDwColumnMapList.FirstOrDefault(
                            cm => cm.Include && cm.DsColumn != null && cm.DsColumn.InPrimaryKey);
                    }




                    if (aDsDwColumnMapReferencedSurogatePrimaryKey != null && aDsDwColumnMapReferencedPrimaryKey != null)
                    {
                        aResultString = aResultString +
                                        string.Format(
                                            "        (SELECT [{0}] FROM [{1}].[{2}].[{3}] WHERE [{1}].[{2}].[{3}].[{4}] = [ST].[{5}]) AS [{6}],",
                                            aDsDwColumnMapReferencedSurogatePrimaryKey.DwColumn.Name, //0
                                            DwDbName, //1
                                            aDsDwTableMapPrimaryKey.DwSchemaName, //2
                                            aDsDwTableMapPrimaryKey.DwTableName,//3
                                            aDsDwColumnMapReferencedPrimaryKey.DwColumn.Name, //4
                                            aDsDwColumnMapForeignKey.DsColumn.Name, //5
                                            aDsDwColumnMapForeignKey.DwColumn.Name //6
                                            ) + Environment.NewLine;
                    }
                }
            }
            aResultString = RemoveAtEnd(aResultString, "," + Environment.NewLine);

            return aResultString;
        }

        private string GenerateScd1BusinessKeyMatch(DsDwTableMap_M aDsDwTableMap)
        {
            string aResultString=String.Empty;
            DsDwColumnMap_M aDsDwColumnMapBusinessKey =
                        aDsDwTableMap.DsDwColumnMapList.FirstOrDefault(
                           cm => cm.Include && cm.Transformation == DsDwColumnTransformation.BusinesKey);
            if (aDsDwColumnMapBusinessKey != null)
            {
                aResultString = string.Format("    [source].[{0}] = [target].[{1}]",
                    aDsDwColumnMapBusinessKey.DsColumn.Name,
                    aDsDwColumnMapBusinessKey.DwColumn.Name);
            }
            return aResultString;
        }


        public string GenerateScd1(DsDwTableMap_M aDsDwTableMap)
        {
            string aResultString=String.Empty;
            if (aDsDwTableMap != null)
            {
                if ((aDsDwTableMap.DsDwColumnMapList.Count(cm => cm.Include && cm.Transformation == DsDwColumnTransformation.Scd1) > 0) &&
                    (aDsDwTableMap.DsDwColumnMapList.Count(cm => cm.Include && cm.Transformation == DsDwColumnTransformation.BusinesKey) == 1))
                {
                    aResultString = string.Format(@"

/******************************************************************************************************
*  Level {7} {2} -> {0}                   
*******************************************************************************************************/


MERGE {0} as [target]
USING
(
    SELECT
{1}
    FROM {2} AS [st]
) as [source]
ON
(
    {3}
)
WHEN MATCHED AND
(
{4}
)
THEN UPDATE
SET
{5}
WHEN NOT MATCHED BY TARGET
THEN INSERT
{6}",

                            GenerateScd1Target(aDsDwTableMap), //0
                            GenerateScd1SourceSelect(aDsDwTableMap), //1
                            GenerateScd1Source(aDsDwTableMap), //2
                            GenerateScd1BusinessKeyMatch(aDsDwTableMap), //3
                            GenerateScd1MatchedAnd(aDsDwTableMap), //4
                            GenerateScd1UpdateSet(aDsDwTableMap), //5
                            GenerateScd1Insert(aDsDwTableMap), //6
                            aDsDwTableMap.Level.ToString("D3") //7
                            );
                }
            }
            return aResultString;
        }
    }
}



