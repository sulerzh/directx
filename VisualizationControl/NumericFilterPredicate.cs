using System;
using System.Xml.Serialization;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class NumericFilterPredicate : FilterPredicate
  {
    public NumericFilterPredicateComparison Operator { get; private set; }

    public double? Value { get; private set; }

    public override bool Unfiltered
    {
      get
      {
        if (this.Operator == NumericFilterPredicateComparison.Unknown)
          return true;
        if (this.Operator != NumericFilterPredicateComparison.IsBlank && this.Operator != NumericFilterPredicateComparison.IsNotBlank)
          return !this.Value.HasValue;
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

    public NumericFilterPredicate(NumericFilterPredicateComparison op, double? value)
    {
      if (value.HasValue && double.IsNaN(value.Value))
        throw new ArgumentException("value may not be double.NaN");
      this.Operator = op;
      this.Value = value;
    }

    internal NumericFilterPredicate(NumericFilterPredicate.SerializableNumericFilterPredicate state)
      : base((FilterPredicate.SerializableFilterPredicate) state)
    {
      this.Unwrap((FilterPredicate.SerializableFilterPredicate) state);
    }

    public override bool SameAs(FilterPredicate filterPredicate)
    {
      NumericFilterPredicate numericFilterPredicate = filterPredicate as NumericFilterPredicate;
      if (numericFilterPredicate == null || this.Operator != numericFilterPredicate.Operator)
        return false;
      double? nullable1 = this.Value;
      double? nullable2 = numericFilterPredicate.Value;
      if (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault())
        return nullable1.HasValue == nullable2.HasValue;
      else
        return false;
    }

    internal override FilterPredicate.SerializableFilterPredicate Wrap()
    {
      NumericFilterPredicate.SerializableNumericFilterPredicate numericFilterPredicate = new NumericFilterPredicate.SerializableNumericFilterPredicate()
      {
        Operator = this.Operator,
        Value = this.Value.HasValue ? this.Value.Value : double.NaN
      };
      base.Wrap((FilterPredicate.SerializableFilterPredicate) numericFilterPredicate);
      return (FilterPredicate.SerializableFilterPredicate) numericFilterPredicate;
    }

    internal override void Unwrap(FilterPredicate.SerializableFilterPredicate wrappedState)
    {
      if (wrappedState == null)
        throw new ArgumentNullException("wrappedState");
      NumericFilterPredicate.SerializableNumericFilterPredicate numericFilterPredicate = wrappedState as NumericFilterPredicate.SerializableNumericFilterPredicate;
      if (numericFilterPredicate == null)
        throw new ArgumentException("wrappedState must be of type SerializableNumericFilterPredicate");
      base.Unwrap((FilterPredicate.SerializableFilterPredicate) numericFilterPredicate);
      this.Operator = numericFilterPredicate.Operator;
      this.Value = double.IsNaN(numericFilterPredicate.Value) ? new double?() : new double?(numericFilterPredicate.Value);
    }

    [Serializable]
    public class SerializableNumericFilterPredicate : FilterPredicate.SerializableFilterPredicate
    {
      [XmlAttribute("Op")]
      public NumericFilterPredicateComparison Operator;
      [XmlAttribute("Val")]
      public double Value;

      internal override FilterPredicate Unwrap()
      {
        return (FilterPredicate) new NumericFilterPredicate(this);
      }
    }
  }
}
