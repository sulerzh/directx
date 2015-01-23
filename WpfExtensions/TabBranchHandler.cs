using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Data.Visualization.WpfExtensions
{
  public class TabBranchHandler
  {
    private FrameworkElement topLevel;
    private readonly KeyGesture switchTabBranchKey;
    private LinkedList<UIElement> tabBranchRoots;

    private event Action Detached;

    public TabBranchHandler(FrameworkElement topLevel, IReadOnlyList<UIElement> tabGroups, KeyGesture switchTabBranchKey)
    {
      this.topLevel = topLevel;
      this.switchTabBranchKey = switchTabBranchKey;
      this.tabBranchRoots = new LinkedList<UIElement>();
      foreach (UIElement uiElement in (IEnumerable<UIElement>) tabGroups)
      {
        UIElement tabBranchRoot = uiElement;
        LinkedListNode<UIElement> node = new LinkedListNode<UIElement>(tabBranchRoot);
        this.tabBranchRoots.AddLast(node);
        KeyEventHandler detacher = (KeyEventHandler) ((sender, e) => this.BranchRootKeyDown(sender, e, node));
        tabBranchRoot.PreviewKeyDown += detacher;
        this.Detached += (Action) (() => tabBranchRoot.PreviewKeyDown -= detacher);
        tabBranchRoot.SetValue(KeyboardNavigation.TabNavigationProperty, (object) KeyboardNavigationMode.Cycle);
        tabBranchRoot.SetValue(KeyboardNavigation.DirectionalNavigationProperty, (object) KeyboardNavigationMode.Cycle);
      }
    }

    public void Detach()
    {
      if (this.Detached == null)
        return;
      this.Detached();
    }

    private void BranchRootKeyDown(object sender, KeyEventArgs e, LinkedListNode<UIElement> node)
    {
      if (!this.switchTabBranchKey.Matches(sender, (InputEventArgs) e))
        return;
      this.NextTabBranch(node);
      e.Handled = true;
    }

    private void NextTabBranch(LinkedListNode<UIElement> currentBranchNode)
    {
      LinkedListNode<UIElement> linkedListNode = currentBranchNode;
      do
      {
        linkedListNode = linkedListNode.Next ?? this.tabBranchRoots.First;
      }
      while (!linkedListNode.Value.IsVisible && linkedListNode.Value != currentBranchNode.Value);
      this.SwitchToTabBranch(linkedListNode.Value);
    }

    public void SwitchToTabBranch(UIElement branchRoot)
    {
      Keyboard.Focus((IInputElement) branchRoot);
      branchRoot.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
    }
  }
}
