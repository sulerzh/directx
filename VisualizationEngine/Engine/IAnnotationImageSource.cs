using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    public interface IAnnotationImageSource
    {
        Image GetAnnotationImage(InstanceId id, bool isSummary);
    }
}
