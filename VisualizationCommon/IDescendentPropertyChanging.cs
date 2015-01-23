using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public interface IDescendentPropertyChanging
  {
    event PropertyChangingEventHandler DescendentPropertyChanging;
  }
}
