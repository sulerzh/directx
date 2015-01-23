using System.Windows;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public delegate int ProcessDraggedItemFromCollectionDelegate<T>(T item, DragDropEffects effects);
}
