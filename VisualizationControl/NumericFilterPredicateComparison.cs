using Microsoft.Data.Visualization.WpfExtensions;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public enum NumericFilterPredicateComparison
  {
    [DisplayString(typeof (Resources), "FiltersTab_UnknownPredicate")] Unknown,
    [DisplayString(typeof (Resources), "FiltersTab_NumericFilterOperator_Is")] Is,
    [DisplayString(typeof (Resources), "FiltersTab_NumericFilterOperator_IsNot")] IsNot,
    [DisplayString(typeof (Resources), "FiltersTab_NumericFilterOperator_IsBlank")] IsBlank,
    [DisplayString(typeof (Resources), "FiltersTab_NumericFilterOperator_IsNotBlank")] IsNotBlank,
    [DisplayString(typeof (Resources), "FiltersTab_NumericFilterOperator_IsLessThan")] IsLessThan,
    [DisplayString(typeof (Resources), "FiltersTab_NumericFilterOperator_IsLessThanOrEqualTo")] IsLessThanOrEqualTo,
    [DisplayString(typeof (Resources), "FiltersTab_NumericFilterOperator_IsGreaterThan")] IsGreaterThan,
    [DisplayString(typeof (Resources), "FiltersTab_NumericFilterOperator_IsGreaterThanOrEqualTo")] IsGreaterThanOrEqualTo,
  }
}
