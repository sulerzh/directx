using Microsoft.Data.Visualization.VisualizationCommon;
using System.ComponentModel;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public interface IViewModel : ICompositeProperty, INotifyPropertyChanged, INotifyPropertyChanging, IDescendentPropertyChanged, IDescendentPropertyChanging, IValidationElement
  {
  }
}
