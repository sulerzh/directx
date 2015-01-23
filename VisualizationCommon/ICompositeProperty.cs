using System.ComponentModel;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public interface ICompositeProperty : INotifyPropertyChanged, INotifyPropertyChanging, IDescendentPropertyChanged, IDescendentPropertyChanging
  {
  }
}
