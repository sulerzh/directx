namespace Microsoft.Data.Visualization.VisualizationControls
{
  public interface IDialogServiceProvider
  {
    bool ShowDialog(IDialog dialog);

    bool DismissDialog(IDialog dialog);
  }
}
