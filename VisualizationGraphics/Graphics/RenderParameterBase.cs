using System;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public abstract class RenderParameterBase : IRenderParameter
    {
        public abstract int SizeInBytes { get; }

        public string Name { get; private set; }

        public RenderParameters[] Parents { get; set; }

        public RenderParameterBase(string name)
        {
            this.Name = name;
        }

        public abstract void CopyDataToBlob(IntPtr blob);

        protected void NotifyParents()
        {
            if (this.Parents == null)
                return;
            for (int index = 0; index < this.Parents.Length; ++index)
                this.Parents[index].SetDirty();
        }
    }
}
