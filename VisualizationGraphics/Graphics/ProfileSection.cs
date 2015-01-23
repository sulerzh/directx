using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    internal abstract class ProfileSection : GraphicsResource
    {
        private List<ProfileSection> subSections = new List<ProfileSection>();

        public string Name { get; private set; }

        public IList<ProfileSection> Sections
        {
            get
            {
                return this.subSections.AsReadOnly();
            }
        }

        public abstract float? Duration { get; }

        protected ProfileSection(string sectionName)
        {
            this.Name = sectionName;
        }

        public static ProfileSection Create(string sectionName, ProfileFrameSection parent)
        {
            return new D3D11ProfileSection(sectionName, parent);
        }

        public void AddSubSection(ProfileSection section)
        {
            this.subSections.Add(section);
        }

        public void Reset(string newSectionName, ProfileFrameSection parent)
        {
            this.Name = newSectionName;
            this.subSections.Clear();
            this.ResetState(parent);
        }

        protected abstract void ResetState(ProfileFrameSection parent);

        public abstract bool Begin();

        public abstract bool End();
    }
}
