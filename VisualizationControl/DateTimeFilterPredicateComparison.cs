using Microsoft.Data.Visualization.WpfExtensions;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public enum DateTimeFilterPredicateComparison
  {
    [DisplayString(typeof (Resources), "FiltersTab_UnknownPredicate")] Unknown,
    [DisplayString(typeof (Resources), "FiltersTab_DateTimeFilterOperator_Is")] Is,
    [DisplayString(typeof (Resources), "FiltersTab_DateTimeFilterOperator_IsNot")] IsNot,
    [DisplayString(typeof (Resources), "FiltersTab_DateTimeFilterOperator_IsBlank")] IsBlank,
    [DisplayString(typeof (Resources), "FiltersTab_DateTimeFilterOperator_IsNotBlank")] IsNotBlank,
    [DisplayString(typeof (Resources), "FiltersTab_DateTimeFilterOperator_IsBefore")] IsBefore,
    [DisplayString(typeof (Resources), "FiltersTab_DateTimeFilterOperator_IsOnOrBefore")] IsOnOrBefore,
    [DisplayString(typeof (Resources), "FiltersTab_DateTimeFilterOperator_IsAfter")] IsAfter,
    [DisplayString(typeof (Resources), "FiltersTab_DateTimeFilterOperator_IsOnOrAfter")] IsOnOrAfter,
  }
}
