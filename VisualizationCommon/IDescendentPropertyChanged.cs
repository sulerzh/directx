using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public interface IDescendentPropertyChanged
  {
    event PropertyChangedEventHandler DescendentPropertyChanged;
  }
}
