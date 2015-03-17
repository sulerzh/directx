using Microsoft.Data.Recommendation.Client;
using Microsoft.Data.Recommendation.Client.Classification;
using Microsoft.Data.Recommendation.Common;
using Microsoft.Data.Recommendation.Common.ObjectModel;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Data.Recommendation.Client.PowerMap
{
    internal class GeoflowClassificationAdapter : ClassificationAdapter
    {
        private Dictionary<HostModelIdentifier, string> CategoryPropertyValues;

        internal AnalysisModelAdapter Adapter { get; set; }

        public GeoflowClassificationAdapter(AnalysisModelAdapter adapter)
        {
            this.Adapter = adapter;
            this.CategoryPropertyValues = new Dictionary<HostModelIdentifier, string>();
        }

        public GeoflowClassificationAdapter()
        {
            this.CategoryPropertyValues = new Dictionary<HostModelIdentifier, string>();
        }

        protected override void CleanUpCategoryCache()
        {
            this.CategoryPropertyValues.Clear();
        }

        protected override void UpdateHostClassification()
        {
            List<HostModelIdentifier> list = new List<HostModelIdentifier>();
            if (this.CategoryPropertyValues.Count > 0)
            {
                foreach (HostModelIdentifier key in this.CategoryPropertyValues.Keys)
                {
                    TableColumn column = key.Column;
                    string str;
                    if (this.CategoryPropertyValues.TryGetValue(key, out str) & null != column)
                    {
                        switch (str)
                        {
                            case "Address":
                                column.Classification = TableMemberClassification.Street;
                                break;
                            case "City":
                                column.Classification = TableMemberClassification.City;
                                break;
                            case "CityAndState":
                                column.Classification = TableMemberClassification.Other;
                                break;
                            case "Continent":
                                column.Classification = TableMemberClassification.Other;
                                break;
                            case "Country":
                                column.Classification = TableMemberClassification.Country;
                                break;
                            case "County":
                                column.Classification = TableMemberClassification.County;
                                break;
                            case "PostalCode":
                                column.Classification = TableMemberClassification.PostalCode;
                                break;
                            case "StateOrProvince":
                                column.Classification = TableMemberClassification.State;
                                break;
                            case "Latitude":
                                column.Classification = TableMemberClassification.Latitude;
                                break;
                            case "Longitude":
                                column.Classification = TableMemberClassification.Longitude;
                                break;
                            case "Place":
                                column.Classification = TableMemberClassification.Other;
                                break;
                            case "XCoord":
                                column.Classification = TableMemberClassification.XCoord;
                                break;
                            case "YCoord":
                                column.Classification = TableMemberClassification.YCoord;
                                break;
                            default:
                                column.Classification = TableMemberClassification.None;
                                break;
                        }
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, string.Format("UpdateHostClassifications: Column {0}, category={1}, classified as {2}.", column.ModelQueryName, str, ((object)column.Classification).ToString()));
                    }
                }
            }
            if ((ClientDiagnostics.Instance.TraceLevel & SourceLevels.Verbose) == SourceLevels.Verbose)
                this.TraceColumnClassifications(this.CategoryPropertyValues.Keys, TraceEventType.Verbose);
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, string.Format("UpdateHostClassifications completed on {0} columns.", this.CategoryPropertyValues.Count));
        }

        protected override void UpdateObjectPathProperty(GetClassificationsResponseItem responseItem, Guid columnId, Dictionary<Guid, IHostModelIdentifier> hostIdMap)
        {
            IHostModelIdentifier hostModelIdentifier;
            if (!hostIdMap.TryGetValue(columnId, out hostModelIdentifier))
                return;
            HostModelIdentifier key = hostModelIdentifier as HostModelIdentifier;
            if (key == null)
                return;
            string categoryName = GeoflowClassificationAdapter.GetCategoryName(responseItem.ClassificationResults);
            if (string.IsNullOrEmpty(categoryName))
                return;
            this.CategoryPropertyValues.Add(key, categoryName);
        }

        private static string GetCategoryName(List<ClassificationResult> classificationResults)
        {
            string str = string.Empty;
            double num = 0.0;
            foreach (ClassificationResult classificationResult in classificationResults)
            {
                if (classificationResult.FeatureId != Guid.Empty)
                {
                    foreach (ClientDataTypeMap clientDataTypeMap in ServiceAccessFactory.ClientSpecificMetaDataProvider.ClientDataTypeMaps)
                    {
                        if (clientDataTypeMap.FeatureId.Equals(classificationResult.FeatureId))
                        {
                            if (num < classificationResult.Weight)
                            {
                                num = classificationResult.Weight;
                                str = clientDataTypeMap.UdmType;
                            }
                            break;
                        }
                    }
                }
            }
            return str;
        }

        protected override string GetColumnClassification(Column column, bool onlyUserCategory, bool allowCustomCategory)
        {
            return string.Empty;
        }

        private void TraceColumnClassifications(IEnumerable<HostModelIdentifier> columnCategoryPaths, TraceEventType traceLevel)
        {
            foreach (HostModelIdentifier key in columnCategoryPaths)
            {
                string str = "";
                if (this.CategoryPropertyValues.TryGetValue(key, out str))
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, string.Format("{0} = {1}", key.Column.ModelQueryName, str));
            }
        }
    }
}
