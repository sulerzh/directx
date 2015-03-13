using ADODB;
using Microsoft.Data.Recommendation.Client.PowerMap;
using Microsoft.Data.Visualization.Client.Excel;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationControls;
using Microsoft.Office.Interop.Excel;
using Microsoft.Reporting.QueryDesign.Edm.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace Microsoft.Data.Visualization.DataProvider
{
    public class ModelWrapper
    {
        private static readonly Regex ExcelInternalModelConnectionStringRegEx = new Regex(ExcelInternalModelConnectionStringMatchPattern);
        private HashSet<string> m_ConnectionsInUse = new HashSet<string>();
        public const string ExcelInternalDataSourceName = "$Embedded$";
        private const string ExcelInternalModelConnectionStringFormat = "Provider=MSOLAP;Data Source={0};Location={1};SQLQueryMode=DataKeys";
        private const string ExcelInternalModelConnectionStringMatchPattern = "^Provider=MSOLAP;Data Source=(?<DataSource>[^;]+);Location=(?<Location>[^;]+);SQLQueryMode=DataKeys$";
        private const string ConnectionString = "WORKSHEET;";
        private const string NamePrefixString = "WorksheetConnection_";
        private Dictionary<int, TableMemberDataType> dataTypeMap;

        private ExcelProxy<Workbook> ExcelWorkbook { get; set; }

        private ExcelProxy<Application> ExcelApplication { get; set; }

        private AnalysisModelAdapter OrlandoAdapter { get; set; }

        public bool InternalModelNotEmpty
        {
            get
            {
                ExcelProxy<Model> modelProxy = this.ExcelWorkbook.InvokeAndProxyOptional(workbook => workbook.Model);
                if (modelProxy == null)
                    return false;
                ExcelProxy<ModelTables> modelTablesProxy = modelProxy.InvokeAndProxyOptional(modelArg => modelArg.ModelTables);
                return modelTablesProxy != null && modelTablesProxy.Invoke(modelTablesArg => (long)modelTablesArg.Count) != 0L;
            }
        }

        public ModelWrapper(Workbook workbook, Connection connection)
        {
            if (workbook == null)
                throw new ArgumentNullException("workbook");
            this.ExcelWorkbook = new ExcelProxy<Workbook>(workbook);
            this.ExcelApplication = new ExcelProxy<Application>(workbook.Application);
            this.dataTypeMap = new Dictionary<int, TableMemberDataType>();
            this.dataTypeMap[3] = TableMemberDataType.Long;
            this.dataTypeMap[5] = TableMemberDataType.Double;
            this.dataTypeMap[6] = TableMemberDataType.Currency;
            this.dataTypeMap[7] = TableMemberDataType.DateTime;
            this.dataTypeMap[11] = TableMemberDataType.Bool;
            this.dataTypeMap[20] = TableMemberDataType.Long;
            this.dataTypeMap[130] = TableMemberDataType.String;
            Stopwatch stopwatch = Stopwatch.StartNew();
            AnalysisModelAdapter.Initialize();
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Orlando AnalysisModelAdapter initialization time: {0} ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
            this.OrlandoAdapter = new AnalysisModelAdapter(connection);
            stopwatch.Stop();
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Orlando AnalysisModelAdapter ctor time: {0} ms", stopwatch.ElapsedMilliseconds);
        }

        public ModelMetadata GetModelTables()
        {
            ModelMetadata modelMetadata = new ModelMetadata();
            this.GetModelTablesFromCSDL(modelMetadata);
            Stopwatch stopwatch = Stopwatch.StartNew();
            int workerThreads;
            int completionPortThreads;
            ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "GetModelTables(): ThreadPool min worker threads={0}, min ioCompletion threads = {1}", workerThreads, completionPortThreads);
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "GetModelTables(): ThreadPool max worker threads={0}, max ioCompletion threads = {1}", workerThreads, completionPortThreads);
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "GetModelTables(): ThreadPool available worker threads={0}, available ioCompletion threads = {1}", workerThreads, completionPortThreads);
            if (AnalysisModelAdapter.IsInitialized)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, string.Format("Orlando AnalysisModelAdapter.IsInitialized time {0} ms", stopwatch.ElapsedMilliseconds));
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Calling StartClassification()");
                if (this.OrlandoAdapter.StartClassification(modelMetadata))
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, string.Format("StartClassification() time {0} ms", stopwatch.ElapsedMilliseconds));
                    if (this.OrlandoAdapter.WaitForClassifications(TimeSpan.FromSeconds(10.0)))
                    {
                        stopwatch.Stop();
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, string.Format("Total Orlando classification time (incl column sampling time): {0} ms", stopwatch.ElapsedMilliseconds));
                    }
                    else
                    {
                        stopwatch.Stop();
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Classifications failed; Total Orlando classification time (incl column sampling time): {0} ms", stopwatch.ElapsedMilliseconds);
                    }
                }
                else
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "StartClassification() returned false - no candidate table columns exist for classification");
                this.OrlandoAdapter.Reset();
            }
            else
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, string.Format("Orlando AnalysisModelAdapter.IsInitialized returned false after waiting for time {0} ms", stopwatch.ElapsedMilliseconds));
            return modelMetadata;
        }

        private void GetModelTablesFromCSDL(ModelMetadata modelMetadata)
        {
            object[,] objArray;

            ADODB.Command cmd = new ADODB.CommandClass();
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Fetching CSDL");
                cmd.ActiveConnection = this.ExcelWorkbook.Invoke(wkbk => wkbk.Model.DataModelConnection.ModelConnection.ADOConnection as Connection);
                cmd.CommandText = "SELECT * FROM  SYSTEMRESTRICTSCHEMA($system.DISCOVER_CSDL_METADATA, [CATALOG_NAME] = 'Microsoft_SQLServer_AnalysisServices', [VERSION] = '1.1')";
                object obj1;
                Recordset recordset = cmd.Execute(out obj1, Missing.Value, -1);
                if (recordset.EOF)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "No CSDL, returning");
                    return;
                }
                else
                    objArray = recordset.GetRows(-1, Missing.Value, Missing.Value) as object[,];
            }
            catch (COMException ex)
            {
                StringBuilder stringBuilder = new StringBuilder();
                int num = 1;
                foreach (ADODB.Error error in cmd.ActiveConnection.Errors)
                    stringBuilder.AppendFormat("{0}: Error source={1}, Number={2:x}, Native error={3:x}, Description={4}; ",
                        num++, error.Source, error.Number, error.NativeError, error.Description);
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Error fetching CSDL: {0}, exception: {1}",
                    stringBuilder.ToString(), ex);
                throw;
            }
            finally
            {
                try
                {
                    cmd.ActiveConnection.Close();
                    cmd.ActiveConnection = null;
                }
                catch (Exception ex)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "CSDL fetch: error closing the model connection after fetching CSDL, ex={0}", ex);
                }
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "CSDL fetch time: {0} ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Parsing CSDL");
            EntityDataModel entityDataModel = EntityDataModel.Load(XmlReader.Create(new StringReader((objArray[0, 0] as string).Replace("xmlns:bi=\"http://schemas.microsoft.com/ado/2008/09/edm\"", string.Empty))));
            Dictionary<string, TableMetadata> dictionary = new Dictionary<string, TableMetadata>();
            Dictionary<string, int> islandNumber = new Dictionary<string, int>();
            int num1 = 0;
            foreach (EntitySet entitySet in entityDataModel.EntitySets)
            {
                DateTime lastRefresh = DateTime.MinValue;
                TableMetadata table = new TableMetadata(entitySet.Caption, entitySet.Name, lastRefresh, !entitySet.Hidden);
                foreach (EdmMember edmMember in entitySet.ElementType.Members)
                {
                    EdmField edmField = edmMember as EdmField;
                    EdmProperty edmProperty;
                    if (edmField == null)
                    {
                        EdmMeasure edmMeasure = edmMember as EdmMeasure;
                        if (edmMeasure != null && edmMeasure.Kpi == null)
                            edmProperty = edmMeasure;
                        else
                            continue;
                    }
                    else
                        edmProperty = edmField;
                    string caption = edmProperty.Caption;
                    TableMemberDataType dataType = TableMemberDataType.Unknown;
                    if (!edmProperty.HasVariantDataType && edmProperty.TypeUsage.EdmType.GetPrimitiveTypeKind().HasValue)
                    {
                        switch (edmProperty.TypeUsage.EdmType.GetPrimitiveTypeKind().Value)
                        {
                            case PrimitiveTypeKind.Boolean:
                                dataType = TableMemberDataType.Bool;
                                break;
                            case PrimitiveTypeKind.DateTime:
                            case PrimitiveTypeKind.Time:
                            case PrimitiveTypeKind.DateTimeOffset:
                                dataType = TableMemberDataType.DateTime;
                                break;
                            case PrimitiveTypeKind.Decimal:
                            case PrimitiveTypeKind.Double:
                            case PrimitiveTypeKind.Single:
                                dataType = TableMemberDataType.Double;
                                break;
                            case PrimitiveTypeKind.SByte:
                            case PrimitiveTypeKind.Int16:
                            case PrimitiveTypeKind.Int32:
                            case PrimitiveTypeKind.Int64:
                                dataType = TableMemberDataType.Long;
                                break;
                            case PrimitiveTypeKind.String:
                                dataType = TableMemberDataType.String;
                                break;
                        }
                    }
                    if (dataType == TableMemberDataType.Unknown)
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Unrecognized  datatype for table={0}, {5}={1}, HasVariantDataType={2}, GetPrimitiveTypeKind.HasValue={3}, GetPrimitiveTypeKind={4}",
                            table.ModelName, caption, edmProperty.HasVariantDataType,
                            edmProperty.TypeUsage.EdmType.GetPrimitiveTypeKind().HasValue, edmProperty.TypeUsage.EdmType.GetPrimitiveTypeKind(), edmField == null ? "measure" : "column");
                    if (edmField == null)
                    {
                        TableMeasure measure = new TableMeasure(table, caption, TableColumnExtensions.TableMeasureDAXQueryName(table, caption), dataType, !edmProperty.Hidden);
                        table.AddMeasure(measure);
                    }
                    else
                    {
                        TableColumn tableColumn = new TableColumn(table, caption, TableColumnExtensions.TableColumnDAXQueryName(table, caption), dataType, !edmProperty.Hidden);
                        table.AddField(tableColumn);
                    }
                }
                dictionary.Add(entitySet.FullName, table);
                islandNumber[entitySet.FullName] = num1++;
            }
            foreach (AssociationSet associationSet in entityDataModel.AssociationSets)
            {
                if (associationSet.AssociationSetEnds.Count != 2)
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Unexpected: associationSet.AssociationSetEnds.Count = {0} (expected 2); name of first association end is: {1}", associationSet.AssociationSetEnds.Count, associationSet.AssociationSetEnds.Count == 0 ? "unknown (count=0)" : associationSet.AssociationSetEnds[0].Name);
                else if (associationSet.State != AssociationState.Active)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Skipping over inactive associationSet; name of first association end is: {1}", associationSet.AssociationSetEnds.Count, associationSet.AssociationSetEnds.Count == 0 ? "unknown (count=0)" : associationSet.AssociationSetEnds[0].Name);
                }
                else
                {
                    AssociationSetEnd associationSetEnd1;
                    AssociationSetEnd associationSetEnd2;
                    if (associationSet.AssociationSetEnds[0].CorrespondingAssociationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                    {
                        associationSetEnd1 = associationSet.AssociationSetEnds[1];
                        associationSetEnd2 = associationSet.AssociationSetEnds[0];
                    }
                    else
                    {
                        associationSetEnd1 = associationSet.AssociationSetEnds[0];
                        associationSetEnd2 = associationSet.AssociationSetEnds[1];
                    }
                    string modelName1 = associationSetEnd1.EntitySet.FullName;
                    string modelName2 = associationSetEnd2.EntitySet.FullName;
                    if (dictionary[modelName1] == null)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Unexpected: PK table name {0} in a model relationship was not found earlier, adding hidden table", modelName1);
                        dictionary[modelName1] = new TableMetadata(modelName1, null, new DateTime(), false);
                    }
                    if (dictionary[modelName2] == null)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Unexpected: FK table name {0} in a model relationship was not found earlier, adding hidden table", modelName2);
                        dictionary[modelName2] = new TableMetadata(modelName2, null, new DateTime(), false);
                    }
                    dictionary[modelName1].AddForeignKeyTable(dictionary[modelName2]);
                    dictionary[modelName2].AddPrimaryKeyTable(dictionary[modelName1]);
                    string name1 = associationSetEnd1.CorrespondingAssociationEndMember.Name;
                    string name2 = associationSetEnd2.CorrespondingAssociationEndMember.Name;
                    TableField field1 = dictionary[modelName1].GetField(name1);
                    TableField field2 = dictionary[modelName2].GetField(name2);
                    if (field1 == null)
                    {
                        TableMetadata table = dictionary[modelName1];
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Unexpected: PK column {0} in table name {1} in a model relationship was not found earlier, adding hidden table", name1, modelName1);
                        field1 = new TableColumn(table, name1, TableColumnExtensions.TableColumnDAXQueryName(table, name1), TableMemberDataType.Unknown, false);
                        table.AddField(field1);
                    }
                    if (field2 == null)
                    {
                        TableMetadata table = dictionary[modelName2];
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Unexpected: FK column {0} in table name {1} in a model relationship was not found earlier, adding hidden table", name2, modelName2);
                        field2 = new TableColumn(table, name2, TableColumnExtensions.TableColumnDAXQueryName(table, name2), TableMemberDataType.Unknown, false);
                        table.AddField(field2);
                    }
                    field1.AddForeignKeyField(field2);
                    field2.AddPrimaryKeyField(field1);
                    if (islandNumber[modelName1] != islandNumber[modelName2])
                    {
                        int foriegnKeyIslandNumber = islandNumber[modelName2];
                        foreach (string index in islandNumber.Keys.Where(modelTableName => islandNumber[modelTableName] == foriegnKeyIslandNumber).ToList())
                            islandNumber[index] = islandNumber[modelName1];
                    }
                }
            }
            foreach (int num2 in islandNumber.Values.Distinct())
            {
                int n = num2;
                IEnumerable<string> enumerable = islandNumber.Keys.Where(modelTableName => islandNumber[modelTableName] == n);
                TableIsland island = new TableIsland();
                foreach (string index in enumerable)
                    island.AddTable(dictionary[index]);
                island.UpdateLookupTables();
                island.RankTables();
                island.Sort();
                modelMetadata.AddTableIsland(island);
            }
            string culture = entityDataModel.Culture;
            try
            {
                modelMetadata.Culture = new CultureInfo(culture ?? string.Empty);
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "CSDL model culture: {0}", (culture ?? "is null, using InvariantCulture"));
            }
            catch (CultureNotFoundException ex)
            {
                modelMetadata.Culture = new CultureInfo(string.Empty);
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "CSDL model culture {0} not found, using InvariantCulture", culture);
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "CSDL parse + model construction time: {0} ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();
        }

        public Dictionary<string, string> GetExcelTablesInModel(bool removeWorkbookNameFromExcelTableNames = true)
        {
            string str = this.ExcelWorkbook.Invoke(wb => wb.Name);
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (ExcelProxy<ModelTable> modelTable in this.ExcelWorkbook.InvokeAndProxy(wb => wb.Model).Enumerate<ModelTable>(m => (IEnumerable)m.ModelTables))
            {
                string key = modelTable.Invoke(mt => mt.Name);
                ExcelProxy<WorkbookConnection> workbookConnectionProxy = modelTable.InvokeAndProxyOptional(mt => mt.SourceWorkbookConnection);
                if (workbookConnectionProxy == null)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Workbook {0} Model Table {1} has no WorkbookConnection - skipping", str, key);
                }
                else
                {
                    string connectionName = null;
                    XlConnectionType connectionType = XlConnectionType.xlConnectionTypeWORKSHEET;
                    workbookConnectionProxy.Invoke(conn =>
                    {
                        connectionName = conn.Name;
                        connectionType = conn.Type;
                    });
                    if (connectionType != XlConnectionType.xlConnectionTypeWORKSHEET)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Workbook {0} ModelTable {1} Connection {2} is of type {3} - skipping", str, key, connectionName, connectionType);
                    }
                    else
                    {
                        ExcelProxy<WorksheetDataConnection> worksheetConnectionProxy = workbookConnectionProxy.InvokeAndProxyOptional(conn => conn.WorksheetDataConnection);
                        if (worksheetConnectionProxy == null)
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "Workbook {0} ModelTable {1} The connection {2} is of type Worksheet but WorksheetDataConnection property is null - skipping", str, key, connectionName);
                        }
                        else
                        {
                            string commandText = null;
                            XlCmdType commandType = XlCmdType.xlCmdExcel;
                            worksheetConnectionProxy.Invoke(conn =>
                            {
                                commandText = conn.CommandText;
                                commandType = conn.CommandType;
                            });
                            if (commandType != XlCmdType.xlCmdExcel)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Worknook {0} ModelTable {1} Worksheet Connection {2} is of type {3} - skipping", str, key, connectionName, commandType);
                            }
                            else
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Workbook {0} ModelTable {1} is connected to Excel Table {2}", str, key, commandText);
                                if (removeWorkbookNameFromExcelTableNames)
                                    commandText = commandText.Split('!').Last();
                                result.Add(key, commandText);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public bool AddToModel(IModelData modelData, out string modelTableName)
        {
            ExcelProxy<WorkbookConnection> workbookConnection;
            bool flag = this.AddToModel(modelData.GetAddToModelObject(), modelData.GetCommandText(), out workbookConnection, out modelTableName);
            modelData.DataWorkbookConnection = workbookConnection;
            return flag;
        }

        public bool AddToModel(string tableName, out string modelTableName)
        {
            ExcelProxy<WorkbookConnection> workbookConnection;
            return this.AddToModel(tableName, tableName, out workbookConnection, out modelTableName);
        }

        public bool AddToModel(object addToModelObject, string commandText, out ExcelProxy<WorkbookConnection> workbookConnection, out string modelTableName)
        {
            ExcelProxy<WorkbookConnection> connectionByCommandText = this.FindConnectionByCommandText(commandText);
            if (connectionByCommandText != null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Add to model skipped, connection command text already exists : {0}", commandText);
                workbookConnection = connectionByCommandText;
                modelTableName = null;
                return false;
            }
            string connectionName = NamePrefixString + commandText;
            int retryTimes = 0;
            bool flag = false;
            workbookConnection = null;
            do
            {
                try
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Adding to model: {0}, commandText: {1}", connectionName, commandText);
                    workbookConnection = this.ExcelWorkbook.InvokeAndProxy(workbook => workbook.Connections.Add2(connectionName, string.Empty, ConnectionString, commandText, XlCmdType.xlCmdExcel, true, false));
                    flag = true;
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == -2147417846 || ex.ErrorCode == -2146827284 || ex.ErrorCode == -2147217842)
                    {
                        if (++retryTimes >= 5)
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "AddToModel(): {0} retries calling Add2 failed: giving up", 5);
                            throw;
                        }
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "AddToModel(): {0} of {1} retries calling Add2 failed with hr=0x{2:x}, will retry", retryTimes, 5, ex.ErrorCode);
                    }
                    else
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "AddToModel(): {0} retries calling Add2 failed: Exception: {1}", (retryTimes + 1), ex);
                        throw;
                    }
                }
                if (flag)
                    break;
            }
            while (retryTimes < 5);
            modelTableName = this.GetExcelTableModelName(connectionName);
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Add to model successful : {0}, commandText: {1}, returning model table name = {2}", connectionName, commandText, modelTableName);
            return true;
        }

        public IModelData GetModelDataFromSelection()
        {
            this.VerifyOneAreaSelected();
            ExcelProxy<ListObject> selectedTable = this.GetSelectedTable();
            if (selectedTable != null)
                return new TableModelData(selectedTable);
            ExcelProxy<Microsoft.Office.Interop.Excel.Range> selection = this.GetSelection();
            if (selection == null)
                return null;
            return new RangeModelData(selection);
        }

        public bool ExpandAndValidateSelection()
        {
            if (!this.IsSelectionARange())
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Invalid model selection: Selection is not a valid range");
                return false;
            }
            if (this.IsEntireSheetSelected())
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Invalid model selection: The entire sheet is selected");
                return false;
            }
            if (this.IsSheetProtected())
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Invalid model selection: The sheet is protected");
                return false;
            }
            if (this.IsSelectionAPivotTable())
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Invalid model selection: Selection is a Pivot Table");
                return false;
            }
            // this.ExpandSelection();
            if (!this.IsSelectionEmpty())
                return true;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Invalid model selection: Selection is empty");
            return false;
        }

        public void SetAdditionalTablesInUse(HashSet<string> tablesInUse)
        {
            foreach (KeyValuePair<string, ExcelProxy<WorkbookConnection>> keyValuePair in this.ExtractConnectionsFromTables(tablesInUse))
                this.m_ConnectionsInUse.Add(keyValuePair.Key);
        }

        public void RemoveUnusedTables()
        {
            this.RemoveUnusedTables(new HashSet<string>());
        }

        public bool RemoveUnusedTables(HashSet<string> tablesStillInUse)
        {
            try
            {
                Dictionary<string, ExcelProxy<WorkbookConnection>> connectionsFromTables = this.ExtractConnectionsFromTables(tablesStillInUse);
                HashSet<string> hashSet = new HashSet<string>();
                foreach (string key in this.m_ConnectionsInUse)
                {
                    if (!connectionsFromTables.ContainsKey(key))
                        hashSet.Add(key);
                }
                foreach (string str in hashSet)
                    this.m_ConnectionsInUse.Remove(str);
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "Failed to mark connections as used with exception {0} - silently ignoring", ex.Message);
                return false;
            }
            return true;
        }

        public Dictionary<string, ExcelProxy<WorkbookConnection>> ExtractConnectionsFromTables(HashSet<string> tables)
        {
            Dictionary<string, ExcelProxy<WorkbookConnection>> connections = new Dictionary<string, ExcelProxy<WorkbookConnection>>();
            this.ExcelWorkbook.Invoke(workbook =>
            {
                foreach (ModelTable modelTable in workbook.Model.ModelTables)
                {
                    if (tables == null || tables.Contains(modelTable.Name))
                    {

                        WorkbookConnection workbookConnection = modelTable.SourceWorkbookConnection;
                        if (!connections.ContainsKey(workbookConnection.Name))
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Adding connection: {0} to hash", workbookConnection.Name);
                            connections.Add(workbookConnection.Name, new ExcelProxy<WorkbookConnection>(workbookConnection));
                        }
                    }
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Adding {0} connections", connections.Count);
            });
            return connections;
        }

        public static bool IsExcelInternalConnection(string connectionString)
        {
            Match match = ModelWrapper.ExcelInternalModelConnectionStringRegEx.Match(connectionString);
            if (!match.Success)
                return false;
            return match.Groups["DataSource"].Value.Equals("$Embedded$");
        }

        public static string GetInternalModelConnectionString(ExcelProxy<Workbook> workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException("workbook");
            return string.Format(CultureInfo.InvariantCulture, 
                ExcelInternalModelConnectionStringFormat,
                "$Embedded$", workbook.Invoke(workbookArg => workbookArg.FullName));
        }

        public string GetSelectionConnectionName()
        {
            return this.GetConnectionName(this.GetFullSelectionAddress());
        }

        public void InitializeExcelInternalConnection()
        {

            this.ExcelWorkbook.Invoke(workbookArg => workbookArg.Model.Initialize());
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "The internal model was successfully initialized");
        }

        public bool PromptUserToAddRelationship()
        {
            return this.ExcelWorkbook.Invoke(workbookArg => workbookArg.Application.Dialogs[XlBuiltInDialog.xlDialogCreateRelationship].Show(Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing));
        }

        private string GetExcelTableModelName(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "GetExcelTableModelName(): connectionName is null or empty - returning null");
                return null;
            }
            string str1 = this.ExcelWorkbook.Invoke(wb => wb.Name);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (ExcelProxy<ModelTable> modelTable in this.ExcelWorkbook.InvokeAndProxy(wb => wb.Model).Enumerate<ModelTable>(m => (IEnumerable)m.ModelTables))
            {
                string str2 = modelTable.Invoke(mt => mt.Name);
                ExcelProxy<WorkbookConnection> workbookConnection = modelTable.InvokeAndProxyOptional(mt => mt.SourceWorkbookConnection);
                if (workbookConnection == null)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "GetExcelTableModelName(): Workbook {0} Model Table {1} has no WorkbookConnection - skipping", str1, str2);
                }
                else
                {
                    string connectionNameForModel = null;

                    XlConnectionType connectionType = XlConnectionType.xlConnectionTypeWORKSHEET;
                    workbookConnection.Invoke(conn =>
                    {
                        connectionNameForModel = conn.Name;
                        connectionType = conn.Type;
                    });
                    if (connectionType != XlConnectionType.xlConnectionTypeWORKSHEET)
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "GetExcelTableModelName(): Workbook {0} ModelTable {1} Connection {2} is of type {3} - skipping", str1, str2, connectionNameForModel, connectionType);
                    else if (string.Compare(connectionName, connectionNameForModel, StringComparison.Ordinal) == 0)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "GetExcelTableModelName(): Workbook {0} Found connectionName={1}; model name is {2}", str1, connectionName, str2);
                        return str2;
                    }
                }
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "GetExcelTableModelName(): Workbook {0} Did not find connectionName={1} - returning null", str1, connectionName);
            return null;
        }

        private ExcelProxy<ListObject> GetSelectedTable()
        {
            string tableName = null;
            IEnumerable<int> tableColNumbers = null;
            IEnumerable<int> tableRowNumbers = null;
            ExcelProxy<ListObject> excelProxy = this.ExcelWorkbook.InvokeAndProxyOptional(workbook => workbook.Application.ActiveCell.ListObject);
            if (excelProxy != null)
                excelProxy.Invoke(tableArg =>
                {
                    tableName = tableArg.DisplayName;
                    tableColNumbers = tableArg.Range.Columns.Cast<Range>().Select(col => col.Column);
                    tableRowNumbers = tableArg.Range.Rows.Cast<Range>().Select(row => row.Row);
                });
            IEnumerable<int> colNumbers = null;
            IEnumerable<int> rowNumbers = null;
            this.ExcelWorkbook.Invoke(workbook =>
            {
                Microsoft.Office.Interop.Excel.Range range1 = workbook.Application.Selection;
                Microsoft.Office.Interop.Excel.Range range2 = range1.Areas.get_Item(range1.Areas.Count);
                colNumbers = range2.Columns.Cast<Range>().Select(col => col.Column);
                rowNumbers = range2.Rows.Cast<Range>().Select(row => row.Row);
            });
            if (tableName != null && 
                tableColNumbers.First() <= colNumbers.First() && 
                (tableRowNumbers.First() <= rowNumbers.First() && tableColNumbers.Last() >= colNumbers.Last()) && 
                tableRowNumbers.Last() >= rowNumbers.Last())
                return excelProxy;
            return null;
        }

        private void VerifyOneAreaSelected()
        {
            if (!this.ExcelWorkbook.InvokeAndProxyOptional((Func<Workbook, Microsoft.Office.Interop.Excel.Range>)(workbook =>
            {
                return workbook.Application.Selection;
            })).Invoke<bool>(range => range.Areas.Count > 1))
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Selection was found invalid - more than 1 areas selected");
            throw new ModelWrapper.InvalidModelData(Microsoft.Data.Visualization.Client.Excel.Resources.AddToModelInvalidDataSource);
        }

        private string GetSelectionAddress(ExcelProxy<Microsoft.Office.Interop.Excel.Range> selection)
        {
            return selection.Invoke(range =>
            {


                Microsoft.Office.Interop.Excel.Range range1 = range.Areas.get_Item(range.Areas.Count);

                return range1.get_Address(true, true, XlReferenceStyle.xlA1, false, Type.Missing);
            });
        }

        private bool IsSelectionARange()
        {
            return this.ExcelWorkbook.Invoke(workbook => workbook.Application.Selection is Microsoft.Office.Interop.Excel.Range);
        }

        private bool IsSelectionEmpty()
        {
            ExcelProxy<Microsoft.Office.Interop.Excel.Range> excelProxy = this.ExcelWorkbook.InvokeAndProxy(workbook =>
            {
                return (Microsoft.Office.Interop.Excel.Range)workbook.Application.Selection;
            });
            if (!excelProxy.Invoke(selectionArg => selectionArg.Areas.Count > 1))
            {

                return excelProxy.Invoke(range => range.Application.WorksheetFunction.CountBlank(range) == (double)range.Count);
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Selection has more than 1 area selected. Treating it as non-empty.");
            return false;
        }

        private bool IsEntireSheetSelected()
        {
            ExcelProxy<Microsoft.Office.Interop.Excel.Range> selection = this.ExcelWorkbook.InvokeAndProxy(workbook =>
            {
                return (Microsoft.Office.Interop.Excel.Range)workbook.Application.Selection;
            });
            string selectionAddress = this.GetSelectionAddress(selection.InvokeAndProxy(range =>
            {
                Worksheet worksheet = range.Worksheet;
                Microsoft.Office.Interop.Excel.Range range1 = worksheet.Cells[1, 1] as Microsoft.Office.Interop.Excel.Range;
                int count1 = worksheet.Rows.Count;
                int count2 = worksheet.Columns.Count;
                Microsoft.Office.Interop.Excel.Range range2 = worksheet.Cells[count1, count2] as Microsoft.Office.Interop.Excel.Range;
                Microsoft.Office.Interop.Excel.Range range3 = worksheet.get_Range(range1, range2);
                return range3;
            }));
            return string.Equals(this.GetSelectionAddress(selection), selectionAddress, StringComparison.Ordinal);
        }

        private bool IsSheetProtected()
        {
            return this.ExcelWorkbook.Invoke((Func<Workbook, bool>)(workbook =>
            {
                return workbook.ActiveSheet.ProtectContents;
            }));
        }

        private bool IsSelectionAPivotTable()
        {
            ExcelProxy<Microsoft.Office.Interop.Excel.Range> excelProxy = this.ExcelWorkbook.InvokeAndProxy(workbook =>
            {
                return (Microsoft.Office.Interop.Excel.Range)workbook.Application.Selection;
            });
            try
            {
                excelProxy.Invoke(range => range.PivotTable);
                return true;
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode != -2146827284)
                    throw ex;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Expected Exception thrown in IsSelectionAPivotTable - range is not a Pivot table");
                return false;
            }
        }

        private bool IsHeaderRowValid(ExcelProxy<Microsoft.Office.Interop.Excel.Range> selection)
        {
            return selection.InvokeAndProxy((Func<Microsoft.Office.Interop.Excel.Range, Microsoft.Office.Interop.Excel.Range>)(range =>
            {
                return range.Rows.Rows.get_Item(1, Type.Missing).Cells;
            })).Invoke<bool>(range => range.Application.WorksheetFunction.CountBlank(range) == 0.0);
        }

        private void ExpandSelection()
        {
            ExcelProxy<Microsoft.Office.Interop.Excel.Range> excelProxy1 = this.ExcelWorkbook.InvokeAndProxy((Func<Workbook, Microsoft.Office.Interop.Excel.Range>)(workbook =>
            {
                return workbook.Application.Selection;
            }));
            if (!this.IsSelectionARange() || excelProxy1.Invoke(range => range.Areas.Count > 1))
                return;
            ExcelProxy<ListObject> excelProxy2 = this.ExcelWorkbook.InvokeAndProxyOptional(workbook => workbook.Application.ActiveCell.ListObject);
            if (excelProxy2 != null)
            {
                excelProxy2.Invoke((Func<ListObject, object>)(tableArg => tableArg.Range.Select()));
            }
            else
            {
                if (excelProxy1.Invoke(range => range.Cells.Count != 1))
                    return;
                ExcelProxy<Microsoft.Office.Interop.Excel.Range> excelProxy3;
                try
                {
                    excelProxy3 = excelProxy1.InvokeAndProxy(range => range.CurrentRegion);
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == -2146827284)
                        throw new ModelWrapper.InvalidModelData(Microsoft.Data.Visualization.Client.Excel.Resources.RetryActionAfterEditDone, ex);
                    throw;
                }
                if (excelProxy3.Invoke(range => range.Cells.Count != 1))
                    excelProxy3 = this.RemoveEmptyExtremities(excelProxy3);
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "selection was expanded to address: {0}", this.GetSelectionAddress(excelProxy3));

                excelProxy3.Invoke((Func<Microsoft.Office.Interop.Excel.Range, object>)(range => range.Select()));
            }
        }

        private ExcelProxy<Microsoft.Office.Interop.Excel.Range> RemoveEmptyExtremities(ExcelProxy<Microsoft.Office.Interop.Excel.Range> range)
        {
            return range.InvokeAndProxy(selection =>
            {

                _Worksheet worksheet = selection.Worksheet;
                int row = selection.Row;
                int column = selection.Column;
                int num1 = selection.Row + selection.Rows.Count - 1;
                int num2 = selection.Column + selection.Columns.Count - 1;

                Microsoft.Office.Interop.Excel.Range range1 = worksheet.Cells[row, column] as Microsoft.Office.Interop.Excel.Range;

                Microsoft.Office.Interop.Excel.Range range2 = worksheet.Cells[row, num2] as Microsoft.Office.Interop.Excel.Range;


                Microsoft.Office.Interop.Excel.Range range3 = worksheet.get_Range(range1, range2);

                if (range3.Application.WorksheetFunction.CountBlank(range3) == range3.Count)
                    ++row;

                Microsoft.Office.Interop.Excel.Range range4 = worksheet.Cells[num1, column] as Microsoft.Office.Interop.Excel.Range;

                Microsoft.Office.Interop.Excel.Range range5 = worksheet.Cells[num1, num2] as Microsoft.Office.Interop.Excel.Range;


                Microsoft.Office.Interop.Excel.Range range6 = worksheet.get_Range(range4, range5);

                if (range6.Application.WorksheetFunction.CountBlank(range6) == range6.Count)
                    --num1;

                Microsoft.Office.Interop.Excel.Range range7 = worksheet.Cells[row, column] as Microsoft.Office.Interop.Excel.Range;

                Microsoft.Office.Interop.Excel.Range range8 = worksheet.Cells[num1, column] as Microsoft.Office.Interop.Excel.Range;


                Microsoft.Office.Interop.Excel.Range range9 = worksheet.get_Range(range7, range8);

                if (range9.Application.WorksheetFunction.CountBlank(range9) == range9.Count)
                    ++column;

                Microsoft.Office.Interop.Excel.Range range10 = worksheet.Cells[row, num2] as Microsoft.Office.Interop.Excel.Range;

                Microsoft.Office.Interop.Excel.Range range11 = worksheet.Cells[num1, num2] as Microsoft.Office.Interop.Excel.Range;


                Microsoft.Office.Interop.Excel.Range range12 = worksheet.get_Range(range10, range11);

                if (range12.Application.WorksheetFunction.CountBlank(range12) == range12.Count)
                    --num2;

                Microsoft.Office.Interop.Excel.Range range13 = worksheet.Cells[row, column] as Microsoft.Office.Interop.Excel.Range;

                Microsoft.Office.Interop.Excel.Range range14 = worksheet.Cells[num1, num2] as Microsoft.Office.Interop.Excel.Range;

                return worksheet.get_Range(range13, range14);
            });
        }

        private ExcelProxy<Microsoft.Office.Interop.Excel.Range> GetSelection()
        {
            ExcelProxy<Microsoft.Office.Interop.Excel.Range> selection = this.ExcelWorkbook.InvokeAndProxyOptional((Func<Workbook, Microsoft.Office.Interop.Excel.Range>)(workbook =>
            {
                return workbook.Application.Selection;
            }));
            if (selection == null)
                return null;
            string selectionAddress1 = this.GetSelectionAddress(selection.InvokeAndProxy(range => range.EntireRow));
            if (string.Equals(this.GetSelectionAddress(selection), selectionAddress1, StringComparison.Ordinal))
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Selection was found invalid - entire row(s) selected");
                throw new ModelWrapper.InvalidModelData(Microsoft.Data.Visualization.Client.Excel.Resources.AddToModelInvalidDataSource);
            }
            string selectionAddress2 = this.GetSelectionAddress(selection.InvokeAndProxy(range => range.EntireColumn));
            if (string.Equals(this.GetSelectionAddress(selection), selectionAddress2, StringComparison.Ordinal))
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Selection was found invalid - entire column(s) selected");
                throw new ModelWrapper.InvalidModelData(Microsoft.Data.Visualization.Client.Excel.Resources.AddToModelInvalidDataSource);
            }
            if (selection.Invoke(range => range.Rows.Count == 1))
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Selection was found invalid - only one row selected");
                throw new ModelWrapper.InvalidModelData(Microsoft.Data.Visualization.Client.Excel.Resources.AddToModelNotEnoughRowsSelected);
            }
            if (!this.IsHeaderRowValid(selection))
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Selection was found invalid - gaps in selected header");
                throw new ModelWrapper.InvalidModelData(Microsoft.Data.Visualization.Client.Excel.Resources.AddToModelGapInHeader);
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "Selection was found to be valid for add to the model scenario");
            return selection;
        }

        private ExcelProxy<WorkbookConnection> FindConnectionByCommandText(string connectionCommandText)
        {
            string commandTextSuffix = "!" + connectionCommandText;
            return this.ExcelWorkbook.InvokeAndProxyOptional(workbook =>
            {
                foreach (ModelTable modelTable in workbook.Model.ModelTables)
                {
                    WorkbookConnection workbookConnection1 = modelTable.SourceWorkbookConnection;
                    if (workbookConnection1 != null && workbookConnection1.Type == XlConnectionType.xlConnectionTypeWORKSHEET)
                    {
                        WorksheetDataConnection worksheetDataConnection = workbookConnection1.WorksheetDataConnection;
                        if (worksheetDataConnection != null)
                        {
                            string a = worksheetDataConnection.CommandText as string;
                            if (a != null)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "comparing connection command text: current={0} requested={1}", a, connectionCommandText);
                                if (a.EndsWith(commandTextSuffix, StringComparison.Ordinal) || string.Equals(a, connectionCommandText, StringComparison.Ordinal))
                                {

                                    WorkbookConnection workbookConnection2 = workbookConnection1;
                                    return workbookConnection2;
                                }
                            }
                        }
                    }
                }
                return (WorkbookConnection)null;
            });
        }

        private static bool InternalModelExists(ExcelProxy<Workbook> workbook)
        {
            ExcelProxy<Model> excelProxy1 = workbook.InvokeAndProxyOptional(workbookArg => workbookArg.Model);
            if (excelProxy1 == null)
                return false;
            ExcelProxy<ModelTables> excelProxy2 = excelProxy1.InvokeAndProxyOptional(modelArg => modelArg.ModelTables);
            return excelProxy2 != null && excelProxy2.Invoke(modelTablesArg => (long)modelTablesArg.Count) != 0L;
        }

        private string GetConnectionName(string fullSelectionAddress)
        {
            return NamePrefixString + fullSelectionAddress;
        }

        private string GetSelectedTableName()
        {
            string tableName = null;
            IEnumerable<int> tableColNumbers = null;
            IEnumerable<int> tableRowNumbers = null;
            this.ExcelApplication.Invoke(app =>
            {
                ListObject listObject = app.ActiveCell.ListObject;
                if (listObject == null)
                    return;
                tableName = listObject.DisplayName;
                tableColNumbers = listObject.Range.Columns.Cast<Range>().Select(col => col.Column);
                tableRowNumbers = listObject.Range.Rows.Cast<Range>().Select(row => row.Row);
            });
            IEnumerable<int> colNumbers = null;
            IEnumerable<int> rowNumbers = null;
            this.ExcelApplication.Invoke(app =>
            {
                Microsoft.Office.Interop.Excel.Range range1 = app.Selection;
                Microsoft.Office.Interop.Excel.Range range2 = range1.Areas.get_Item(range1.Areas.Count);
                colNumbers = range2.Columns.Cast<Range>().Select(col => col.Column);
                rowNumbers = range2.Rows.Cast<Range>().Select(row => row.Row);
            });
            if (tableName != null && tableColNumbers.First() <= colNumbers.First() && (tableRowNumbers.First() <= rowNumbers.First() && tableColNumbers.Last() >= colNumbers.Last()) && tableRowNumbers.Last() >= rowNumbers.Last())
                return tableName;
            return null;
        }

        private string GetActiveWorkbookName()
        {
            string bookName = null;
            this.ExcelApplication.Invoke(app =>
            {
                Workbook activeWorkbook = app.ActiveWorkbook;
                if (activeWorkbook == null)
                    return;
                bookName = activeWorkbook.Name;
            });
            return bookName;
        }

        private string GetActiveWorksheetName()
        {
            string sheetName = null;
            this.ExcelApplication.Invoke(app =>
            {
                Worksheet worksheet = app.ActiveSheet as Worksheet;
                if (worksheet == null)
                    return;
                sheetName = worksheet.Name;
            });
            return sheetName;
        }

        private string GetSelectionAddress()
        {
            string address = null;
            this.ExcelApplication.Invoke(app =>
            {
                Microsoft.Office.Interop.Excel.Range range1 = app.Selection;
                Microsoft.Office.Interop.Excel.Range range2 = range1.Areas.get_Item(range1.Areas.Count);
                address = range2.get_Address(true, true, XlReferenceStyle.xlA1, false, Type.Missing);
            });
            return address;
        }

        private string GetFullSelectionAddress()
        {
            return this.GetSelectedTableName() ?? this.GetWorksheetQualifiedAddress(this.GetSelectionAddress());
        }

        private string GetActiveWorkbookQualifiedTableName(string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}!{1}", new object[2]
      {
        this.GetActiveWorkbookName(),
        tableName
      });
        }

        private string GetWorksheetQualifiedAddress(string address)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}!{1}", new object[2]
      {
        this.GetActiveWorksheetName(),
        address
      });
        }

        private static string GetWorkbookNameFromConnectionString(string connectionString)
        {
            return Path.GetFileName(ModelWrapper.ExcelInternalModelConnectionStringRegEx.Match(connectionString).Groups["Location"].Value);
        }

        private ExcelProxy<Workbook> GetWorkbook(string connectionString)
        {
            string workbookName = ModelWrapper.GetWorkbookNameFromConnectionString(connectionString);
            return this.ExcelApplication.InvokeAndProxy(applicationArg => applicationArg.Workbooks[workbookName]);
        }

        private bool IsConnectionExistInActiveWorkbook(string connectionCommandText)
        {
            return this.ExcelApplication.Invoke(app =>
            {
                foreach (ModelTable modelTable in app.ActiveWorkbook.Model.ModelTables)
                {
                    WorkbookConnection workbookConnection = modelTable.SourceWorkbookConnection;
                    if (workbookConnection != null && workbookConnection.Type == XlConnectionType.xlConnectionTypeWORKSHEET)
                    {
                        WorksheetDataConnection worksheetDataConnection = workbookConnection.WorksheetDataConnection;
                        if (worksheetDataConnection != null)
                        {
                            string a = worksheetDataConnection.CommandText as string;
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "comparing connection command text: current={0} requested={1}", a, connectionCommandText);
                            if (string.Equals(a, connectionCommandText, StringComparison.Ordinal))
                                return true;
                        }
                    }
                }
                return false;
            });
        }

        public class InvalidModelData : Exception
        {
            public InvalidModelData(string message)
                : base(message)
            {
            }

            public InvalidModelData(string message, Exception e)
                : base(message, e)
            {
            }
        }
    }
}
