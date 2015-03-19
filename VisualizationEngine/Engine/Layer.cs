using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
    public abstract class Layer
    {
        private LayerScaling scaling = new LayerScaling();
        private LayerTimeScaling timeScaling = new LayerTimeScaling();
        private float opacity = 1f;
        private float dataDimensionScale;
        private float fixedDimensionScale;
        protected bool incrementalDataUpdate;

        protected double MaxAbsValue { get; set; }

        public abstract int DataCount { get; }

        internal LayerColorManager ColorManager { get; set; }

        public float DataDimensionScale
        {
            get
            {
                return this.dataDimensionScale;
            }
            set
            {
                if (value == this.dataDimensionScale)
                    return;
                if (this.EngineDispatcher != null)
                {
                    this.EngineDispatcher.RunOnRenderThread(() =>
                    {
                        this.IsDirty = true;
                        this.dataDimensionScale = value;
                    });
                }
                else
                {
                    this.IsDirty = true;
                    this.dataDimensionScale = value;
                }
            }
        }

        public float Opacity
        {
            get
            {
                return this.opacity;
            }
            set
            {
                this.opacity = Math.Max(0.0f, Math.Min(1f, value));
                this.IsDirty = true;
            }
        }

        public float FixedDimensionScale
        {
            get
            {
                return this.fixedDimensionScale;
            }
            set
            {
                if (value == this.fixedDimensionScale)
                    return;
                if (this.EngineDispatcher != null)
                {
                    this.EngineDispatcher.RunOnRenderThread(() =>
                    {
                        this.IsDirty = true;
                        this.fixedDimensionScale = value;
                    });
                }
                else
                {
                    this.IsDirty = true;
                    this.fixedDimensionScale = value;
                }
            }
        }

        public virtual LayerType LayerType { get; protected set; }

        internal bool IsOverlay { get; set; }

        internal bool IsDirty { get; set; }

        internal IVisualizationEngineDispatcher EngineDispatcher { get; set; }

        internal Dispatcher EventDispatcher { get; set; }

        public double MinValue { get; private set; }

        public double MaxValue { get; private set; }

        public double ViewScale
        {
            get
            {
                return this.scaling.ViewScale;
            }
        }

        public bool DataInputInProgress { get; protected set; }

        protected CustomSpaceTransform DataInputCustomSpace { get; set; }

        public virtual bool DisplayNullValues
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public virtual bool DisplayZeroValues
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public virtual bool DisplayNegativeValues
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        internal LayerScaling Scaling
        {
            get
            {
                return this.scaling;
            }
        }

        internal LayerTimeScaling TimeScaling
        {
            get
            {
                return this.timeScaling;
            }
        }

        protected bool UseLogarithmicClampedValue { get; set; }

        internal Layer()
        {
            this.DataDimensionScale = 1f;
            this.FixedDimensionScale = 1f;
        }

        public abstract void EraseAllData();

        internal virtual void Update(SceneState state)
        {
        }

        public abstract void BeginDataInput(int estimate, bool progressiveDataInput, bool ignoreData, CustomSpaceTransform dataCustomSpace, double minInstanceValue, double maxInstanceValue, DateTime? minTime, DateTime? maxTime);

        public abstract void AddData(double latitude, double longitude, IEnumerable<IInstanceParameter> parameters);

        public abstract void EndDataInput();

        public abstract Vector3D GetDataPosition(int bufferPosition, bool flatMap);

        public abstract Coordinates GetDataLocation(int bufferPosition);

        internal virtual void PreDraw(Renderer renderer, SceneState state, LayerRenderingParameters options)
        {
        }

        internal abstract void Draw(Renderer renderer, SceneState state, LayerRenderingParameters options);

        internal abstract void ResetGraphicsData();

        internal virtual void OnAddLayer()
        {
        }

        public virtual bool SetColor(int shift, Color4F color)
        {
            return false;
        }

        public virtual bool ResetColor(int shift)
        {
            return false;
        }

        public virtual bool ResetAllColors()
        {
            return false;
        }

        public bool CanSetLayerType(LayerType type)
        {
            return this.LayerType == type || this.LayerType != LayerType.HeatMapChart && this.LayerType != LayerType.RegionChart && (type != LayerType.HeatMapChart && type != LayerType.RegionChart);
        }

        public bool TrySetLayerType(LayerType type)
        {
            if (!this.CanSetLayerType(type))
                return false;
            this.LayerType = type;
            return true;
        }

        public void LockViewScale(double scale)
        {
            this.scaling.LockViewScale(scale);
        }

        public void UnlockViewScale()
        {
            this.scaling.UnlockViewScale();
            this.IsDirty = true;
        }

        protected Coordinates CoordinatesFromLongLatDegrees(double longitude, double latitude)
        {
            if (this.DataInputCustomSpace == null)
                return Coordinates.FromDegrees(longitude, latitude);
            double lat;
            double lon;
            this.DataInputCustomSpace.TransformSpacesDataToDegrees(latitude, longitude, out lat, out lon);
            return Coordinates.FromDegrees(lon, lat);
        }

        protected float GetLayerClampedValue(double value)
        {
            if (double.IsNaN(value))
                return float.NaN;
            if (this.MaxAbsValue != 0.0)
                value /= this.MaxAbsValue;
            if (this.UseLogarithmicClampedValue && value != 0.0)
                value = Math.Max(0.0, 0.0482549424336947 * Math.Log(Math.Abs(value)) + 1.0) * Math.Sign(value);
            if (Math.Abs(value) < 1E-06)
                value = 1E-06 * Math.Sign(value);
            return (float)value;
        }

        protected void UpdateMinMaxValues(double minInstanceValue, double maxInstanceValue)
        {
            this.MinValue = minInstanceValue;
            this.MaxValue = maxInstanceValue;
            if (double.IsNaN(minInstanceValue) || double.IsNaN(maxInstanceValue))
                this.MaxAbsValue = 1.0;
            else
                this.MaxAbsValue = Math.Max(Math.Abs(minInstanceValue), Math.Abs(maxInstanceValue));
        }

        public Cap GetDataEnvelope(bool flatMap)
        {
            if (this.DataCount < 1)
                return SphericalCap.Empty;
            List<Vector3D> locations = new List<Vector3D>(this.DataCount);
            for (int bufferPosition = 0; bufferPosition < this.DataCount; ++bufferPosition)
            {
                Vector3D dataPosition = this.GetDataPosition(bufferPosition, flatMap);
                if (dataPosition != Vector3D.Empty)
                    locations.Add(dataPosition);
            }
            Cap cap = Cap.Construct(flatMap);
            if (!cap.SetCenter(locations))
            {
                cap.IsWholeWorld = true;
                return cap;
            }
            cap.SetExtent(locations);
            return cap;
        }

        public virtual void Dispose()
        {
        }
    }
}
