// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.HitTestableLayer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
    public abstract class HitTestableLayer : Layer, IHitTestable
    {
        private HashSet<InstanceId> selectedItems = new HashSet<InstanceId>();
        private List<InstanceId> newSelection = new List<InstanceId>();
        private object newSelectionSync = new object();
        private const int TransientHoveredElementFrameCount = 20;
        public const SelectionStyle DefaultSelectionStyle = SelectionStyle.Outline;
        private int layerId;
        private IInstanceIdRelationshipProvider instanceProvider;
        private SelectionMode selectionMode;
        private SelectionMode selectionModeOnItemsChanged;
        private InstanceId? currentHoveredElement;
        private bool hoveredElementUpdated;
        private int framesSinceLastHoverUpdate;
        private bool hasNewSelection;
        private bool newSelectionShouldReplace;
        private object selectionTag;

        protected bool SelectedItemsUpdated { get; set; }

        protected HashSet<InstanceId> SelectedItems
        {
            get
            {
                return this.selectedItems;
            }
        }

        protected IInstanceIdRelationshipProvider InstanceIdRelationshipProvider
        {
            get
            {
                return this.instanceProvider;
            }
        }

        internal SelectionStyle SelectionStyle { get; set; }

        public bool HitTestRequested { get; set; }

        public InstanceId? HoveredElement
        {
            get
            {
                return this.currentHoveredElement;
            }
        }

        public bool RenderInFront
        {
            get
            {
                return false;
            }
        }

        public int Id
        {
            get
            {
                return this.layerId;
            }
            internal set
            {
                this.layerId = value;
            }
        }

        public event EventHandler<SelectionEventArgs> OnSelectionChanged;

        public event EventHandler<HoveredElementEventArgs> OnHoveredElementChanged;

        internal event EventHandler<SelectionStyleChangedEventArgs> OnSelectionStyleChanged;

        protected HitTestableLayer(IInstanceIdRelationshipProvider idProvider)
        {
            this.instanceProvider = idProvider;
        }

        public abstract object DrawHitTest(Renderer renderer, SceneState state, DepthStencilState depthStencil, BlendState blend, RasterizerState rasterizer, IHitTestManager hitTestManager);

        public abstract void GeoSelect(double latitude, double longitude, double distance, bool flatMap);

        public abstract Vector3F GetInstancePosition(InstanceId id);

        internal virtual void AddInstancePointsToList(InstanceId id, List<Vector3D> locations)
        {
            Vector3D vector3D = this.GetInstancePosition(id).ToVector3D();
            if (!(vector3D != Vector3D.Empty))
                return;
            locations.Add(vector3D);
        }

        internal override void Update(SceneState state)
        {
            base.Update(state);
            if (!this.HitTestRequested && this.framesSinceLastHoverUpdate > 20 && this.currentHoveredElement.HasValue)
            {
                this.currentHoveredElement = new InstanceId?();
                this.hoveredElementUpdated = true;
            }
            lock (this.newSelectionSync)
            {
                if (this.hasNewSelection)
                {
                    if (this.newSelectionShouldReplace)
                    {
                        this.selectedItems.Clear();
                        if (this.newSelection != null)
                        {
                            for (int local_0 = 0; local_0 < this.newSelection.Count; ++local_0)
                                this.selectedItems.Add(this.newSelection[local_0]);
                        }
                    }
                    else if (this.newSelection.Count > 0)
                        this.selectedItems.UnionWith((IEnumerable<InstanceId>)this.newSelection);
                    this.hasNewSelection = false;
                    this.selectionModeOnItemsChanged = SelectionMode.None;
                    this.SelectedItemsUpdated = true;
                }
            }
            if (this.SelectedItemsUpdated)
            {
                InstanceId[] selected = new InstanceId[this.selectedItems.Count];
                this.selectedItems.CopyTo(selected);
                if (this.EventDispatcher != null)
                {
                    Action action = delegate
                    {
                        if (this.OnSelectionChanged == null)
                            return;
                        this.OnSelectionChanged((object)this, new SelectionEventArgs(this, (IList<InstanceId>)selected, this.selectionModeOnItemsChanged, this.SelectionStyle, this.selectionTag));
                    };
                    this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, action);
                }
            }
            if (this.hoveredElementUpdated)
            {
                if (this.EventDispatcher != null)
                {
                    Action action = delegate
                    {
                        if (this.OnHoveredElementChanged == null)
                            return;
                        this.OnHoveredElementChanged((object)this,
                            new HoveredElementEventArgs(this.currentHoveredElement, state.ElapsedFrames));
                    };
                    this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, action);
                }
                this.hoveredElementUpdated = false;
            }
            else
                ++this.framesSinceLastHoverUpdate;
        }

        public void SetSelected(IList<InstanceId> ids, bool replaceSelection)
        {
            this.SetSelected(ids, replaceSelection, SelectionStyle.Outline, (object)null);
        }

        public void SetSelected(IList<InstanceId> ids, bool replaceSelection, SelectionStyle selectionStyle, object tag)
        {
            lock (this.newSelectionSync)
            {
                this.newSelection.Clear();
                if (ids != null)
                    this.newSelection.AddRange((IEnumerable<InstanceId>)ids);
                this.SelectionStyle = selectionStyle;
                if (this.OnSelectionStyleChanged != null)
                    this.OnSelectionStyleChanged((object)this, new SelectionStyleChangedEventArgs(this.SelectionStyle));
                this.newSelectionShouldReplace = replaceSelection;
                this.hasNewSelection = true;
                this.selectionTag = tag;
                this.IsDirty = true;
            }
        }

        public InstanceId[] GetSelected()
        {
            InstanceId[] array = new InstanceId[this.selectedItems.Count];
            this.selectedItems.CopyTo(array);
            return array;
        }

        public bool OnHitTestPass(InstanceId hitId, SceneState state, object context, out Vector3F objectPos)
        {
            objectPos = this.currentHoveredElement.HasValue ? this.GetInstancePosition(this.currentHoveredElement.Value) : Vector3F.Empty;
            if (state == null)
                return false;
            if (context != null)
            {
                SelectionMode selectionMode = (SelectionMode)context;
                if (selectionMode != SelectionMode.None)
                    this.ResetSelectionStyle();
                switch (selectionMode)
                {
                    case SelectionMode.Single:
                    case SelectionMode.SingleRightClick:
                        if (this.selectedItems.Contains(hitId))
                            return true;
                        this.selectedItems.Clear();
                        this.selectedItems.Add(hitId);
                        this.selectionModeOnItemsChanged = selectionMode;
                        this.IsDirty = true;
                        this.SelectedItemsUpdated = true;
                        break;
                    case SelectionMode.Add:
                        bool flag = true;
                        foreach (InstanceId instance in this.InstanceIdRelationshipProvider.GetRelatedIdsOverTime(hitId))
                        {
                            if (this.selectedItems.Remove(new InstanceId(hitId.LayerId, instance)))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                            this.selectedItems.Add(hitId);
                        this.selectionModeOnItemsChanged = selectionMode;
                        this.IsDirty = true;
                        this.SelectedItemsUpdated = true;
                        break;
                }
            }
            if (!this.currentHoveredElement.HasValue || !this.currentHoveredElement.Value.Equals(hitId))
            {
                this.currentHoveredElement = (int)hitId.Id != 0 ? new InstanceId?(hitId) : new InstanceId?();
                this.IsDirty = true;
                this.hoveredElementUpdated = true;
            }
            this.framesSinceLastHoverUpdate = 0;
            return false;
        }

        public void OnHitTestFail(SceneState state, object context, bool anyHitTestReturnedTrue)
        {
            if (context != null)
            {
                SelectionMode selectionMode = (SelectionMode)context;
                if (selectionMode != SelectionMode.None)
                    this.ResetSelectionStyle();
                switch (selectionMode)
                {
                    case SelectionMode.Single:
                    case SelectionMode.SingleRightClick:
                        if (!anyHitTestReturnedTrue)
                        {
                            this.selectedItems.Clear();
                            this.selectionModeOnItemsChanged = selectionMode;
                            this.IsDirty = true;
                            this.SelectedItemsUpdated = true;
                            break;
                        }
                        else
                            break;
                }
            }
            if (!this.currentHoveredElement.HasValue)
                return;
            this.currentHoveredElement = new InstanceId?();
            this.IsDirty = true;
            this.hoveredElementUpdated = true;
        }

        private void ResetSelectionStyle()
        {
            this.selectionTag = (object)null;
            this.SelectionStyle = SelectionStyle.Outline;
            if (this.OnSelectionStyleChanged == null)
                return;
            this.OnSelectionStyleChanged((object)this, new SelectionStyleChangedEventArgs(this.SelectionStyle));
        }

        protected SelectionMode GetSelectionMode()
        {
            SelectionMode selectionMode = this.selectionMode;
            this.selectionMode = SelectionMode.None;
            return selectionMode;
        }

        internal void SetSelectionMode(SelectionMode mode)
        {
            this.selectionMode = mode;
            this.IsDirty = true;
            this.HitTestRequested = true;
        }

        public override void Dispose()
        {
            base.Dispose();
            this.selectedItems = new HashSet<InstanceId>();
        }
    }
}
