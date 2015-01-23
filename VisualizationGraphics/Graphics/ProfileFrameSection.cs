namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal abstract class ProfileFrameSection : ProfileSection
    {
        public abstract float? Frequency { get; }

        public abstract bool IsReady { get; }

        public bool Discard { get; internal set; }

        protected ProfileFrameSection(string sectionName)
            : base(sectionName)
        {
        }

        public static ProfileFrameSection Create(string sectionName, Renderer renderer)
        {
            return new D3D11ProfileFrameSection(sectionName, renderer);
        }
    }
}
