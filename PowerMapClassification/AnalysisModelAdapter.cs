using ADODB;
using Microsoft.Data.Recommendation.Client;
using Microsoft.Data.Recommendation.Client.Classification;
using Microsoft.Data.Recommendation.Client.PowerMap.Sampler;
using Microsoft.Data.Recommendation.Client.Storage;
using Microsoft.Data.Recommendation.Common;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Stream = System.IO.Stream;

namespace Microsoft.Data.Recommendation.Client.PowerMap
{
    public class AnalysisModelAdapter : IDisposable
    {
        public AnalysisModelController Controller;
        private static bool? configurationInstanceIsInitialized;

        public SampleManager SampleManager { get; private set; }

        public ClassificationAdapter Classifier { get; private set; }

        public static bool IsInitialized
        {
            get
            {
                if (!configurationInstanceIsInitialized.HasValue)
                {
                    try
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Orlando IsInitialized.get: calling Configuration.Instance.InitializedEvent.WaitOne.");
                        Configuration.Instance.InitializedEvent.WaitOne();
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "Orlando IsInitialized.get: Configuration.Instance.InitializedEvent.WaitOne completed.");
                        if (Configuration.Instance.IsInitialized && Configuration.Instance.FeatureAnalyzerBuilder != null)
                        {
                            FeatureAnalyzer analyzer = Configuration.Instance.FeatureAnalyzerBuilder.CreateAnalyzer();
                            configurationInstanceIsInitialized = analyzer.FeatureExtractors != null && analyzer.FeatureExtractors.Count != 0;
                        }
                        else
                            configurationInstanceIsInitialized = false;
                    }
                    catch (Exception ex)
                    {
                        VisualizationTraceSource.Current.Fail("Orlando IsInitialized.get failed, setting IsInitialized to false.", ex);
                        configurationInstanceIsInitialized = false;
                    }
                }
                return configurationInstanceIsInitialized.Value;
            }
        }

        public AnalysisModelAdapter(Connection connection)
        {
            try
            {
                SampleManager = new GeoflowSampleManager(connection);
                Classifier = new GeoflowClassificationAdapter();
                Controller = new AnalysisModelController("PowerMap", SampleManager, Classifier,
                    TimeSpan.FromMilliseconds(0.0), new List<string>()
                    {
                        "Country",
                        "StateOrProvince",
                        "County",
                        "CityAndState",
                        "City",
                        "Address",
                        "PostalCode",
                        "Latitude",
                        "Longitude"
                    });
                Controller.ResetPeriod = TimeSpan.Zero;
                Controller.MaxResetCountPerPeriod = 1000;
                Controller.Initialize();
            }
            catch (Exception ex)
            {
                ClientDiagnostics.Instance.TraceUnhandledException(TraceComponent.ModelBuilder, "Error during Power Map classification model initialization", ex);
                VisualizationTraceSource.Current.Fail("Error during Power Map classification model initialization, setting this.Controller to null", ex);
                Controller = null;
            }
        }

        public static void Initialize()
        {
            if (Configuration.Instance.IsInitialized)
                return;
            if (configurationInstanceIsInitialized.HasValue)
                return;
            try
            {
                RegistrySettings.BaseKey = "Software\\Microsoft\\Office\\15.0\\Excel";
                FileSystemStore.BaseFolder = "Microsoft/PowerMap/";
                ClientDiagnostics.Initialize();
                string clientId = string.Format("{0}_{1}_{2}", "Power Map", "1", Assembly.GetExecutingAssembly().GetName().Version.Major);
                Dictionary<CultureInfo, string> dictionary = new Dictionary<CultureInfo, string>();
                dictionary.Add(CultureInfo.GetCultureInfo("en-us"), LoadResourceAsString("Microsoft.Data.Recommendation.Client.PowerMap.Resources.LocalizedStrings.xml"));
                Configuration.Instance.HostSettingsImplementation = new MockHostSettings();
                Configuration.Instance.StartInitialization(RegistrySettings.GetServerUri(), clientId, RegistrySettings.GetUserId(), (RegistrySettings.IsFipsEnabled() ? 1 : 0) != 0, new ClientSpecificMetaData()
                {
                    ClientDataTypeMapXml = LoadResourceAsString("Microsoft.Data.Recommendation.Client.PowerMap.Resources.ClientDataTypeMap.xml"),
                    ClientDataTypeMapXsd = LoadResourceAsString("Microsoft.Data.Recommendation.Client.PowerMap.Resources.ClientDataTypeMap.xsd"),
                    DecisionTreeXml = LoadResourceAsString("Microsoft.Data.Recommendation.Client.PowerMap.Resources.DecisionTree.xml"),
                    LocalizedStringsXml = dictionary,
                    LocalizedStringsXsd = LoadResourceAsString("Microsoft.Data.Recommendation.Client.PowerMap.Resources.LocalizedStrings.xsd")
                });
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail("Orlando initialization failed.", ex);
                Configuration.Instance.InitializedEvent.Set();
            }
        }

        private static string LoadResourceAsString(string resourceName)
        {
            try
            {
                using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    using (StreamReader streamReader = new StreamReader(manifestResourceStream))
                        return streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail(string.Format("Loading Orlando Resource {0} failed with exception", resourceName), ex);
                throw;
            }
        }

        public bool StartClassification(ModelMetadata modelMetaData)
        {
            if (modelMetaData == null || Controller == null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "StartClassification(): returning false: modelMetadata is{0} null, this.Controller is{1} null", modelMetaData == null ? (object)string.Empty : (object)" not", Controller == null ? (object)string.Empty : (object)" not");
                return false;
            }
            if (Controller.Model == null)
                Controller.Model = new AnalysisModel();
            AnalysisModel model = Controller.Model;
            Queue<ModelChangeNotification> changes = new Queue<ModelChangeNotification>();
            foreach (TableIsland tableIsland in modelMetaData.TableIslands)
            {
                foreach (TableMetadata table in tableIsland.Tables)
                {
                    if (table.Visible)
                        MarkTableAsDirty(table, changes);
                }
            }
            if (changes.Count > 0)
                Controller.ModelChanged(changes.ToArray());
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "StartClassification(): Queuing {0} changes (= total number of columns for Orlando)", (object)changes.Count);
            model.RequiresInitialization = false;
            return changes.Count > 0;
        }

        public void MarkTableAsDirty(TableMetadata table, Queue<ModelChangeNotification> changes)
        {
            HostModelIdentifier hostModelIdentifier = new HostModelIdentifier(table.ModelName, Controller.Model.Tableset.Name, AnalysisObjectType.Table);
            ModelChangeNotification changeNotification1 = new ModelChangeNotification(DirtyReason.Added, hostModelIdentifier, (IHostModelIdentifier)Controller.Model.Tableset.HostObjectId);
            changes.Enqueue(changeNotification1);
            foreach (TableField tableField in table.Fields)
            {
                TableColumn hostColumn = tableField as TableColumn;
                if (hostColumn != null && hostColumn.Visible && (TableMemberDataType.String == hostColumn.DataType || TableMemberDataType.Long == hostColumn.DataType || TableMemberDataType.Double == hostColumn.DataType))
                {
                    ModelChangeNotification changeNotification2 = new ModelChangeNotification(DirtyReason.Added, new HostModelIdentifier(hostColumn.Name, table.ModelName, AnalysisObjectType.Column, hostColumn), hostModelIdentifier);
                    changes.Enqueue(changeNotification2);
                }
            }
        }

        public bool WaitForClassifications(TimeSpan timeout)
        {
            return Controller.StateMachine.WaitForState(ClientState.ReadyToRecommend, timeout);
        }

        public void Reset()
        {
            try
            {
                Controller.Reset();
            }
            catch (Exception ex)
            {
                ClientDiagnostics.Instance.TraceUnhandledException(TraceComponent.ModelBuilder, "Error during controller reset", ex);
            }
        }

        public void Dispose()
        {
        }
    }
}
