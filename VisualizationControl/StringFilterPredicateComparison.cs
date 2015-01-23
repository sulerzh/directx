using Microsoft.Data.Visualization.WpfExtensions;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public enum StringFilterPredicateComparison
  {
    [DisplayString(typeof (Resources), "FiltersTab_UnknownPredicate")] Unknown,
    [DisplayString(typeof (Resources), "FiltersTab_StringFilterOperator_Contains")] Contains,
    [DisplayString(typeof (Resources), "FiltersTab_StringFilterOperator_DoesNotContain")] DoesNotContain,
    [DisplayString(typeof (Resources), "FiltersTab_StringFilterOperator_StartsWith")] StartsWith,
    [DisplayString(typeof (Resources), "FiltersTab_StringFilterOperator_DoesNotStartWith")] DoesNotStartWith,
    [DisplayString(typeof (Resources), "FiltersTab_StringFilterOperator_Is")] Is,
    [DisplayString(typeof (Resources), "FiltersTab_StringFilterOperator_IsNot")] IsNot,
    [DisplayString(typeof (Resources), "FiltersTab_StringFilterOperator_IsBlank")] IsBlank,
    [DisplayString(typeof (Resources), "FiltersTab_StringFilterOperator_IsNotBlank")] IsNotBlank,
  }
}
