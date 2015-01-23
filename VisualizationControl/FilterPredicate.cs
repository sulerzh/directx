using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public abstract class FilterPredicate
  {
    public abstract bool Unfiltered { get; }

    public abstract bool HasUpdatableProperties { get; }

    protected FilterPredicate()
    {
    }

    protected FilterPredicate(FilterPredicate.SerializableFilterPredicate state)
    {
    }

    public abstract bool SameAs(FilterPredicate filterPredicate);

    internal virtual FilterPredicateProperties UpdateProperties(ModelQueryColumn queryColumn)
    {
      return (FilterPredicateProperties) null;
    }

    protected virtual void Wrap(FilterPredicate.SerializableFilterPredicate state)
    {
    }

    internal virtual void Unwrap(FilterPredicate.SerializableFilterPredicate state)
    {
    }

    internal abstract FilterPredicate.SerializableFilterPredicate Wrap();

    [Serializable]
    public abstract class SerializableFilterPredicate
    {
      internal abstract FilterPredicate Unwrap();
    }
  }
}
