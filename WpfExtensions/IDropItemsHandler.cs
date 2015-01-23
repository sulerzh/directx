using System.Collections.ObjectModel;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public interface IDropItemsHandler : IDropHandler
  {
    void DropItem(object item, int index);

    void AddDroppableTypeOnDrop<T>(DropItemIntoCollectionDelegate<T> onDropCallback);

    void AddDefaultDropCollection<T>(Collection<T> collection);

    void AddDroppableTypeHandlers<T>(DropItemIntoCollectionDelegate<T> onDropCallback, ValidateDropItemDelegate<T> validateCallback);

    void AddDroppableTypeHandlers<T>(Collection<T> collection, ValidateDropItemDelegate<T> validateCallback);
  }
}
