using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public abstract class DataSource
    {
        public const int InvalidDataVersion = 0;
        protected int reuseCount;

        public string Name { get; private set; }

        public CultureInfo ModelCulture { get; set; }

        public int DataVersion { get; protected set; }

        protected internal bool OkayToShutdown
        {
            get
            {
                return this.reuseCount == 0;
            }
        }

        public bool FieldsChangedSinceLastQuery { get; protected set; }

        protected List<DataView> DataViews { get; private set; }

        protected abstract bool NoQueryResults { get; }

        public DataSource(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            this.Name = name;
            this.DataVersion = 0;
            this.DataViews = new List<DataView>();
            this.FieldsChangedSinceLastQuery = true;
            this.ModelCulture = CultureInfo.InvariantCulture;
        }

        internal void IncrementReuseCount()
        {
            Interlocked.Increment(ref this.reuseCount);
        }

        internal void DecrementReuseCount()
        {
            Interlocked.Decrement(ref this.reuseCount);
        }

        public bool RunQuery(CancellationToken cancellationToken, bool forceRequery = false, bool shouldRunMainQuery = false, bool incrementDataVersion = false)
        {
            bool flag1 = forceRequery || this.FieldsChangedSinceLastQuery || shouldRunMainQuery;
            bool flag2 = false;
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (flag1)
                {
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "{0} Starting query, forceRequery={1}", (object)this.Name, forceRequery);
                    flag2 = this.QueryData(cancellationToken, shouldRunMainQuery);
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "{0} Finished running query, forceRequery={1}", (object)this.Name, forceRequery);
                    this.FieldsChangedSinceLastQuery = false;
                }
                if (!flag2)
                {
                    if (incrementDataVersion)
                    {
                        ++this.DataVersion;
                        flag2 = true;
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "{0} Caught OperationCanceledException, clearing data views, forceRequery={1}, dataChanged={2}", (object)this.Name, forceRequery, flag1);
                if (flag2)
                    this.ClearStaleDataViews();
                throw;
            }
            return flag2;
        }

        public void UpdateViews(CancellationToken cancellationToken, bool dataChanged, bool zoomToData, bool updateDisplay = true)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                this.UpdateDataViews(cancellationToken, dataChanged, zoomToData, updateDisplay);
            }
            catch (OperationCanceledException ex)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "{0} Caught OperationCanceledException, clearing data views, updateDisplay={1}, dataChanged={2}", (object)this.Name, updateDisplay, dataChanged);
                if (dataChanged)
                    this.ClearStaleDataViews();
                throw;
            }
        }

        internal DataView CreateDataView(DataView.DataViewType type)
        {
            DataView dataView = this.InstantiateDataView(type);
            List<DataView> dataViews = this.DataViews;
            if (dataViews == null)
                return dataView;
            dataViews.Add(dataView);
            return dataView;
        }

        internal void RemoveDataView(DataView view)
        {
            List<DataView> dataViews = this.DataViews;
            if (dataViews == null)
                return;
            dataViews.Remove(view);
        }

        protected internal virtual void Shutdown()
        {
            if (!this.OkayToShutdown)
                return;
            this.Name = string.Empty;
            List<DataView> dataViews = this.DataViews;
            this.DataViews = null;
        }

        protected void UpdateDataViews(CancellationToken cancellationToken, bool dataChanged = true, bool zoomToData = false, bool updateDisplay = true)
        {
            if (!dataChanged && !updateDisplay)
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "{0} Starting to update views, dataChanged={1}, updateDisplay={2}", (object)this.Name, dataChanged, updateDisplay);
            List<DataView> dataViews = this.DataViews;
            if (dataViews != null)
            {
                DataView[] array = new DataView[dataViews.Count];
                dataViews.CopyTo(array);
                foreach (DataView dataView in array)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    dataView.Update(cancellationToken, this.DataVersion, zoomToData, updateDisplay);
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "{0} Finished updating views, dataChanged={1}, updateDisplay={2}", (object)this.Name, dataChanged, updateDisplay);
            }
            else
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Warning, 0, "{0} DataViews colloection was null, dataChanged={1}, updateDisplay={2}", (object)this.Name, dataChanged, updateDisplay);
        }

        protected void ClearStaleDataViews()
        {
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "{0} Starting to clear stale views", (object)this.Name);
            List<DataView> dataViews = this.DataViews;
            if (dataViews == null)
                return;
            DataView[] array = new DataView[dataViews.Count];
            dataViews.CopyTo(array);
            foreach (DataView dataView in array)
                dataView.Clear(this.DataVersion, true);
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "{0} Finished clearing stale views", (object)this.Name);
        }

        protected void VerifyQueryResultsAreCurrent(int sourceDataVersion)
        {
            if (this.NoQueryResults)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "{0}: VerifyQueryResultsAreCurrent(): No query results - this.DataVersion={1}, sourceDataVersion={2}, this.FieldsChangedSinceLastQuery={3}", (object)this.Name, (object)this.DataVersion, (object)sourceDataVersion, this.FieldsChangedSinceLastQuery);
                throw new DataSource.InvalidQueryResultsException("Query results null or empty");
            }
            else
            {
                if (this.DataVersion == sourceDataVersion && !this.FieldsChangedSinceLastQuery)
                    return;
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "{0}: VerifyQueryResultsAreCurrent(): Query results stale - this.DataVersion={1}, sourceDataVersion={2}, this.FieldsChangedSinceLastQuery={3}", (object)this.Name, (object)this.DataVersion, (object)sourceDataVersion, this.FieldsChangedSinceLastQuery);
                throw new DataSource.InvalidQueryResultsException("Query results stale")
                {
                    QueryResultsStale = true
                };
            }
        }

        public abstract void SetFieldsFromFieldWellDefinition(FieldWellDefinition wellDefinition, bool forceMainQuery);

        public abstract int GetQueryResultsItemCount(int sourceDataVersion);

        public abstract string ModelDataIdForId(InstanceId id, bool anyMeasure, bool anyCategoryValue);

        public abstract string ModelDataIdForSeriesIndex(int seriesIndex);

        protected abstract DataView InstantiateDataView(DataView.DataViewType type);

        protected abstract bool QueryData(CancellationToken cancellationToken, bool shouldRunMainQuery);

        public class InvalidQueryResultsException : Exception
        {
            public bool QueryResultsStale { get; set; }

            public bool QueryEvaluationFailed { get; set; }

            public InvalidQueryResultsException(string message)
                : base(message)
            {
            }

            public InvalidQueryResultsException(string message, Exception e)
                : base(message, e)
            {
            }
        }
    }
}
