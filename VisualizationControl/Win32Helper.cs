using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public static class Win32Helper
  {
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern Win32Helper.ExecutionState SetThreadExecutionState(Win32Helper.ExecutionState esFlags);

    [Flags]
    public enum ExecutionState : uint
    {
      ES_AWAYMODE_REQUIRED = 64U,
      ES_CONTINUOUS = 2147483648U,
      ES_DISPLAY_REQUIRED = 2U,
      ES_SYSTEM_REQUIRED = 1U,
      ES_USER_PRESENT = 4U,
    }
  }
}
