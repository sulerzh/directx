using System.Windows;
using System.Windows.Automation.Peers;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class RangeSliderAutomationPeer : FrameworkElementAutomationPeer
  {
    public RangeSliderAutomationPeer(RangeSlider owner)
      : base((FrameworkElement) owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
      return AutomationControlType.Custom;
    }

    protected override string GetClassNameCore()
    {
      return "RangeSlider";
    }
  }
}
