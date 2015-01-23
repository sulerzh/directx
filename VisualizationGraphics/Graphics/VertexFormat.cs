using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class VertexFormat : IEquatable<VertexFormat>
    {
        private List<VertexComponent> components = new List<VertexComponent>();
        private int sizeInBytes = -1;
        private VertexFormat parentFormat;
        private VertexFormat[] streamFormats;

        public IList<VertexComponent> Components
        {
            get
            {
                return this.components.AsReadOnly();
            }
        }

        public VertexFormat[] StreamFormats
        {
            get
            {
                if (this.streamFormats == null)
                {
                    var dictionary = new Dictionary<int, List<VertexComponent>>();
                    for (int i = 0; i < this.components.Count; ++i)
                    {
                        int slot = this.components[i].Slot;
                        if (!dictionary.ContainsKey(slot))
                            dictionary.Add(slot, new List<VertexComponent>());
                        dictionary[slot].Add(this.components[i]);
                    }
                    this.streamFormats = new VertexFormat[dictionary.Count];
                    int index = 0;
                    foreach (int key in dictionary.Keys)
                    {
                        this.streamFormats[index] = VertexFormat.Create(dictionary[key].ToArray());
                        this.streamFormats[index].parentFormat = this;
                        ++index;
                    }
                }
                return this.streamFormats;
            }
        }

        public VertexFormat FormatAllStreams
        {
            get
            {
                return this.parentFormat;
            }
        }

        protected VertexFormat()
        {
        }

        protected VertexFormat(VertexComponent component0)
        {
            this.components.Add(component0);
        }

        protected VertexFormat(VertexComponent component0, VertexComponent component1)
        {
            this.components.Add(component0);
            this.components.Add(component1);
        }

        protected VertexFormat(VertexComponent component0, VertexComponent component1, VertexComponent component2)
        {
            this.components.Add(component0);
            this.components.Add(component1);
            this.components.Add(component2);
        }

        protected VertexFormat(VertexComponent component0, VertexComponent component1, VertexComponent component2, VertexComponent component3)
        {
            this.components.Add(component0);
            this.components.Add(component1);
            this.components.Add(component2);
            this.components.Add(component3);
        }

        protected VertexFormat(VertexComponent[] components)
        {
            this.components.AddRange(components);
        }

        public static VertexFormat Create(VertexComponent component0)
        {
            return new D3D11VertexFormat(component0);
        }

        public static VertexFormat Create(VertexComponent component0, VertexComponent component1)
        {
            return new D3D11VertexFormat(component0, component1);
        }

        public static VertexFormat Create(VertexComponent component0, VertexComponent component1, VertexComponent component2)
        {
            return new D3D11VertexFormat(component0, component1, component2);
        }

        public static VertexFormat Create(VertexComponent component0, VertexComponent component1, VertexComponent component2, VertexComponent component3)
        {
            return new D3D11VertexFormat(component0, component1, component2, component3);
        }

        public static VertexFormat Create(VertexComponent[] components)
        {
            return new D3D11VertexFormat(components);
        }

        public int GetVertexSizeInBytes()
        {
            if (this.sizeInBytes < 0)
            {
                this.sizeInBytes = 0;
                for (int i = 0; i < this.components.Count; ++i)
                    this.sizeInBytes += this.components[i].SizeInBytes;
            }
            return this.sizeInBytes;
        }

        public bool HasComponent(VertexSemantic semantic)
        {
            int count = 0;
            return this.HasComponent(semantic, out count);
        }

        public bool HasComponent(VertexSemantic semantic, out int count)
        {
            int num = 0;
            for (int i = 0; i < this.components.Count; ++i)
            {
                if (this.components[i].Semantic == semantic)
                    ++num;
            }
            count = num;
            return num > 0;
        }

        public bool IsSubsetOf(VertexFormat other)
        {
            for (int i = 0; i < this.components.Count; ++i)
            {
                if (!this.components[i].Equals(other.components[i]))
                    return false;
            }
            return true;
        }

        public bool Equals(VertexFormat other)
        {
            if (this.components.Count == other.components.Count)
                return this.IsSubsetOf(other);
            return false;
        }
    }
}
