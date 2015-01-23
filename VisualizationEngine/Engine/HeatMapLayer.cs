// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.HeatMapLayer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
    public class HeatMapLayer : Layer
    {
        private HeatMapVertex[] valueVertices = new HeatMapVertex[0];
        private object dataInputLock = new object();
        private double currentMinValue = double.MaxValue;
        private double currentMinPositiveValue = double.MaxValue;
        private double currentMaxValue = double.MinValue;
        private const float DefaultNoValue = 0.25f;
        private const float NoValueCircleOfInfluenceFactor = 0.3f;
        private HeatMapRenderer heatMapRenderer;
        private HeatMapTimeVertex[] timeVertices;
        private int startValueCount;
        private int valueCount;
        private bool addingData;
        private bool okayToAddData;
        private bool pendingData;
        private bool ignoreInputValue;
        private DateTime? currentMinTime;
        private DateTime? currentMaxTime;

        public float HeatmapMinValue { get; private set; }

        public float HeatmapMaxValue { get; private set; }

        public override int DataCount
        {
            get
            {
                return this.valueVertices.Length;
            }
        }

        public override bool DisplayNullValues
        {
            get
            {
                return this.heatMapRenderer.ShowNulls;
            }
            set
            {
                this.heatMapRenderer.ShowNulls = value;
                this.RaiseScaleChanged();
                this.IsDirty = true;
            }
        }

        public override bool DisplayNegativeValues
        {
            get
            {
                return this.heatMapRenderer.ShowNegatives;
            }
            set
            {
                this.heatMapRenderer.ShowNegatives = value;
                this.RaiseScaleChanged();
                this.IsDirty = true;
            }
        }

        public override bool DisplayZeroValues
        {
            get
            {
                return this.heatMapRenderer.ShowZeros;
            }
            set
            {
                this.heatMapRenderer.ShowZeros = value;
                this.RaiseScaleChanged();
                this.IsDirty = true;
            }
        }

        public event EventHandler<HeatMapScaleEventArgs> OnScaleChanged;

        public HeatMapLayer()
        {
            this.heatMapRenderer = new HeatMapRenderer(this.Scaling, this.TimeScaling);
            this.IsOverlay = true;
            this.LayerType = LayerType.HeatMapChart;
        }

        public override void BeginDataInput(int estimate, bool progressiveDataInput, bool ignoreData, CustomSpaceTransform dataCustomSpace, double minInstanceValue, double maxInstanceValue, DateTime? minTime, DateTime? maxTime)
        {
            this.DataInputInProgress = true;
            this.DataInputCustomSpace = dataCustomSpace;
            lock (this.dataInputLock)
            {
                this.incrementalDataUpdate = progressiveDataInput;
                if (!progressiveDataInput)
                    this.addingData = true;
                this.okayToAddData = true;
                this.ignoreInputValue = ignoreData;
                if (ignoreData)
                {
                    this.currentMinValue = 0.0;
                    this.currentMinPositiveValue = 0.0;
                    this.currentMaxValue = 1.0;
                }
            }
            this.currentMinTime = minTime;
            this.currentMaxTime = maxTime;
            this.UpdateMinMaxValues(minInstanceValue, maxInstanceValue);
            if (this.valueVertices == null || this.valueVertices.Length < estimate)
                this.valueVertices = new HeatMapVertex[estimate];
            if (this.currentMinTime.HasValue && this.currentMaxTime.HasValue)
            {
                if (this.timeVertices != null && this.timeVertices.Length >= estimate)
                    return;
                this.timeVertices = new HeatMapTimeVertex[estimate];
            }
            else
                this.timeVertices = (HeatMapTimeVertex[])null;
        }

        public override void AddData(double latitude, double longitude, IEnumerable<IInstanceParameter> parameters)
        {
            if (parameters == null || Enumerable.Count<IInstanceParameter>(parameters) == 0)
                return;
            foreach (IInstanceParameter instanceParameter in parameters)
            {
                Coordinates coordinates = this.CoordinatesFromLongLatDegrees(longitude, latitude);
                this.AddData(coordinates.Latitude, coordinates.Longitude, instanceParameter.RealNumberValue, instanceParameter.StartTime, instanceParameter.EndTime);
            }
            this.IsDirty = true;
        }

        private void AddData(double latitude, double longitude, double value, DateTime? startTime, DateTime? endTime)
        {
            if (this.ignoreInputValue)
                value = 0.25;
            else if (double.IsInfinity(value))
                return;
            float layerClampedValue = this.GetLayerClampedValue(value);
            if (!double.IsNaN(value))
            {
                this.currentMinValue = Math.Min(this.currentMinValue, (double)layerClampedValue);
                if ((double)layerClampedValue > 0.0)
                    this.currentMinPositiveValue = Math.Min(this.currentMinPositiveValue, (double)layerClampedValue);
                this.currentMaxValue = Math.Max(this.currentMaxValue, (double)layerClampedValue);
            }
            Vector3F position = (Vector3F)Coordinates.GeoTo3D(longitude, latitude);
            lock (this.dataInputLock)
            {
                if (!this.okayToAddData)
                    return;
                if (this.timeVertices != null)
                {
                    double local_2 = (this.currentMaxTime.Value - this.currentMinTime.Value).TotalMilliseconds;
                    this.timeVertices[this.valueCount] = new HeatMapTimeVertex(startTime.HasValue ? (local_2 == 0.0 ? 0.0f : (float)((startTime.Value - this.currentMinTime.Value).TotalMilliseconds / local_2)) : -1f, endTime.HasValue ? (local_2 == 0.0 ? 0.0f : (float)((endTime.Value - this.currentMinTime.Value).TotalMilliseconds / local_2)) : -1f);
                }
                this.valueVertices[this.valueCount++] = new HeatMapVertex(position, layerClampedValue);
                this.pendingData = true;
            }
        }

        public override void EndDataInput()
        {
            lock (this.dataInputLock)
            {
                this.incrementalDataUpdate = false;
                this.addingData = false;
                this.okayToAddData = false;
            }
            this.DataInputInProgress = false;
            this.DataInputCustomSpace = (CustomSpaceTransform)null;
        }

        public override void EraseAllData()
        {
            lock (this.dataInputLock)
            {
                this.startValueCount = 0;
                this.valueCount = 0;
                this.okayToAddData = false;
            }
            this.currentMinValue = double.MaxValue;
            this.currentMinPositiveValue = double.MaxValue;
            this.currentMaxValue = double.MinValue;
            this.IsDirty = true;
            this.DataInputInProgress = false;
            this.DataInputCustomSpace = (CustomSpaceTransform)null;
        }

        internal override void Update(SceneState state)
        {
            base.Update(state);
            lock (this.dataInputLock)
            {
                if (!this.addingData && this.pendingData)
                    this.IsDirty = true;
                HeatMapLayer temp_8 = this;
                int temp_13 = temp_8.IsDirty | this.heatMapRenderer.IsDirty ? 1 : 0;
                temp_8.IsDirty = temp_13 != 0;
            }
        }

        internal override void Draw(Renderer renderer, SceneState state, LayerRenderingParameters options)
        {
            lock (this.dataInputLock)
            {
                if (!this.addingData)
                {
                    if (this.pendingData)
                    {
                        this.pendingData = false;
                        this.heatMapRenderer.SetVertexData(this.valueVertices, this.timeVertices, this.startValueCount, this.valueCount, (float)this.currentMinValue, (float)this.currentMinPositiveValue, (float)this.currentMaxValue, this.currentMinTime, this.currentMaxTime, state);
                        this.startValueCount = this.valueCount;
                        this.RaiseScaleChanged();
                    }
                }
            }
            if (this.valueCount <= 0 || this.startValueCount <= 0)
                return;
            this.heatMapRenderer.CircleOfInfluence = options.HeatMapCircleOfInfluence * this.FixedDimensionScale;
            if (this.ignoreInputValue)
                this.heatMapRenderer.CircleOfInfluence *= 0.3f;
            this.heatMapRenderer.IsVariableCircleOfInfluence = options.HeatMapVariableCircleOfInfluence;
            this.heatMapRenderer.Alpha = options.HeatMapAlpha * this.Opacity;
            this.heatMapRenderer.MaxValueForAlpha = options.HeatMapMaxValueForAlpha;
            this.heatMapRenderer.BlendMode = options.HeatMapBlendMode;
            this.heatMapRenderer.GaussianBlurEnable = options.HeatMapGaussianBlurEnable;
            float num = Math.Max(0.0f, (float)(1.0 / (double)this.DataDimensionScale * (1.0 / (double)options.InstanceVariableScaleFactor)));
            if ((double)num != (double)this.heatMapRenderer.MaxHeatFactor)
            {
                this.heatMapRenderer.MaxHeatFactor = num;
                this.RaiseScaleChanged();
            }
            this.heatMapRenderer.Draw(renderer, state);
        }

        private void RaiseScaleChanged()
        {
            if (this.OnScaleChanged == null || this.EventDispatcher == null)
                return;
            Action action = delegate
            {
                if (this.OnScaleChanged == null)
                    return;
                double num1 = this.DisplayNegativeValues ? this.currentMinValue : (this.DisplayZeroValues || this.DisplayNullValues ? 0.0 : this.currentMinPositiveValue);
                double num2 = !this.DisplayNegativeValues ? this.currentMaxValue : 1.0;
                float num3 = (float)(num1 * (double)this.heatMapRenderer.MaxHeatFactor * num2);
                float num4 = (float)(this.currentMaxValue * (double)this.heatMapRenderer.MaxHeatFactor * num2);
                this.HeatmapMinValue = num3 * (float)this.MaxAbsValue;
                this.HeatmapMaxValue = num4 * (float)this.MaxAbsValue;
                this.OnScaleChanged((object)this, new HeatMapScaleEventArgs(this.HeatmapMinValue, this.HeatmapMaxValue));
            };
            this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        internal override void ResetGraphicsData()
        {
            this.heatMapRenderer.Dispose();
            this.heatMapRenderer = new HeatMapRenderer(this.Scaling, this.TimeScaling);
        }

        public override Vector3D GetDataPosition(int bufferPosition, bool flatMap)
        {
            try
            {
                Vector3D position = this.valueVertices[bufferPosition].Position.ToVector3D();
                if (flatMap)
                    position = Coordinates.UnitSphereToFlatMap(position);
                return position;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Vector3D.Empty;
            }
        }

        public override Coordinates GetDataLocation(int bufferPosition)
        {
            return Coordinates.World3DToGeo(this.valueVertices[bufferPosition].Position.ToVector3D());
        }

        public override void Dispose()
        {
            base.Dispose();
            if (this.EngineDispatcher == null)
            {
                if (this.heatMapRenderer == null)
                    return;
                this.heatMapRenderer.Dispose();
            }
            else
                this.EngineDispatcher.RunOnRenderThread((RenderThreadMethod)(() => this.heatMapRenderer.Dispose()));
        }
    }
}
