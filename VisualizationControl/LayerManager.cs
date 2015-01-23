using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
    public class LayerManager : PropertyChangedNotificationBase, ITourLayerManager
    {
        public static readonly double MinimumDataDimensionScaleValue = Math.Sqrt(0.01);
        public static readonly double MaximumDataDimensionScaleValue = Math.Sqrt(100.0);
        public static readonly double MinimumFixedDimensionScaleValue = Math.Sqrt(0.0004);
        public static readonly double MaximumFixedDimensionScaleValue = Math.Sqrt(100.0);
        private double dataDimensionScale = double.NaN;
        private double fixedDimensionScale = double.NaN;
        private SelectionStats selectionStats = new SelectionStats();
        private ObservableCollectionEx<DecoratorModel> _decorators = new ObservableCollectionEx<DecoratorModel>();
        private readonly StringBuilder layerContentBuilder = new StringBuilder(1048576);
        internal const double MinimumClampedDataDimensionScaleValue = 0.1;
        internal const double MaximumClampedDataDimensionScaleValue = 10.0;
        internal const double MinimumClampedFixedDimensionScaleValue = 0.02;
        internal const double MaximumClampedFixedDimensionScaleValue = 10.0;
        internal const double MinimumClampedFixedDimensionScaleValueForBubble = 0.22;
        private const double MinimumDataDimensionClampMultiplier = 0.1;
        private const double MaximumDataDimensionClampMultiplier = 10.0;
        private const double MinimumFixedDimensionClampMultiplier = 0.02;
        private const double MaximumFixedDimensionClampMultiplier = 10.0;
        public const double MinimumOpacityFactorValue = 0.0;
        public const double MaximumOpacityFactorValue = 1.0;
        public const double DataDimensionScaleDefault = 1.0;
        public const double FixedDimensionScaleDefault = 1.0;
        public const double OpacityFactorDefault = 1.0;
        private ObservableCollectionEx<LayerDefinition> layerDefinitions;
        private DateTime? playFrom;
        private DateTime? playTo;
        private object syncRoot;
        private IDataSourceFactory dataSourceFactory;

        public VisualizationModel Model { get; private set; }

        public SelectionStats SelectionStats
        {
            get
            {
                return this.selectionStats;
            }
        }

        public ObservableCollectionEx<LayerDefinition> LayerDefinitions
        {
            get
            {
                lock (this.syncRoot)
                    return this.layerDefinitions;
            }
        }

        public ObservableCollectionEx<DecoratorModel> Decorators
        {
            get
            {
                lock (this.syncRoot)
                    return this._decorators;
            }
            private set
            {
                lock (this.syncRoot)
                    this._decorators = value;
            }
        }

        public static string MinTimeProperty
        {
            get
            {
                return "MinTime";
            }
        }

        public virtual DateTime? MinTime
        {
            get
            {
                DateTime dateTime = DateTime.MaxValue;
                bool flag = true;
                lock (this.syncRoot)
                {
                    foreach (LayerDefinition item_0 in (Collection<LayerDefinition>)this.layerDefinitions)
                    {
                        DateTime? local_3 = item_0.PlayFromTime;
                        if (local_3.HasValue && local_3.Value < dateTime)
                        {
                            dateTime = local_3.Value;
                            flag = false;
                        }
                    }
                }
                if (!flag)
                    return new DateTime?(dateTime);
                else
                    return new DateTime?();
            }
        }

        public static string MaxTimeProperty
        {
            get
            {
                return "MaxTime";
            }
        }

        public virtual DateTime? MaxTime
        {
            get
            {
                DateTime dateTime = DateTime.MinValue;
                bool flag = true;
                lock (this.syncRoot)
                {
                    foreach (LayerDefinition item_0 in (Collection<LayerDefinition>)this.layerDefinitions)
                    {
                        DateTime? local_3 = item_0.PlayToTime;
                        if (local_3.HasValue && local_3.Value > dateTime)
                        {
                            dateTime = local_3.Value;
                            flag = false;
                        }
                    }
                }
                if (!flag)
                    return new DateTime?(dateTime);
                else
                    return new DateTime?();
            }
        }

        public static string DataDimensionScaleProperty
        {
            get
            {
                return "DataDimensionScale";
            }
        }

        public double DataDimensionScale
        {
            get
            {
                double d = this.dataDimensionScale;
                if (!double.IsNaN(d))
                    return d;
                else
                    return 1.0;
            }
            set
            {
                if (double.IsNaN(value))
                {
                    value = double.NaN;
                }
                else
                {
                    value = Math.Min(value, LayerManager.MaximumDataDimensionScaleValue);
                    value = Math.Max(value, LayerManager.MinimumDataDimensionScaleValue);
                }
                this.dataDimensionScale = value;
                this.SetDataDimensionScale();
                this.RaisePropertyChanged(LayerManager.DataDimensionScaleProperty);
                this.NotifyStateChanged((object)this);
            }
        }

        public static string FixedDimensionScaleProperty
        {
            get
            {
                return "FixedDimensionScale";
            }
        }

        public double FixedDimensionScale
        {
            get
            {
                double d = this.fixedDimensionScale;
                if (!double.IsNaN(d))
                    return d;
                else
                    return 1.0;
            }
            set
            {
                if (double.IsNaN(value))
                {
                    value = double.NaN;
                }
                else
                {
                    value = Math.Min(value, LayerManager.MaximumFixedDimensionScaleValue);
                    value = Math.Max(value, LayerManager.MinimumFixedDimensionScaleValue);
                }
                this.fixedDimensionScale = value;
                this.SetFixedDimensionScale();
                this.RaisePropertyChanged(LayerManager.FixedDimensionScaleProperty);
                this.NotifyStateChanged((object)this);
            }
        }

        public static string PlayFromTimeProperty
        {
            get
            {
                return "PlayFromTime";
            }
        }

        public virtual DateTime? PlayFromTime
        {
            get
            {
                if (this.playFrom.HasValue)
                    return this.playFrom;
                else
                    return this.MinTime;
            }
            set
            {
                this.playFrom = value;
                this.RaisePropertyChanged(LayerManager.PlayFromTimeProperty);
                this.NotifyStateChanged((object)this);
            }
        }

        public static string PlayToTimeProperty
        {
            get
            {
                return "PlayToTime";
            }
        }

        public virtual DateTime? PlayToTime
        {
            get
            {
                if (this.playTo.HasValue)
                    return this.playTo;
                else
                    return this.MaxTime;
            }
            set
            {
                this.playTo = value;
                this.RaisePropertyChanged(LayerManager.PlayToTimeProperty);
                this.NotifyStateChanged((object)this);
            }
        }

        internal Dispatcher QueryEngineDispatcher
        {
            get
            {
                return this.Model.QueryEngineDispatcher;
            }
        }

        public event Action StateChanged;

        public LayerManager(VisualizationModel model, IDataSourceFactory dataSourceFactory)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (dataSourceFactory == null)
                throw new ArgumentNullException("dataSourceFactory");
            this.Model = model;
            this.dataSourceFactory = dataSourceFactory;
            this.layerDefinitions = new ObservableCollectionEx<LayerDefinition>();
            this.syncRoot = new object();
        }

        public string CreateDefaultSceneLayerContent()
        {
            return this.WriteSceneContentToString(true);
        }

        public string GetSceneLayersContent()
        {
            return this.WriteSceneContentToString(false);
        }

        public string WriteSceneContentToString(bool shouldDropLayers)
        {
            this.layerContentBuilder.Clear();
            XmlWriter xmlWriter = XmlWriter.Create(this.layerContentBuilder);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(LayerManager.SerializableLayerManager), "http://microsoft.data.visualization.geo3d/1.0");
            try
            {
                lock (this.syncRoot)
                {
                    LayerManager.SerializableLayerManager local_2 = new LayerManager.SerializableLayerManager()
                    {
                        LayerDefinitions = Enumerable.ToList<LayerDefinition.SerializableLayerDefinition>(Enumerable.Where<LayerDefinition.SerializableLayerDefinition>(Enumerable.Select<LayerDefinition, LayerDefinition.SerializableLayerDefinition>((IEnumerable<LayerDefinition>)this.layerDefinitions, (Func<LayerDefinition, LayerDefinition.SerializableLayerDefinition>)(layer => layer.Wrap())), (Func<LayerDefinition.SerializableLayerDefinition, bool>)(k => !shouldDropLayers))),
                        Decorators = Enumerable.ToList<DecoratorModel>(Enumerable.Where<DecoratorModel>((IEnumerable<DecoratorModel>)this._decorators, (Func<DecoratorModel, bool>)(k => !shouldDropLayers))),
                        PlayFromIsNull = !this.PlayFromTime.HasValue,
                        PlayFromTicks = this.PlayFromTime.HasValue ? this.PlayFromTime.Value.Ticks : DateTime.MinValue.Ticks,
                        PlayToIsNull = !this.PlayToTime.HasValue,
                        PlayToTicks = this.PlayToTime.HasValue ? this.PlayToTime.Value.Ticks : DateTime.MinValue.Ticks,
                        DataDimensionScale = this.dataDimensionScale,
                        FixedDimensionScale = this.fixedDimensionScale
                    };
                    xmlSerializer.Serialize(xmlWriter, (object)local_2);
                }
            }
            catch (Exception ex)
            {
                VisualizationTraceSource.Current.Fail("Exception while serializing layer definitions.", ex);
                throw;
            }
            finally
            {
                xmlWriter.Close();
            }
            string str = ((object)this.layerContentBuilder).ToString();
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "GetSceneLayersContent() serialized LayerManager.layerDefinitions to xml: {0}", (object)str);
            return str;
        }

        public void SetSceneLayersContent(string layersContent, Guid customMapSpaceId, Action<object, Exception> completedCallback = null, object completionContext = null)
        {
            this.SetSceneLayers(this.PrepareSceneLayers(layersContent, customMapSpaceId, false, (Action<object, Exception>)null, (object)null), completedCallback, completionContext);
        }

        public object PrepareSceneLayers(string layersContent, Guid customMapSpaceId, Action<object, Exception> completedCallback = null, object completionContext = null)
        {
            return this.PrepareSceneLayers(layersContent, customMapSpaceId, true, completedCallback, completionContext);
        }

        private object PrepareSceneLayers(string layersContent, Guid customMapSpaceId, bool prepareData,
            Action<object, Exception> completedCallback = null, object completionContext = null)
        {
            object result;
            if (string.IsNullOrEmpty(layersContent))
            {
                throw new ArgumentException("layersContent is null or empty");
            }
            Guid cookie = Guid.NewGuid();
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableLayerManager));
            StringReader textReader = new StringReader(layersContent);
            try
            {
                SerializableLayerManager manager;
                int count;
                try
                {
                    manager = (SerializableLayerManager)serializer.Deserialize(textReader);
                }
                catch (Exception exception)
                {
                    VisualizationTraceSource.Current.Fail("Exception while deserializing a scene ", exception);
                    throw new TourDeserializationException("Exception while deserializing a scene ", exception);
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                    "PrepareSceneLayers(): cookie={0}, new layer content : {1}", new object[] { cookie, layersContent });

                Func<LayerDefinition.SerializableLayerDefinition, LayerDefinition> selector = serializedLayer =>
                    serializedLayer.Unwrap(
                        this,
                        this.LayerDefinitions.FirstOrDefault<LayerDefinition>(
                            layer => layer.Key == serializedLayer.Key),
                        this.Model.GetModelMetadata().Culture);
                List<LayerDefinition> newLayerDefinitions =
                    manager.LayerDefinitions.Select<LayerDefinition.SerializableLayerDefinition, LayerDefinition>(selector)
                        .ToList<LayerDefinition>();

                newLayerDefinitions.ForEach((Action<LayerDefinition>)(layer =>
                {
                    layer.DataLoadedCustomSpaceId = customMapSpaceId;
                    if (layer.GeoVisualization == null)
                    {
                        throw new TourDeserializationException("Null geovisualization for a layer in the scene");
                    }
                }));
                List<LayerDefinition> list = new List<LayerDefinition>();
                LayerDefinition[] array = null;
                lock (this.syncRoot)
                {
                    Action<LayerDefinition> action = null;
                    count = this.LayerDefinitions.Count;
                    array = new LayerDefinition[count];
                    Array.Clear(array, 0, array.Length);
                    for (int i = 0; i < newLayerDefinitions.Count; i++)
                    {
                        int reusedLayerIndex = 0;
                        this.layerDefinitions.FirstOrDefault<LayerDefinition>(delegate(LayerDefinition layer)
                        {
                            if ((layer.Key == newLayerDefinitions[i].Key) &&
                                (layer.DataLoadedCustomSpaceId == customMapSpaceId))
                            {
                                return true;
                            }
                            reusedLayerIndex++;
                            return false;
                        });
                        if (reusedLayerIndex == count)
                        {
                            list.Add(newLayerDefinitions[i]);
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                                "PrepareSceneLayers(cookie={0}): Adding layerDefinition with id = {1}",
                                new object[] { cookie, newLayerDefinitions[i].Id });
                        }
                        else
                        {
                            array[reusedLayerIndex] = newLayerDefinitions[i];
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                                "PrepareSceneLayers(cookie={0}): Reusing layerDefinition with id = {1}",
                                new object[] { cookie, newLayerDefinitions[i].Id });
                        }
                    }
                    ModelMetadata modelMetadata = this.Model.GetModelMetadata();
                    bool requery = false;
                    bool queryChanged = false;
                    int newlyAddedLayerIndex = 0;
                    List<string> tablesChanged = new List<string>(0);
                    list.ForEach(delegate(LayerDefinition layer)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                            "PrepareSceneLayers(cookie={0}): Calling ModelMetadataChanged for layerDefinition with id = {1}",
                            new object[] { cookie, layer.Id });
                        layer.ModelMetadataChanged(modelMetadata, tablesChanged, ref requery, ref queryChanged);
                        if (layer.ForInstructionsOnly && (modelMetadata.TableIslands.Count > 0))
                        {
                            layer.ForInstructionsOnly = false;
                        }
                        newlyAddedLayerIndex++;
                    });
                    Settings settings = new Settings
                    {
                        TraceMessage = string.Format("{0} - PrepareSceneLayers", cookie),
                        InProgressQueryCount = 1,
                        QueryCompletedCallback = completedCallback,
                        CompletionContext = completionContext
                    };
                    if (prepareData)
                    {
                        newlyAddedLayerIndex = 0;
                        if (action == null)
                        {
                            action = delegate(LayerDefinition layer)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                                    "PrepareSceneLayers(cookie={0}): Calling PrepareData for layerDefinition with id = {1}",
                                    new object[] { cookie, layer.Id });
                                layer.PrepareData(cookie, settings, false);
                                newlyAddedLayerIndex++;
                            };
                        }
                        list.ForEach(action);
                        if (newlyAddedLayerIndex == 0)
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                                "PrepareSceneLayers(cookie={0}): no newly added layers to call PrepareData on",
                                new object[] { cookie });
                        }
                        for (int j = count - 1; j >= 0; j--)
                        {
                            if (array[j] != null)
                            {
                                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                                    "PrepareSceneLayers(cookie={0}): Calling PrepareData for reused layerDefinition with id = {1}",
                                    new object[] { cookie, this.layerDefinitions[j].Id });
                                this.layerDefinitions[j].PrepareData(cookie, settings, true);
                            }
                        }
                    }
                    else
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                            "PrepareSceneLayers(cookie={0}): prepareData = false, skipping call to PrepareData",
                            new object[] { cookie });
                    }
                    if (Interlocked.Decrement(ref settings.InProgressQueryCount) == 0)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                            "PrepareSceneLayers(cookie={0}): data for all layers has been prepared, calling RefreshSettings",
                            new object[] { cookie });
                        this.RefreshSettings(settings, null);
                    }
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                    "PrepareSceneLayers() returning -  cookie={0}", new object[] { cookie });
                PrepareSceneLayersContext context = new PrepareSceneLayersContext
                {
                    serializableLayerDefinitions = manager,
                    newlyAddedLayers = list,
                    reusedLayers = array,
                    layerDefinitionCount = count,
                    cookie = cookie
                };
                result = context;
            }
            catch (Exception exception2)
            {
                VisualizationTraceSource.Current.Fail(
                    "Exception while loading PrepareSceneLayers, cookie=" + cookie.ToString(), exception2);
                throw;
            }
            finally
            {
                textReader.Close();
            }
            return result;
        }

        public void SetSceneLayers(object preparedContext, Action<object, Exception> completedCallback = null,
            object completionContext = null)
        {
            PrepareSceneLayersContext context = preparedContext as PrepareSceneLayersContext;
            if (context == null)
            {
                throw new ArgumentException("prepareContext is null or not of type PrepareSceneLayersContext");
            }
            Guid cookie = context.cookie;
            try
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "SetSceneLayers() cookie={0}",
                    new object[] { cookie });
                SerializableLayerManager serializableLayerDefinitions = context.serializableLayerDefinitions;
                List<LayerDefinition> newlyAddedLayers = context.newlyAddedLayers;
                LayerDefinition[] reusedLayers = context.reusedLayers;
                lock (this.syncRoot)
                {
                    Action visibilityChangedCallback = null;
                    int count = this.LayerDefinitions.Count;
                    if (count != context.layerDefinitionCount)
                    {
                        throw new InvalidOperationException("context.layerDefinitionCount=" +
                                                            context.layerDefinitionCount.ToString() +
                                                            " != current layerDefinitionCount=" + count.ToString());
                    }
                    for (int i = 0; i < count; i++)
                    {
                        if ((reusedLayers[i] != null) && (this.layerDefinitions[i].Key != reusedLayers[i].Key))
                        {
                            throw new InvalidOperationException("reusedLayers[" + i.ToString() + "].Key=" +
                                                                reusedLayers[i].Key.ToString() +
                                                                " != this.layerDefinitions[" + i.ToString() + "].Key=" +
                                                                this.layerDefinitions[i].Key.ToString());
                        }
                    }
                    foreach (DecoratorModel model in this.Decorators.Clone())
                    {
                        if (!(model.Content is TimeDecoratorModel))
                        {
                            this.Decorators.Remove(model);
                        }
                    }
                    this.ResetPlayTimes();
                    DateTime? playFrom = serializableLayerDefinitions.PlayFromIsNull
                        ? null
                        : ((DateTime?)new DateTime(serializableLayerDefinitions.PlayFromTicks));
                    DateTime? playTo = serializableLayerDefinitions.PlayToIsNull
                        ? null
                        : ((DateTime?)new DateTime(serializableLayerDefinitions.PlayToTicks));
                    Settings settings = new RefreshDisplaySettings
                    {
                        TraceMessage = string.Format("{0} - SetSceneLayers", cookie),
                        IgnoreDisplayPropertiesUpdates = true,
                        PlayFrom = playFrom,
                        PlayTo = playTo,
                        InProgressQueryCount = newlyAddedLayers.Count + 1,
                        QueryCompletedCallback = completedCallback,
                        CompletionContext = completionContext
                    };
                    for (int j = count - 1; j >= 0; j--)
                    {
                        if (reusedLayers[j] == null)
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                                "SetSceneLayers(cookie={0}): Removing layerDefinition with id = {1}",
                                new object[] { cookie, this.layerDefinitions[j].Id });
                            this.RemoveLayerDefinition(this.layerDefinitions[j]);
                        }
                        else
                        {
                            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                                "SetSceneLayers(cookie={0}): Reusing layerDefinition with id = {1}, updating layer's display properties",
                                new object[] { cookie, this.layerDefinitions[j].Id });
                            if (visibilityChangedCallback == null)
                            {
                                visibilityChangedCallback = () => Interlocked.Increment(ref settings.InProgressQueryCount);
                            }
                            this.layerDefinitions[j].UpdateDisplayProperties(reusedLayers[j], settings,
                                visibilityChangedCallback);
                        }
                    }
                    LayerDefinition[] definitionArray2 = this.layerDefinitions.ToArray<LayerDefinition>();
                    this.LayerDefinitions.RemoveAll();
                    for (int k = 0; k < definitionArray2.Length; k++)
                    {
                        this.layerDefinitions.Add(definitionArray2[k]);
                    }

                    newlyAddedLayers.ForEach((Action<LayerDefinition>)(layer =>
                    {
                        layer.DisallowIncrementRevisionCount();
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                            "SetSceneLayers(cookie={0}): Adding layerDefinition with id = {1}",
                            new object[] { cookie, layer.Id });
                        this.LayerDefinitions.Add(layer);
                        layer.AllowIncrementRevisionCount(true);
                    }));
                    this.Model.ColorSelector.SetColorIndex(
                        this.layerDefinitions.SelectMany<LayerDefinition, int>(delegate(LayerDefinition layer)
                        {
                            if ((layer.GeoVisualization != null) && (layer.GeoVisualization.ColorIndices != null))
                            {
                                return layer.GeoVisualization.ColorIndices;
                            }
                            return Enumerable.Empty<int>();
                        }));
                    this.DataDimensionScale = serializableLayerDefinitions.DataDimensionScale;
                    this.FixedDimensionScale = serializableLayerDefinitions.FixedDimensionScale;
                    this.Decorators.RemoveAll();

                    serializableLayerDefinitions.Decorators.ForEach(
                        (Action<DecoratorModel>)(decorator =>
                            this.Decorators.Add(decorator)));
                    int newlyAddedLayerIndex = 0;
                    newlyAddedLayers.ForEach(delegate(LayerDefinition layer)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                            "SetSceneLayers(cookie={0}): Calling RefreshDisplay for layerDefinition with id = {1}",
                            new object[] { cookie, layer.Id });
                        layer.RefreshDisplay(false, null, false, false, settings, null, null);
                        newlyAddedLayerIndex++;
                    });
                    if (Interlocked.Decrement(ref settings.InProgressQueryCount) == 0)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                            "SetSceneLayers(cookie={0}): layers to be queried count = 0, calling RefreshSettings",
                            new object[] { cookie });
                        this.RefreshSettings(settings, null);
                    }
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0,
                    "SetSceneLayers() replaced LayerManager.layerDefinitions with {0} layers, cookie={1}",
                    new object[] { serializableLayerDefinitions.LayerDefinitions.Count, cookie });
            }
            catch (Exception exception)
            {
                VisualizationTraceSource.Current.Fail("Exception in SetSceneLayers, cookie=" + cookie.ToString(),
                    exception);
                throw;
            }
        }

        public DataSource CreateDataSource(string name)
        {
            DataSource dataSource = this.dataSourceFactory.CreateDataSource(name);
            dataSource.ModelCulture = this.Model.GetModelMetadata().Culture;
            return dataSource;
        }

        public LayerDefinition AddLayerDefinition(string name = null, string modelTableName = null, bool createGeoVisualization = true, bool forInstructionsOnly = false)
        {
            if (name == null)
            {
                lock (this.syncRoot)
                {
                    int local_0 = Enumerable.Count<LayerDefinition>((IEnumerable<LayerDefinition>)this.layerDefinitions) + 1;
                    name = string.Format(Resources.LayerDefaultName, (object)local_0.ToString());
                    while (Enumerable.Any<LayerDefinition>((IEnumerable<LayerDefinition>)this.layerDefinitions, (Func<LayerDefinition, bool>)(layer => layer.Name.ToLower() == name.ToLower())))
                    {
                        ++local_0;
                        name = string.Format(Resources.LayerDefaultName, (object)local_0.ToString());
                    }
                }
            }
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "AddLayerDefinition(): Adding layer '{0}'", (object)name);
            LayerDefinition layerDefinition = new LayerDefinition(this, name, modelTableName, forInstructionsOnly);
            if (createGeoVisualization)
                layerDefinition.AddGeoVisualization((DataSource)null);
            lock (this.syncRoot)
                this.layerDefinitions.Add(layerDefinition);
            this.ResetPlayTimes();
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "AddLayerDefinition(): Added layer '{0}', id={1}", (object)layerDefinition.Name, (object)layerDefinition.Id);
            return layerDefinition;
        }

        public bool RemoveLayerDefinition(LayerDefinition layerDefinition)
        {
            if (layerDefinition == null)
                throw new ArgumentNullException("layerDefinition");
            bool flag;
            lock (this.syncRoot)
                flag = this.layerDefinitions.Remove(layerDefinition);
            if (flag)
            {
                Guid id = layerDefinition.Id;
                string name = layerDefinition.Name;
                layerDefinition.Removed();
                this.ResetPlayTimes();
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "RemoveLayerDefinition(): Removed layer '{0}', id = {1}", (object)name, (object)id);
            }
            return flag;
        }

        public bool RemoveAllLayerDefinitions()
        {
            bool removed = false;
            lock (this.syncRoot)
            {
                List<LayerDefinition> source = this.layerDefinitions.ToList<LayerDefinition>();

                source.ForEach((Action<LayerDefinition>)(layer =>
                {
                    removed |= this.RemoveLayerDefinition(layer);
                }));
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Information, 0, "RemoveAllLayerDefinitions: removed {0} layers", new object[] { source.Count<LayerDefinition>() });
            }
            return removed;

        }

        public void ForciblyRefreshDisplay(bool isZoomToData = false)
        {
            lock (this.syncRoot)
            {
                foreach (LayerDefinition definition in this.layerDefinitions)
                {
                    definition.RefreshDisplay(isZoomToData, null, false, true, null, null, null);
                }
            }

        }

        public IEnumerable<TableIsland> GetTableIslands()
        {
            return (IEnumerable<TableIsland>)this.Model.GetModelMetadata().TableIslands;
        }

        internal void Reset()
        {
            this._decorators.RemoveAll();
            this.RemoveAllLayerDefinitions();
            this.ResetPlayTimes();
            this.dataDimensionScale = double.NaN;
            this.fixedDimensionScale = double.NaN;
            this.SelectionStats.Clear();
        }

        internal void ResetPlayTimes()
        {
            this.playFrom = new DateTime?();
            this.playTo = new DateTime?();
            this.NotifyPlayTimePropertyChanges();
        }

        internal void RefreshSettings(bool layerVisible, LayerManager.Settings settings, Exception ex)
        {
            if (settings == null)
                return;
            if (Interlocked.Decrement(ref settings.InProgressQueryCount) == 0)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.RefreshSettings(cookie={0}): InProgressQueryCount=0", (object)settings.TraceMessage);
                this.RefreshSettings(settings, ex);
            }
            else
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.RefreshSettings(cookie={0}): InProgressQueryCount={1} (after decrement)", (object)settings.TraceMessage, (object)settings.InProgressQueryCount);
        }

        internal void SelectionStatsChanged()
        {
            this.SelectionStats.Clear();
            foreach (LayerDefinition layerDefinition in (Collection<LayerDefinition>)this.layerDefinitions)
            {
                GeoVisualization geoVisualization = layerDefinition.GeoVisualization;
                if (geoVisualization != null)
                    this.SelectionStats.UpdateWithSelectionStats(geoVisualization.SelectionStats);
            }
        }

        internal void ModelChanged(List<string> tablesWithUpdatedData)
        {
            lock (this.syncRoot)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.ModelChanged(): starting");
                if (this.Model.Engine.TimeControl.VisualTimeEnabled)
                {
                    this.Model.Engine.TimeControl.VisualTimeEnabled = false;
                    if (this.MaxTime.HasValue)
                        this.Model.Engine.TimeControl.CurrentVisualTime = this.MaxTime.Value;
                }
                List<LayerDefinition> local_0 = new List<LayerDefinition>((IEnumerable<LayerDefinition>)this.LayerDefinitions);
                ObservableCollectionEx<DecoratorModel> local_1 = this.Decorators.Clone();
                this.LayerDefinitions.RemoveAll();
                ModelMetadata modelMetadata = this.Model.GetModelMetadata();
                bool[] requery = new bool[local_0.Count];
                bool[] queryChanged = new bool[local_0.Count];
                Array.Clear((Array)requery, 0, requery.Length);
                Array.Clear((Array)queryChanged, 0, queryChanged.Length);
                int layerIndex = 0;
                local_0.ForEach((Action<LayerDefinition>)(layer =>
                {
                    GeoVisualization geoVisualization = layer.GeoVisualization;
                    if (geoVisualization != null && geoVisualization.DataSource != null)
                        geoVisualization.DataSource.ModelCulture = modelMetadata.Culture;
                    if (!layer.ForInstructionsOnly)
                    {
                        layer.ModelMetadataChanged(modelMetadata, tablesWithUpdatedData, ref requery[layerIndex], ref queryChanged[layerIndex]);
                        if (requery[layerIndex])
                            layer.RefreshDisplay(false, (CancellationTokenSource)null, true, false, (LayerManager.Settings)null, (Action<object, bool, bool, Exception>)null, (object)null);
                        this.layerDefinitions.Add(layer);
                    }
                    else
                        layer.Removed();
                    ++layerIndex;
                }));
                foreach (DecoratorModel item_0 in (Collection<DecoratorModel>)local_1)
                {
                    if (!this.Decorators.Contains(item_0))
                        this.Decorators.Add(item_0);
                }
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.ModelChanged() completed");
            }
        }

        internal void NotifyStateChanged(object sender)
        {
            if (this.StateChanged == null)
                return;
            this.StateChanged();
        }

        private void NotifyPlayTimePropertyChanges()
        {
            this.RaisePropertyChanged(LayerManager.PlayFromTimeProperty);
            this.RaisePropertyChanged(LayerManager.PlayToTimeProperty);
            this.RaisePropertyChanged(LayerManager.MinTimeProperty);
            this.RaisePropertyChanged(LayerManager.MaxTimeProperty);
        }

        private void RefreshSettings(LayerManager.Settings settings, Exception ex)
        {
            LayerManager.RefreshDisplaySettings refreshDisplaySettings = settings as LayerManager.RefreshDisplaySettings;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.RefreshSettings(cookie={0}): ex is{1} null", (object)settings.TraceMessage, ex == null ? (object)string.Empty : (object)" not");
            if (refreshDisplaySettings != null)
            {
                VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.RefreshSettings(cookie={0}): setting from/to play time", (object)settings.TraceMessage);
                DateTime? minTime = this.MinTime;
                DateTime? maxTime = this.MaxTime;
                if (minTime.HasValue && maxTime.HasValue && (refreshDisplaySettings.PlayFrom.HasValue && refreshDisplaySettings.PlayFrom.Value >= minTime.Value) && refreshDisplaySettings.PlayFrom.Value <= maxTime.Value)
                {
                    this.playFrom = refreshDisplaySettings.PlayFrom;
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.RefreshSettings(cookie={0}): set playFrom to {1}", (object)settings.TraceMessage, (object)refreshDisplaySettings.PlayFrom);
                }
                else
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.RefreshSettings(cookie={0}): did not change playFrom={1}", (object)settings.TraceMessage, (object)this.playFrom);
                if (minTime.HasValue && maxTime.HasValue && (refreshDisplaySettings.PlayTo.HasValue && refreshDisplaySettings.PlayTo.Value >= minTime.Value) && refreshDisplaySettings.PlayTo.Value <= maxTime.Value)
                {
                    this.playTo = refreshDisplaySettings.PlayTo;
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.RefreshSettings(cookie={0}): set playTo to {1}", (object)settings.TraceMessage, (object)refreshDisplaySettings.PlayTo);
                }
                else
                    VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.RefreshSettings(cookie={0}): did not change playTo={1}", (object)settings.TraceMessage, (object)this.playTo);
                this.NotifyPlayTimePropertyChanges();
            }
            if (settings.QueryCompletedCallback == null)
                return;
            VisualizationTraceSource.Current.TraceEvent(TraceEventType.Verbose, 0, "LayerManager.RefreshSettings(cookie={0}): calling query callback", (object)settings.TraceMessage);
            settings.QueryCompletedCallback(settings.CompletionContext, ex);
        }

        private void SetDataDimensionScale()
        {
            lock (this.syncRoot)
            {
                foreach (LayerDefinition item_0 in (Collection<LayerDefinition>)this.layerDefinitions)
                {
                    if (item_0.GeoVisualization != null)
                        item_0.GeoVisualization.SetDataDimensionScale();
                }
            }
        }

        private void SetFixedDimensionScale()
        {
            lock (this.syncRoot)
            {
                foreach (LayerDefinition item_0 in (Collection<LayerDefinition>)this.layerDefinitions)
                {
                    if (item_0.GeoVisualization != null)
                        item_0.GeoVisualization.SetFixedDimensionScale();
                }
            }
        }

        [XmlRoot("SerializedLayerManager", IsNullable = false, Namespace = "http://microsoft.data.visualization.geo3d/1.0")]
        [Serializable]
        public class SerializableLayerManager
        {
            [XmlArray("LayerDefinitions")]
            [XmlArrayItem("LayerDefinition", typeof(LayerDefinition.SerializableLayerDefinition))]
            public List<LayerDefinition.SerializableLayerDefinition> LayerDefinitions;
            [XmlArray("Decorators")]
            [XmlArrayItem("Decorator", typeof(DecoratorModel))]
            public List<DecoratorModel> Decorators;

            [XmlAttribute]
            public bool PlayFromIsNull { get; set; }

            [XmlAttribute]
            public long PlayFromTicks { get; set; }

            [XmlAttribute]
            public bool PlayToIsNull { get; set; }

            [XmlAttribute]
            public long PlayToTicks { get; set; }

            [XmlAttribute("DataScale")]
            public double DataDimensionScale { get; set; }

            [XmlAttribute("DimnScale")]
            public double FixedDimensionScale { get; set; }
        }

        public class Settings
        {
            public string TraceMessage;
            public int InProgressQueryCount;
            public Action<object, Exception> QueryCompletedCallback;
            public object CompletionContext;
        }

        public class RefreshDisplaySettings : LayerManager.Settings
        {
            public bool IgnoreDisplayPropertiesUpdates;
            public DateTime? PlayFrom;
            public DateTime? PlayTo;
        }

        private class PrepareSceneLayersContext
        {
            public int layerDefinitionCount;
            public LayerManager.SerializableLayerManager serializableLayerDefinitions;
            public List<LayerDefinition> newlyAddedLayers;
            public LayerDefinition[] reusedLayers;
            public Guid cookie;
        }
    }
}
