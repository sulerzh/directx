using Microsoft.Data.Visualization.WpfExtensions;
using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public enum FilterPredicateOperator
  {
    [DisplayString(typeof (Resources), "FiltersTab_FilterPredicateOperator_And")] And,
    [DisplayString(typeof (Resources), "FiltersTab_FilterPredicateOperator_Or")] Or,
  }
}
