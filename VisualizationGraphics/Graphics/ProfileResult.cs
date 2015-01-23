namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class ProfileResult
    {
        public string Name { get; private set; }

        public float? Duration { get; private set; }

        public int Level { get; private set; }

        public ProfileResult(string name, float? duration, int level)
        {
            this.Name = name;
            this.Duration = duration;
            this.Level = level;
        }
    }
}
