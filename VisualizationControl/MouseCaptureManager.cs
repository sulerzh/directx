using System.Windows;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  internal static class MouseCaptureManager
  {
    private static UIElement s_mouseCapturedElement;

    public static bool CaptureMouse(UIElement element)
    {
      return MouseCaptureManager.CaptureMouse(element, false);
    }

    public static bool CaptureMouse(UIElement element, bool force)
    {
      bool flag = element.CaptureMouse();
      if (!flag && force)
      {
        MouseCaptureManager.ClearMouseCapture();
        flag = element.CaptureMouse();
      }
      if (flag)
      {
        MouseCaptureManager.s_mouseCapturedElement = element;
        MouseCaptureManager.s_mouseCapturedElement.LostMouseCapture += new MouseEventHandler(MouseCaptureManager.OnElementLostMouseCapture);
      }
      return flag;
    }

    public static void ClearMouseCapture()
    {
      if (MouseCaptureManager.s_mouseCapturedElement == null)
        return;
      MouseCaptureManager.s_mouseCapturedElement.LostMouseCapture -= new MouseEventHandler(MouseCaptureManager.OnElementLostMouseCapture);
      MouseCaptureManager.s_mouseCapturedElement.ReleaseMouseCapture();
      MouseCaptureManager.s_mouseCapturedElement = (UIElement) null;
    }

    public static UIElement GetMouseCapturedElement()
    {
      return MouseCaptureManager.s_mouseCapturedElement;
    }

    private static void OnElementLostMouseCapture(object sender, MouseEventArgs e)
    {
      UIElement uiElement = sender as UIElement;
      if (MouseCaptureManager.s_mouseCapturedElement == uiElement)
        MouseCaptureManager.s_mouseCapturedElement = (UIElement) null;
      if (uiElement == null)
        return;
      uiElement.LostMouseCapture -= new MouseEventHandler(MouseCaptureManager.OnElementLostMouseCapture);
    }
  }
}
