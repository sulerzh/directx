using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Data.Visualization.Engine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    /// <summary>
    /// 定义可视化抽象父类，目前的继承类有GeoVisualization和ChartVisualization
    /// </summary>
    public abstract class Visualization
    {
        private object queryCheckLock = new object();
        private List<LayerManager.Settings> queryCompletionNotificationList = new List<LayerManager.Settings>();
        protected bool initialized;
        protected bool visible;

        public virtual bool Visible
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
                bool flag = !value && this.LayerDefinition.Visible;
                bool visible = value && this.LayerDefinition.Visible;
                if (flag == visible)
                    return;
                this.OnVisibleChanged(visible);
            }
        }

        public bool ShouldRefreshDisplay
        {
            get
            {
                DataBinding dataBinding = this.DataBinding;
                if (this.RefreshDisplayPending)
                    return true;
                if (dataBinding != null)
                    return dataBinding.DisplayNeedsRefresh;
                else
                    return false;
            }
        }

        public DataSource DataSource { get; protected set; }

        public DataBinding DataBinding { get; protected set; }

        public LayerDefinition LayerDefinition { get; internal set; }

        public FieldWellDefinition FieldWellDefinition { get; protected set; }

        public object VisualElement
        {
            get
            {
                if (this.DataBinding == null)
                    return null;
                return this.DataBinding.VisualElement;
            }
        }

        internal VisualizationModel VisualizationModel
        {
            get
            {
                LayerDefinition layerDefinition = this.LayerDefinition;
                LayerManager layerManager = layerDefinition == null ? null : layerDefinition.LayerManager;
                if (layerManager != null)
                    return layerManager.Model;
                return null;
            }
        }

        protected bool RefreshDisplayPending { get; set; }

        internal Visualization(SerializableVisualization state, LayerDefinition layerDefinition, DataSource dataSource)
        {
            if (layerDefinition == null)
                throw new ArgumentNullException("layerDefinition");
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            this.initialized = false;
            this.DataSource = dataSource;
            this.LayerDefinition = layerDefinition;
        }

        internal Visualization(LayerDefinition layerDefinition, DataSource dataSource)
        {
            if (layerDefinition == null)
                throw new ArgumentNullException("layerDefinition");
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            this.initialized = false;
            this.LayerDefinition = layerDefinition;
            this.DataSource = dataSource;
            this.Visible = true;
        }

        public void BeginSettingsUpdates()
        {
            LayerDefinition layerDefinition = this.LayerDefinition;
            if (layerDefinition == null)
                return;
            layerDefinition.DisallowIncrementRevisionCount();
        }

        public void EndSettingsUpdates()
        {
            LayerDefinition layerDefinition = this.LayerDefinition;
            if (layerDefinition == null)
                return;
            layerDefinition.AllowIncrementRevisionCount(false);
        }

        internal bool ShouldPrepareData(LayerManager.Settings layerManagerSettings)
        {
            LayerDefinition layerDefinition = this.LayerDefinition;
            if (layerDefinition == null || (!this.Visible || !layerDefinition.Visible))
                return false;
            FieldWellDefinition fieldWellDefinition = this.FieldWellDefinition;
            if (fieldWellDefinition == null)
                return false;
            lock (this.queryCheckLock)
            {
                if (!fieldWellDefinition.FieldsChangedSinceLastQuery)
                    return false;
                lock (this.queryCompletionNotificationList)
                    this.queryCompletionNotificationList.Add(layerManagerSettings);
                return true;
            }
        }

        internal void NotifyQueryCompletion(LayerManager layerManager, bool isVisible, Exception ex)
        {
            lock (this.queryCompletionNotificationList)
            {
                foreach (LayerManager.Settings s in this.queryCompletionNotificationList)
                {
                    try
                    {
                        layerManager.RefreshSettings(isVisible, s, ex);
                    }
                    catch (Exception)
                    {
                    }
                }
                this.queryCompletionNotificationList.Clear();
            }
        }

        internal void PrepareData(bool zoomToData = false, CancellationTokenSource cancellationTokenSource = null, bool forceRequeryIfVisible = false, bool forciblyRefreshDisplay = false, LayerManager.Settings layerManagerSettings = null)
        {
            if (!this.initialized)
                throw new InvalidOperationException("Object has not been initialized.");
            CancellationToken cancellationToken = cancellationTokenSource == null ? CancellationToken.None : cancellationTokenSource.Token;
            FieldWellDefinition fieldWellDefinition = this.FieldWellDefinition;
            DataSource dataSource = this.DataSource;
            LayerDefinition layerDefinition = this.LayerDefinition;
            LayerManager layerManager = layerDefinition == null ? null : layerDefinition.LayerManager;
            bool flag = true;
            if (fieldWellDefinition == null || dataSource == null || layerManager == null)
                throw new OperationCanceledException("Operation was cancelled because the layer was shut down", cancellationToken);
            bool isVisible = this.Visible && layerDefinition.Visible;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (isVisible)
                {
                    lock (this.queryCheckLock)
                    {
                        int local_7;
                        if (fieldWellDefinition.GetFieldsChangedSinceLastQuery(out local_7))
                        {
                            dataSource.SetFieldsFromFieldWellDefinition(fieldWellDefinition, forceRequeryIfVisible);
                            fieldWellDefinition.ResetFieldsChangedSinceLastQuery(local_7);
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                    bool dataChanged = dataSource.RunQuery(cancellationToken, forceRequeryIfVisible, true, false);
                    dataSource.UpdateViews(cancellationToken, dataChanged, zoomToData, false);
                }
                flag = false;
                this.NotifyQueryCompletion(layerManager, isVisible, null);
            }
            catch (Exception ex)
            {
                if (flag)
                    this.NotifyQueryCompletion(layerManager, isVisible, ex);
                throw;
            }
        }

        public void RefreshDisplay(bool zoomToData, CancellationTokenSource cancellationTokenSource = null, bool forceRequeryIfVisible = false, bool forciblyRefreshDisplay = false, LayerManager.Settings layerManagerSettings = null)
        {
            if (!this.initialized)
            {
                throw new InvalidOperationException("Object has not been initialized.");
            }
            CancellationToken token = (cancellationTokenSource == null) ? CancellationToken.None : cancellationTokenSource.Token;
            LayerDefinition layerDefinition = this.LayerDefinition;
            LayerManager layerManager = (layerDefinition == null) ? null : layerDefinition.LayerManager;
            if (layerManager == null)
            {
                throw new OperationCanceledException("Operation was cancelled because the LayerDefinition was shut down", token);
            }
            bool flag = false;
            bool flag2 = true;
            object obj2 = null;
            bool visible = this.Visible && layerDefinition.Visible;
            try
            {
                token.ThrowIfCancellationRequested();
                FieldWellDefinition fieldWellDefinition = this.FieldWellDefinition;
                DataSource dataSource = this.DataSource;
                token.ThrowIfCancellationRequested();
                if ((fieldWellDefinition == null) || (dataSource == null))
                {
                    throw new OperationCanceledException("Operation was cancelled because the layer was shut down", token);
                }
                obj2 = this.OnBeginRefresh(visible, layerManagerSettings);
                flag = true;
                lock (this.queryCheckLock)
                {
                    int num;
                    if (fieldWellDefinition.GetFieldsChangedSinceLastQuery(out num))
                    {
                        dataSource.SetFieldsFromFieldWellDefinition(fieldWellDefinition, forceRequeryIfVisible && visible);
                        fieldWellDefinition.ResetFieldsChangedSinceLastQuery(num);
                        token.ThrowIfCancellationRequested();
                        if (dataSource.FieldsChangedSinceLastQuery && ((layerManagerSettings == null) || !((LayerManager.RefreshDisplaySettings)layerManagerSettings).IgnoreDisplayPropertiesUpdates))
                        {
                            this.DisplayPropertiesUpdated(true);
                        }
                    }
                }
                bool dataChanged = dataSource.RunQuery(token, forceRequeryIfVisible && visible, visible, forciblyRefreshDisplay);
                token.ThrowIfCancellationRequested();
                if (visible)
                {
                    this.Show(token);
                    dataSource.UpdateViews(token, dataChanged, zoomToData, true);
                }
                else
                {
                    this.Hide();
                }
                flag2 = false;
                this.NotifyQueryCompletion(layerManager, visible, null);
                flag = false;
                this.OnEndRefresh(visible, (dynamic)obj2, (dynamic)null);
                this.RefreshDisplayPending = !visible;
            }
            catch (Exception exception)
            {
                if (flag)
                {
                    this.OnEndRefresh(visible, (dynamic)obj2, exception);
                    flag = false;
                }
                else if (flag2)
                {
                    this.NotifyQueryCompletion(layerManager, visible, exception);
                }
                throw;
            }

        }

        public string ModelDataIdForId(InstanceId id, bool anyMeasure, bool anyCategoryValue)
        {
            DataSource dataSource = this.DataSource;
            if (dataSource == null)
                return null;
            return dataSource.ModelDataIdForId(id, anyMeasure, anyCategoryValue);
        }

        public string ModelDataIdForSeriesIndex(int seriesIndex)
        {
            DataSource dataSource = this.DataSource;
            if (dataSource == null)
                return null;
            return dataSource.ModelDataIdForSeriesIndex(seriesIndex);
        }

        internal void DisplayPropertiesUpdated(bool refreshDisplayRequired)
        {
            LayerDefinition layerDefinition = this.LayerDefinition;
            if (layerDefinition == null)
                return;
            layerDefinition.DisplayPropertiesUpdated(refreshDisplayRequired);
        }

        internal virtual void ModelMetadataChanged(ModelMetadata modelMetadata, List<string> tablesWithUpdatedData, ref bool requery, ref bool queryChanged)
        {
            FieldWellDefinition fieldWellDefinition = this.FieldWellDefinition;
            if (fieldWellDefinition == null)
                return;
            fieldWellDefinition.ModelMetadataChanged(modelMetadata, tablesWithUpdatedData, ref requery, ref queryChanged);
            if (!queryChanged)
                return;
            this.DisplayPropertiesUpdated(true);
        }

        protected void Initialize()
        {
            this.DataBinding = this.CreateDataBinding();
            this.initialized = true;
            this.DisplayPropertiesUpdated(true);
        }

        internal virtual void OnVisibleChanged(bool visible)
        {
        }

        protected virtual object OnBeginRefresh(bool visible, LayerManager.Settings layerManagerSettings)
        {
            return null;
        }

        protected virtual void OnEndRefresh(bool visible, object context, Exception ex)
        {
        }

        internal virtual void Removed()
        {
            this.DataBinding.Removed();
            this.Shutdown();
        }

        internal virtual void Shutdown()
        {
            if (this.DataSource != null)
            {
                this.DataSource.Shutdown();
                this.DataSource.DecrementReuseCount();
                this.DataSource = null;
            }
            this.DataBinding = null;
            this.LayerDefinition = null;
            if (this.FieldWellDefinition == null)
                return;
            this.FieldWellDefinition.Shutdown();
            this.FieldWellDefinition = null;
        }

        protected virtual void SnapState(SerializableVisualization state)
        {
            state.Visible = this.Visible;
        }

        protected virtual void SetStateTo(SerializableVisualization state)
        {
            this.Visible = state.Visible;
        }

        public abstract bool Remove();

        protected abstract DataBinding CreateDataBinding();

        protected abstract void Show(CancellationToken cancellationToken);

        protected abstract void Hide();

        [Serializable]
        public class SerializableVisualization
        {
            [XmlAttribute]
            public bool Visible;
        }
    }
}
