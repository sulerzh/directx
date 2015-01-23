using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class StringFilterPredicate : FilterPredicate
  {
    public StringFilterPredicateComparison Operator { get; private set; }

    public string Value { get; private set; }

    public override bool Unfiltered
    {
      get
      {
        if (this.Operator == StringFilterPredicateComparison.Unknown)
          return true;
        if (this.Operator != StringFilterPredicateComparison.IsBlank && this.Operator != StringFilterPredicateComparison.IsNotBlank)
          return this.Value == null;
        else
          return false;
      }
    }

    public override bool HasUpdatableProperties
    {
      get
      {
        return false;
      }
    }

    public StringFilterPredicate(StringFilterPredicateComparison op, string value)
    {
      this.Operator = op;
      this.Value = value;
    }

    internal StringFilterPredicate(StringFilterPredicate.SerializableStringFilterPredicate state)
      : base((FilterPredicate.SerializableFilterPredicate) state)
    {
      this.Unwrap((FilterPredicate.SerializableFilterPredicate) state);
    }

    public override bool SameAs(FilterPredicate filterPredicate)
    {
      StringFilterPredicate stringFilterPredicate = filterPredicate as StringFilterPredicate;
      if (stringFilterPredicate == null || this.Operator != stringFilterPredicate.Operator)
        return false;
      else
        return this.Value == stringFilterPredicate.Value;
    }

    internal override FilterPredicate.SerializableFilterPredicate Wrap()
    {
      StringFilterPredicate.SerializableStringFilterPredicate stringFilterPredicate = new StringFilterPredicate.SerializableStringFilterPredicate()
      {
        Operator = this.Operator,
        Value = this.Value
      };
      base.Wrap((FilterPredicate.SerializableFilterPredicate) stringFilterPredicate);
      return (FilterPredicate.SerializableFilterPredicate) stringFilterPredicate;
    }

    internal override void Unwrap(FilterPredicate.SerializableFilterPredicate wrappedState)
    {
      if (wrappedState == null)
        throw new ArgumentNullException("wrappedState");
      StringFilterPredicate.SerializableStringFilterPredicate stringFilterPredicate = wrappedState as StringFilterPredicate.SerializableStringFilterPredicate;
      if (stringFilterPredicate == null)
        throw new ArgumentException("wrappedState must be of type SerializableStringFilterPredicate");
      base.Unwrap((FilterPredicate.SerializableFilterPredicate) stringFilterPredicate);
      this.Operator = stringFilterPredicate.Operator;
      this.Value = stringFilterPredicate.Value;
    }

    [Serializable]
    public class SerializableStringFilterPredicate : FilterPredicate.SerializableFilterPredicate
    {
      [XmlAttribute("Op")]
      public StringFilterPredicateComparison Operator;
      [XmlAttribute("Val")]
      public string Value;

      internal override FilterPredicate Unwrap()
      {
        return (FilterPredicate) new StringFilterPredicate(this);
      }
    }
  }
}
