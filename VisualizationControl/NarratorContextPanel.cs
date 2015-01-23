using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public class NarratorContextPanel : Grid
  {
    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return (AutomationPeer) new FrameworkElementAutomationPeer((FrameworkElement) this);
    }
  }
}
