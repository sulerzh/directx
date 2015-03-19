using Microsoft.Data.Visualization.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class LayerDefinition
    {
        private object revisionCounterLock = new object();
        private GeoVisualization geoVisualization;
        private object syncRoot;
        private string name;
        private bool visible;
        private CancellationTokenSource cancellationSource;
        private int revisionCount;
        private Guid revisionCountGuid;
        private int allowRevisionIncrementsCounter;
        private bool revisionCountIncrementPending;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (!(value != this.name))
                    return;
                this.name = value;
                this.DisplayPropertiesUpdated(false);
            }
        }

        public bool Visible
        {
            get
            {
                return this.visible;
            }
            set
            {
                if (this.visible == value)
                    return;
                this.visible = value;
                if (this.GeoVisualization == null)
                    return;
                bool flag = !value && this.GeoVisualization.Visible;
                bool visible = value && this.GeoVisualization.Visible;
                if (flag == visible)
                    return;
                this.GeoVisualization.OnVisibleChanged(visible);
            }
        }

        public string ModelTableNameForAutoGeocoding { get; private set; }

        public bool Deserialized { get; private set; }

        public bool RefreshedAfterModelMetadataChanged { get; private set; }

        public Guid Id { get; private set; }

        public Guid RevisionCountGuid
        {
            get
            {
                lock (this.revisionCounterLock)
                {
                    if (this.revisionCountGuid == Guid.Empty)
                        this.revisionCountGuid = Guid.NewGuid();
                    return this.revisionCountGuid;
                }
            }
        }

        public string Key
        {
            get
            {
                return this.Id.ToString("N") + "|" + this.RevisionCountGuid.ToString("N");
            }
        }

        public Guid DataLoadedCustomSpaceId { get; set; }

        public LayerManager LayerManager { get; private set; }

        public GeoVisualization GeoVisualization
        {
            get
            {
                lock (this.syncRoot)
                    return this.geoVisualization;
            }
        }

        public DateTime? PlayFromTime
        {
            get
            {
                if (this.GeoVisualization == null)
                    return new DateTime?();
                return this.GeoVisualization.PlayFromTime;
            }
        }

        public DateTime? PlayToTime
        {
            get
            {
                if (this.GeoVisualization == null)
                    return new DateTime?();
                return this.GeoVisualization.PlayToTime;
            }
        }

        internal bool ForInstructionsOnly { get; set; }

        private int RevisionCount
        {
            get
            {
                lock (this.revisionCounterLock)
                    return this.revisionCount;
            }
        }

        private string GeoDataSourceName
        {
            get
            {
                return this.Name + " GeoVisualizationDataSource";
            }
        }

        public event Action DisplayPropertiesChanged;

        public LayerDefinition(LayerManager layerManager, string name, string modelTableName = null, bool forInstructionsOnly = false)
        {
            if (layerManager == null)
                throw new ArgumentNullException("layerManager");
            if (name == null)
                throw new ArgumentNullException("name");
            this.syncRoot = new object();
            this.LayerManager = layerManager;
            this.allowRevisionIncrementsCounter = 1;
            this.Name = name;
            this.Visible = true;
            this.Id = Guid.NewGuid();
            this.revisionCount = 0;
            this.revisionCountGuid = Guid.Empty;
            this.allowRevisionIncrementsCounter = 0;
            this.revisionCountIncrementPending = false;
            this.ForInstructionsOnly = forInstructionsOnly;
            this.ModelTableNameForAutoGeocoding = modelTableName;
            this.Deserialized = false;
            this.RefreshedAfterModelMetadataChanged = false;
        }

        internal LayerDefinition(LayerDefinition.SerializableLayerDefinition state, LayerManager layerManager, LayerDefinition inUseLayerWithSameKey, CultureInfo modelCulture)
            : this(layerManager, state.Name, null, state.ForInstructionsOnly)
        {
            this.Unwrap(state, inUseLayerWithSameKey, modelCulture);
            this.Deserialized = true;
        }

        internal void PrepareData(Guid cookie, LayerManager.Settings layerManagerSettings, bool layerBeingReused)
        {
            lock (this.syncRoot)
            {
                if (this.geoVisualization == null)
                    return;
                if (this.ForInstructionsOnly)
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerDefinition.PrepareData(cookie={0}): ForInstructionsOnly is true for layerDefinition (layerbeingReused={1}, with id = {2}", (object)cookie, layerBeingReused, (object)this.Id);
                else if (this.geoVisualization.ShouldPrepareData(layerManagerSettings))
                {
                    Interlocked.Increment(ref layerManagerSettings.InProgressQueryCount);
                    if (!layerBeingReused)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerDefinition.PrepareData(cookie={0}): Calling PrepareData for not-reused layerDefinition with id = {1}", (object)cookie, (object)this.Id);
                        this.BeginInvokeAction(((Visualization)this.geoVisualization).PrepareData, false, (CancellationTokenSource)null, false, false, (LayerManager.Settings)null, (Action<object, bool, bool, Exception>)null, (object)null);
                    }
                    else
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerDefinition.PrepareData(cookie={0}): ShouldPrepareData is true for reused layerDefinition with id = {1}", (object)cookie, (object)this.Id);
                }
                else
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerDefinition.PrepareData(cookie={0}): ShouldPrepareData is false for layerDefinition (layerbeingReused={1}, with id = {2}", (object)cookie, layerBeingReused, (object)this.Id);
            }
        }

        public void RefreshDisplay(bool zoomToData = false, CancellationTokenSource cancellationTokenSource = null, bool forceRequeryIfVisible = false, bool forciblyRefreshDisplay = false, LayerManager.Settings layerManagerSettings = null, Action<object, bool, bool, Exception> completionCallback = null, object context = null)
        {
            lock (this.syncRoot)
            {
                if (this.geoVisualization == null)
                    return;
                this.BeginInvokeAction(((Visualization)this.geoVisualization).RefreshDisplay, zoomToData, cancellationTokenSource, forceRequeryIfVisible, forciblyRefreshDisplay, layerManagerSettings, completionCallback, context);
            }
        }

        public GeoVisualization AddGeoVisualization(DataSource dataSource = null)
        {
            lock (this.syncRoot)
            {
                if (this.geoVisualization != null)
                    throw new InvalidOperationException("The layer already has a geo-visualization");
                this.DisallowIncrementRevisionCount();
                this.geoVisualization = new GeoVisualization(this, dataSource ?? this.LayerManager.CreateDataSource(this.GeoDataSourceName));
                this.DisplayPropertiesUpdated(true);
                this.AllowIncrementRevisionCount(false);
                return this.geoVisualization;
            }
        }

        public bool RemoveGeoVisualization()
        {
            GeoVisualization geoVisualization;
            lock (this.syncRoot)
            {
                CancellationTokenSource local_2 = Interlocked.Exchange(ref this.cancellationSource, null);
                if (local_2 != null)
                {
                    try
                    {
                        local_2.Cancel(false);
                    }
                    catch (ObjectDisposedException exception_0)
                    {
                    }
                }
                geoVisualization = this.geoVisualization;
                this.geoVisualization = null;
            }
            bool flag = geoVisualization != null;
            if (flag)
            {
                this.DisplayPropertiesUpdated(true);
                geoVisualization.Removed();
            }
            return flag;
        }

        public bool Remove()
        {
            return this.LayerManager.RemoveLayerDefinition(this);
        }

        internal void Removed()
        {
            this.RemoveGeoVisualization();
            this.Shutdown();
        }

        internal void UpdateDisplayProperties(LayerDefinition newLayer, LayerManager.Settings settings, Action visibilityChangedCallback)
        {
            this.DisallowIncrementRevisionCount();
            bool wasVisible = this.Visible && (this.geoVisualization == null || this.geoVisualization.Visible);
            this.Name = newLayer.Name;
            this.Visible = newLayer.Visible;
            this.Deserialized = true;
            if (newLayer.GeoVisualization == null)
                this.RemoveGeoVisualization();
            else if (this.GeoVisualization == null)
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "UpdateDisplayProperties() LayerDefinition did not have a GeoVisualization and is being replaced by a layer with a GeoVis");
            else
                this.GeoVisualization.UpdateDisplayProperties(newLayer.GeoVisualization, settings, visibilityChangedCallback, wasVisible);
            newLayer.Removed();
            this.NotifyDisplayPropertiesChanged();
            this.AllowIncrementRevisionCount(true);
        }

        internal void DisplayPropertiesUpdated(bool refreshDisplayRequired)
        {
            bool flag;
            if (refreshDisplayRequired)
            {
                lock (this.revisionCounterLock)
                    flag = this.UnlockedIncrementRevisionCount();
            }
            else
                flag = true;
            if (!flag)
                return;
            this.LayerManager.NotifyStateChanged(this);
        }

        internal void AllowIncrementRevisionCount(bool discardPendingCounts = false)
        {
            bool flag = false;
            lock (this.revisionCounterLock)
            {
                --this.allowRevisionIncrementsCounter;
                if (this.allowRevisionIncrementsCounter == 0)
                {
                    if (this.revisionCountIncrementPending)
                    {
                        if (discardPendingCounts)
                            this.revisionCountIncrementPending = false;
                        else
                            flag = this.UnlockedIncrementRevisionCount();
                    }
                }
            }
            if (!flag)
                return;
            this.LayerManager.NotifyStateChanged(this);
        }

        internal void DisallowIncrementRevisionCount()
        {
            lock (this.revisionCounterLock)
                ++this.allowRevisionIncrementsCounter;
        }

        internal void ModelMetadataChanged(ModelMetadata modelMetadata, List<string> tablesWithUpdatedData, ref bool requery, ref bool queryChanged)
        {
            lock (this.syncRoot)
            {
                if (this.geoVisualization == null)
                    return;
                this.geoVisualization.ModelMetadataChanged(modelMetadata, tablesWithUpdatedData, ref requery, ref queryChanged);
                this.RefreshedAfterModelMetadataChanged = true;
            }
        }

        internal LayerDefinition.SerializableLayerDefinition Wrap()
        {
            return new LayerDefinition.SerializableLayerDefinition()
            {
                Name = this.Name,
                Id = this.Id,
                ForInstructionsOnly = this.ForInstructionsOnly,
                RevisionCount = this.RevisionCount,
                RevisionCountGuid = this.RevisionCountGuid,
                Visible = this.Visible,
                Geo = this.GeoVisualization == null ? null : this.GeoVisualization.Wrap()
            };
        }

        internal void Unwrap(LayerDefinition.SerializableLayerDefinition state, LayerDefinition inUseLayerWithSameKey, CultureInfo modelCulture)
        {
            if (state == null)
                throw new ArgumentNullException("state");
            if (this.geoVisualization != null)
                throw new InvalidOperationException(string.Format("A GeoVisualization has already been created for layerdefinition'{0}', Id={1}", this.Name, this.Id));
            this.Name = state.Name;
            this.Id = state.Id;
            this.Visible = state.Visible;
            this.revisionCount = state.RevisionCount;
            this.revisionCountGuid = state.RevisionCountGuid;
            this.DisallowIncrementRevisionCount();
            GeoVisualization geoVisualization = inUseLayerWithSameKey == null ? null : inUseLayerWithSameKey.GeoVisualization;
            DataSource dataSource1 = geoVisualization == null ? null : geoVisualization.DataSource;
            DataSource dataSource2;
            if (dataSource1 != null)
            {
                dataSource2 = dataSource1;
                dataSource2.IncrementReuseCount();
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "LayerDefinition.Unwrap(): reusing data source for layer Id={1}, layer key={2}", (object)this.Name, (object)this.Id, (object)this.Key);
            }
            else
            {
                dataSource2 = this.LayerManager.CreateDataSource(this.GeoDataSourceName);
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "LayerDefinition.Unwrap(): creating new data source for layer Id={1}, after query runs", (object)this.Name, (object)this.Id);
            }
            this.geoVisualization = state.Geo == null ? null : state.Geo.Unwrap(this, dataSource2, modelCulture);
            this.AllowIncrementRevisionCount(true);
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "LayerDefinition.Unwrap() completed, name={0}, Id={1}", (object)this.Name, (object)this.Id);
        }

        private void NotifyDisplayPropertiesChanged()
        {
            if (this.DisplayPropertiesChanged == null)
                return;
            this.DisplayPropertiesChanged();
        }

        private bool UnlockedIncrementRevisionCount()
        {
            bool flag = false;
            if (this.allowRevisionIncrementsCounter == 0)
            {
                this.revisionCountIncrementPending = false;
                this.revisionCountGuid = Guid.Empty;
                ++this.revisionCount;
                flag = true;
            }
            else
                this.revisionCountIncrementPending = true;
            return flag;
        }

        private void Shutdown()
        {
            this.DisplayPropertiesChanged = null;
            this.geoVisualization = null;
            this.name = string.Empty;
            this.LayerManager = null;
        }

        private void BeginInvokeAction(Action<bool, CancellationTokenSource, bool, bool, LayerManager.Settings> action, bool zoomToData = false, CancellationTokenSource clientCancellationSource = null, bool forceRequeryIfVisible = false, bool forciblyRefreshDisplay = false, LayerManager.Settings layerManagerSettings = null, Action<object, bool, bool, Exception> completionCallback = null, object context = null)
        {
            CancellationTokenSource cancellationTokenSource1 = new CancellationTokenSource();
            CancellationTokenSource cancellationTokenSource2 = Interlocked.Exchange(ref this.cancellationSource, cancellationTokenSource1);
            CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(clientCancellationSource == null ? CancellationToken.None : clientCancellationSource.Token, cancellationTokenSource1.Token);
            if (cancellationTokenSource2 != null)
            {
                try
                {
                    cancellationTokenSource2.Cancel(false);
                }
                catch (ObjectDisposedException ex)
                {
                }
            }
            Action
                <bool, CancellationTokenSource, CancellationTokenSource, bool, bool,
                    Microsoft.Data.Visualization.VisualizationControls.LayerManager.Settings> method =
                        delegate(bool zoom, CancellationTokenSource linkedSource,
                            CancellationTokenSource cancellationSource,
                            bool requery, bool refreshDisplay,
                            Microsoft.Data.Visualization.VisualizationControls.LayerManager.Settings settings)
                        {
                            bool flag1 = false;
                            Exception exception = null;
                            bool flag2 = false;
                            try
                            {
                                action(zoom, linkedSource, requery, refreshDisplay, settings);
                                flag1 = true;
                            }
                            catch (OperationCanceledException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0,
                                    "LayerDefinition {0} (Guid = {1}): User cancelled query evaluation (caught OperationCanceledException)",
                                    (object)this.Name, (object)this.Id);
                                flag2 = true;
                            }
                            catch (ThreadAbortException ex)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0,
                                    "LayerDefinition {0} (Guid = {1}): Caught ThreadAbortException", (object)this.Name,
                                    (object)this.Id);
                            }
                            catch (DataSource.InvalidQueryResultsException ex)
                            {
                                VisualizationTraceSource.Current.Fail(
                                    "QueryEngine caught and ignored exception in query evaluation", ex);
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0,
                                    "LayerDefinition {0} (Guid = {1}): InvokeActionOnDispatcher() caught InvalidQueryResultsException, QueryResultsFailed = {2}, QueryResultsFailed = {3}, innerException={4}",
                                    (object)this.Name, (object)this.Id,
                                    ex.QueryEvaluationFailed,
                                    ex.QueryResultsStale, (object)ex.InnerException);
                                exception = ex;
                            }
                            catch (Exception ex)
                            {
                                VisualizationTraceSource.Current.Fail("QueryEngine caught and ignored exception", ex);
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0,
                                    "LayerDefinition {0} (Guid = {1}): InvokeActionOnDispatcher() caught and ignored exception {2}",
                                    (object)this.Name, (object)this.Id, (object)ex);
                                exception = ex;
                            }
                            finally
                            {
                                Interlocked.CompareExchange(ref this.cancellationSource,
                                    null, cancellationSource);
                                cancellationSource.Dispose();
                                if (completionCallback != null)
                                {
                                    try
                                    {
                                        completionCallback(context, flag1, flag2, exception);
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                            }
                        };
            this.LayerManager.QueryEngineDispatcher.BeginInvoke(method, zoomToData, (object)linkedTokenSource, (object)cancellationTokenSource1, forceRequeryIfVisible, forciblyRefreshDisplay, (object)layerManagerSettings);
        }

        [Serializable]
        public class SerializableLayerDefinition
        {
            [XmlAttribute]
            public string Name;
            [XmlAttribute("Guid")]
            public Guid Id;
            [XmlAttribute("Rev")]
            public int RevisionCount;
            [XmlAttribute("RevGuid")]
            public Guid RevisionCountGuid;
            [XmlAttribute]
            public bool Visible;
            [XmlAttribute("InstOnly")]
            public bool ForInstructionsOnly;
            [XmlElement("GeoVis")]
            public GeoVisualization.SerializableGeoVisualization Geo;

            [XmlIgnore]
            internal string Key
            {
                get
                {
                    return this.Id.ToString("N") + "|" + this.RevisionCountGuid.ToString("N");
                }
            }

            internal LayerDefinition Unwrap(LayerManager layerManager, LayerDefinition inUseLayerWithSameKey, CultureInfo modelCulture)
            {
                return new LayerDefinition(this, layerManager, inUseLayerWithSameKey, modelCulture);
            }
        }
    }
}
