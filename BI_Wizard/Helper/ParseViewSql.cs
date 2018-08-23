using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using BI_Wizard.Model;

namespace BI_Wizard.Helper
{
    public class ColumnInfo : Base_VM
    {
        private string _Alias=String.Empty;
        private string _TableAlias=String.Empty;
        private string _TableColumnName=String.Empty;
        private string _TableServer=String.Empty;
        private string _TableDatabase=String.Empty;
        private string _TableSchema=String.Empty;
        private string _TableName=String.Empty;

        public string Alias
        {
            get { return _Alias; }
            set { SetProperty(ref _Alias, value); }
        }

        public string TableAlias
        {
            get { return _TableAlias; }
            set { SetProperty(ref _TableAlias, value); }
        }

        public string TableColumnName
        {
            get { return _TableColumnName; }
            set { SetProperty(ref _TableColumnName, value); }
        }

        public string TableServer
        {
            get { return _TableServer; }
            set { SetProperty(ref _TableServer, value); }
        }

        public string TableDatabase
        {
            get { return _TableDatabase; }
            set { SetProperty(ref _TableDatabase, value); }
        }

        public string TableSchema
        {
            get { return _TableSchema; }
            set { SetProperty(ref _TableSchema, value); }
        }

        public string TableName
        {
            get { return _TableName; }
            set { SetProperty(ref _TableName, value); }
        }
    }

    public class ParseViewSql : Base_VM
    {
        private ObservableCollection<ColumnInfo> _ColumnList = new ObservableCollection<ColumnInfo>();

        public ObservableCollection<ColumnInfo> ColumnList
        {
            get { return _ColumnList; }
            set { SetProperty(ref _ColumnList, value); }
        }

        public void FillEmptyAlias()
        {
            foreach (ColumnInfo aColumnInfo in ColumnList)
            {
                if (string.IsNullOrWhiteSpace(aColumnInfo.Alias))
                {
                    aColumnInfo.Alias = aColumnInfo.TableColumnName;
                }
            }
        }

        public void AddColumnIfNeeded(int aSelectElementID, string aIdentifier)
        {
            if (ColumnList.Count > aSelectElementID)
            {
                //Insert error
            }
            else
            {
                ColumnList.Add(new ColumnInfo { Alias = aIdentifier });
            }
        }

        public void AddRefereceIdentifier(int aSelectElementID, MultiPartIdentifier aMultiPartIdentifier)
        {
            if (ColumnList.Count > aSelectElementID)
            {
                ColumnInfo aColumnInfo = ColumnList[aSelectElementID];
                int aIdentIdx = 0;
                foreach (Identifier aIdentifier in aMultiPartIdentifier.Identifiers)
                {
                    if (aMultiPartIdentifier.Identifiers.Count == 2)
                    {
                        if (aIdentIdx == 0)
                            aColumnInfo.TableAlias = aIdentifier.Value;
                        if (aIdentIdx == 1)
                            aColumnInfo.TableColumnName = aIdentifier.Value;
                    }
                    if (aMultiPartIdentifier.Identifiers.Count == 3)
                    {
                        if (aIdentIdx == 0)
                            aColumnInfo.TableSchema = aIdentifier.Value;
                        if (aIdentIdx == 1)
                            aColumnInfo.TableName = aIdentifier.Value;
                        if (aIdentIdx == 2)
                            aColumnInfo.TableColumnName = aIdentifier.Value;
                    }
                    aIdentIdx = aIdentIdx + 1;
                }
            }
            else
            {
                AddLogText(string.Format("Error: column with idx:{0} not found.", aSelectElementID));
            }
        }

        public void AddTableReference(SchemaObjectName aSchemaObjectName, Identifier aAliasIdentifier)
        {
            if (ColumnList.Count > 0)
            {
                foreach (ColumnInfo aColumnInfo in ColumnList)
                {
                    if (aAliasIdentifier != null && aColumnInfo.TableAlias.ToLower() == aAliasIdentifier.Value.ToLower())
                    {
                        if (aSchemaObjectName.ServerIdentifier != null)
                            aColumnInfo.TableServer = aSchemaObjectName.ServerIdentifier.Value;
                        if (aSchemaObjectName.DatabaseIdentifier != null)
                            aColumnInfo.TableDatabase = aSchemaObjectName.DatabaseIdentifier.Value;
                        if (aSchemaObjectName.SchemaIdentifier != null)
                            aColumnInfo.TableSchema = aSchemaObjectName.SchemaIdentifier.Value;
                        if (aSchemaObjectName.BaseIdentifier != null)
                            aColumnInfo.TableName = aSchemaObjectName.BaseIdentifier.Value;
                    }
                    else if ((aAliasIdentifier == null) &&
                             (aSchemaObjectName.BaseIdentifier != null) &&
                             (aSchemaObjectName.BaseIdentifier.Value.ToLower() == aColumnInfo.TableAlias.ToLower()))
                    {
                        if (aSchemaObjectName.ServerIdentifier != null)
                            aColumnInfo.TableServer = aSchemaObjectName.ServerIdentifier.Value;
                        if (aSchemaObjectName.DatabaseIdentifier != null)
                            aColumnInfo.TableDatabase = aSchemaObjectName.DatabaseIdentifier.Value;
                        if (aSchemaObjectName.SchemaIdentifier != null)
                            aColumnInfo.TableSchema = aSchemaObjectName.SchemaIdentifier.Value;
                        if (aSchemaObjectName.BaseIdentifier != null)
                            aColumnInfo.TableName = aSchemaObjectName.BaseIdentifier.Value;
                    }
                }
            }
            else
            {
                AddLogText("Could not add table reference becouse there are no Columns!");
            }
        }

        private void ProcessViewStatementBody(TSqlStatement sqlStatement)
        {
            if ((sqlStatement.GetType() == typeof(CreateViewStatement)) ||
                (sqlStatement.GetType() == typeof(AlterViewStatement)))
            {
                ViewStatementBody aViewStatementBody = (ViewStatementBody) sqlStatement;
                AddLogText("Columns meta data:");
                foreach (Identifier aColumnIdentifier in aViewStatementBody.Columns)
                {
                    AddLogText(string.Format("Column:{0}", aColumnIdentifier.Value));
                    ColumnList.Add(new ColumnInfo { Alias = aColumnIdentifier.Value });
                }

                AddLogText("");
                AddLogText("QueryExpression SelectElements:");
                SelectStatement aSelectStatement = aViewStatementBody.SelectStatement;
                QueryExpression aQueryExpression = aSelectStatement.QueryExpression;
                if (aQueryExpression.GetType() == typeof(QuerySpecification))
                {
                    QuerySpecification aQuerySpecification = (QuerySpecification) aQueryExpression;
                    int aSelectElementID = 0;
                    foreach (SelectElement aSelectElement in aQuerySpecification.SelectElements)
                    {
                        if (aSelectElement.GetType() == typeof(SelectScalarExpression))
                        {
                            SelectScalarExpression aSelectScalarExpression = (SelectScalarExpression) aSelectElement;

                            string identStr = string.Empty;
                            IdentifierOrValueExpression aIdentifierOrValueExpression =
                                aSelectScalarExpression.ColumnName;
                            if (aIdentifierOrValueExpression != null)
                            {
                                if (aIdentifierOrValueExpression.ValueExpression == null)
                                {
                                    AddLogText(string.Format("Identifier={0}",
                                        aIdentifierOrValueExpression.Identifier.Value));
                                    identStr = aIdentifierOrValueExpression.Identifier.Value;
                                }
                                else
                                {
                                    AddLogText("Value Expression found!");
                                }
                            }
                            AddColumnIfNeeded(aSelectElementID, identStr);

                            ScalarExpression aScalarExpression = aSelectScalarExpression.Expression;
                            PrintSelectScalarExperssionRecurse(aSelectElementID, aScalarExpression);
                        }
                        else
                        {
                            AddColumnIfNeeded(aSelectElementID, "Error, something else than SelectScalarExpression found");
                            AddLogText("We only support SelectScalarExpression.");
                        }
                        aSelectElementID = aSelectElementID + 1;
                        AddLogText("");
                    }
                    AddLogText("");
                    AddLogText("Table References:");
                    FromClause aFromClause = aQuerySpecification.FromClause;
                    foreach (TableReference aTableReference in aFromClause.TableReferences)
                    {
                        PrintTableReferenceRecurse(aTableReference);
                    }
                }
                FillEmptyAlias();
            }
            else
            {
                AddLogText("This is not a view statement, but a:" + sqlStatement.ToString());
            }
        }

        private string MultiPartIdentifierToString(int aSelectElementID, MultiPartIdentifier aMultiPartIdentifier)
        {
            String res = String.Empty;
            foreach (Identifier aIdentifier in aMultiPartIdentifier.Identifiers)
            {
                if (String.IsNullOrEmpty(res))
                {
                    res = aIdentifier.Value;
                }
                else
                {
                    res = res + "." + aIdentifier.Value;
                }
            }
            AddRefereceIdentifier(aSelectElementID, aMultiPartIdentifier);
            return res;
        }

        private void PrintSelectScalarExperssionRecurse(int aSelectElementID, ScalarExpression aScalarExpression)
        {
            if (aScalarExpression.GetType() == typeof(ColumnReferenceExpression))
            {
                ColumnReferenceExpression aColumnReferenceExpression = (ColumnReferenceExpression) aScalarExpression;
                AddLogText(string.Format("ColumnType={0}", aColumnReferenceExpression.ColumnType.ToString()));
                MultiPartIdentifier aMultiPartIdentifier = aColumnReferenceExpression.MultiPartIdentifier;
                AddLogText(string.Format("Reference Identifier={0}",
                    MultiPartIdentifierToString(aSelectElementID, aMultiPartIdentifier)));
            }
            else if (aScalarExpression.GetType() == typeof(ConvertCall))
            {
                ConvertCall aConvertCall = (ConvertCall) aScalarExpression;
                ScalarExpression aScalarExpressionParameter = aConvertCall.Parameter;
                PrintSelectScalarExperssionRecurse(aSelectElementID, aScalarExpressionParameter);
            }
            else
            {
                AddLogText(String.Format("Not supported Expression:{0}", aScalarExpression.GetType().ToString()));
            }
        }

        private void PrintTableReferenceRecurse(TableReference aTableReference)
        {
            if (aTableReference.GetType() == typeof(NamedTableReference))
            {
                NamedTableReference aNamedTableReference = (NamedTableReference) aTableReference;
                Identifier aAliasIdentifier = aNamedTableReference.Alias;
                SchemaObjectName aSchemaObjectName = aNamedTableReference.SchemaObject;
                AddLogText(string.Format("Table Reference Server.Database.Schema.Base={0}.{1}.{2}.{3}",
                    (aSchemaObjectName.ServerIdentifier != null) ? aSchemaObjectName.ServerIdentifier.Value : "",
                    (aSchemaObjectName.DatabaseIdentifier != null) ? aSchemaObjectName.DatabaseIdentifier.Value : "",
                    (aSchemaObjectName.SchemaIdentifier != null) ? aSchemaObjectName.SchemaIdentifier.Value : "",
                    (aSchemaObjectName.BaseIdentifier != null) ? aSchemaObjectName.BaseIdentifier.Value : "")
                    );
                if (aAliasIdentifier != null)
                {
                    AddLogText(string.Format("Table Reference Alias:{0}", aAliasIdentifier.Value));
                }
                AddTableReference(aSchemaObjectName, aAliasIdentifier);
            }
            if (aTableReference.GetType() == typeof(QualifiedJoin))
            {
                QualifiedJoin aQualifiedJoin = (QualifiedJoin) aTableReference;
                AddLogText(string.Format("Table Reference QualifiedJoinType ={0}", aQualifiedJoin.QualifiedJoinType.ToString()));
                PrintTableReferenceRecurse(aQualifiedJoin.FirstTableReference);
                PrintTableReferenceRecurse(aQualifiedJoin.SecondTableReference);
            }
            if (aTableReference.GetType() == typeof(JoinTableReference))
            {
                JoinTableReference aJoinTableReference = (JoinTableReference) aTableReference;
                PrintTableReferenceRecurse(aJoinTableReference.FirstTableReference);
                PrintTableReferenceRecurse(aJoinTableReference.SecondTableReference);
            }
        }

        private void AddLogText(string aLine)
        {
            //ResultTextBox.Text = ResultTextBox.Text + aLine + Environment.NewLine;
        }

        public IList<ParseError> ParseViewSqlErrors;

        public bool ParseView(string aViewSql)
        {
            TSql100Parser SqlParser = new TSql100Parser(false);
            ColumnList.Clear();

            TSqlFragment result = SqlParser.Parse(new StringReader(aViewSql), out ParseViewSqlErrors);

            foreach (ParseError aParseError in ParseViewSqlErrors)
            {
                AddLogText(string.Format("Parse sql error:{0} at line nr:{1} column:{2} ",
                    aParseError.Message, aParseError.Line, aParseError.Column));
            }


            if (ParseViewSqlErrors.Count == 0)
            {
                TSqlScript SqlScript = result as TSqlScript;
                foreach (TSqlBatch sqlBatch in SqlScript.Batches)
                {
                    foreach (TSqlStatement sqlStatement in sqlBatch.Statements)
                    {
                        ProcessViewStatementBody(sqlStatement);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
