using System.Windows;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public delegate void ProcessDraggedItemDelegate<T>(T item, DragDropEffects effects);
}
