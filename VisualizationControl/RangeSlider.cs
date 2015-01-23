using Microsoft.Reporting.Windows.Common.Internal;
using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [TemplatePart(Name = "PART_ThumbGrid", Type = typeof (Grid))]
  [TemplatePart(Name = "PART_SliderTrack", Type = typeof (Border))]
  [TemplatePart(Name = "PART_ThumbCanvas", Type = typeof (Canvas))]
  [TemplatePart(Name = "PART_LeftThumb", Type = typeof (Thumb))]
  [TemplatePart(Name = "PART_MiddleThumb", Type = typeof (Thumb))]
  [TemplatePart(Name = "PART_RightThumb", Type = typeof (Thumb))]
  public class RangeSlider : Control
  {
    public static readonly DependencyProperty RangeSliderBorderBrushProperty = DependencyProperty.Register("RangeSliderBorderBrush", typeof (Brush), typeof (RangeSlider), (PropertyMetadata) null);
    public static readonly DependencyProperty NormalBackgroundProperty = DependencyProperty.Register("NormalBackground", typeof (Brush), typeof (RangeSlider), (PropertyMetadata) null);
    public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register("HoverBackground", typeof (Brush), typeof (RangeSlider), (PropertyMetadata) null);
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof (double), typeof (RangeSlider), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(RangeSlider.OnSliderPropertyChanged)));
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof (double), typeof (RangeSlider), new PropertyMetadata((object) 1.0, new PropertyChangedCallback(RangeSlider.OnSliderPropertyChanged)));
    public static readonly DependencyProperty LeftThumbValueProperty = DependencyProperty.Register("LeftThumbValue", typeof (double), typeof (RangeSlider), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(RangeSlider.OnSliderPropertyChanged)));
    public static readonly DependencyProperty RightThumbValueProperty = DependencyProperty.Register("RightThumbValue", typeof (double), typeof (RangeSlider), new PropertyMetadata((object) 1.0, new PropertyChangedCallback(RangeSlider.OnSliderPropertyChanged)));
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof (ICommand), typeof (RangeSlider), (PropertyMetadata) null);
    public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register("SmallChange", typeof (double), typeof (RangeSlider), (PropertyMetadata) null);
    public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register("AccentBrush", typeof (Brush), typeof (RangeSlider), new PropertyMetadata((object) new SolidColorBrush(Colors.Black)));
    private bool m_templateApplied;
    private Border m_sliderTrack;
    private Thumb m_leftThumb;
    private Thumb m_middleThumb;
    private Thumb m_rightThumb;
    private Thumb m_draggedThumb;
    private Grid m_thumbGrid;
    private Canvas m_thumbCanvas;
    private double m_leftDragValue;
    private double m_rightDragValue;
    private Cursor m_oldCursor;
    private Cursor m_newCursor;
    private bool m_thumbMoved;
    private NarratorContextPanel m_leftThumbContainer;
    private NarratorContextPanel m_middleThumbContainer;
    private NarratorContextPanel m_rightThumbContainer;

    public Brush RangeSliderBorderBrush
    {
      get
      {
        return this.GetValue(RangeSlider.RangeSliderBorderBrushProperty) as Brush;
      }
      set
      {
        this.SetValue(RangeSlider.RangeSliderBorderBrushProperty, (object) value);
      }
    }

    public Brush NormalBackground
    {
      get
      {
        return this.GetValue(RangeSlider.NormalBackgroundProperty) as Brush;
      }
      set
      {
        this.SetValue(RangeSlider.NormalBackgroundProperty, (object) value);
      }
    }

    public Brush HoverBackground
    {
      get
      {
        return this.GetValue(RangeSlider.HoverBackgroundProperty) as Brush;
      }
      set
      {
        this.SetValue(RangeSlider.HoverBackgroundProperty, (object) value);
      }
    }

    public Thumb LeftThumb
    {
      get
      {
        return this.m_leftThumb;
      }
    }

    public Thumb MiddleThumb
    {
      get
      {
        return this.m_middleThumb;
      }
    }

    public Thumb RightThumb
    {
      get
      {
        return this.m_rightThumb;
      }
    }

    public Canvas ThumbCanvas
    {
      get
      {
        return this.m_thumbCanvas;
      }
    }

    public Grid ThumbGrid
    {
      get
      {
        return this.m_thumbGrid;
      }
    }

    public Border Track
    {
      get
      {
        return this.m_sliderTrack;
      }
    }

    public double Minimum
    {
      get
      {
        return (double) this.GetValue(RangeSlider.MinimumProperty);
      }
      set
      {
        this.SetValue(RangeSlider.MinimumProperty, (object) value);
      }
    }

    public double Maximum
    {
      get
      {
        return (double) this.GetValue(RangeSlider.MaximumProperty);
      }
      set
      {
        this.SetValue(RangeSlider.MaximumProperty, (object) value);
      }
    }

    public double LeftThumbValue
    {
      get
      {
        return (double) this.GetValue(RangeSlider.LeftThumbValueProperty);
      }
      set
      {
        this.SetValue(RangeSlider.LeftThumbValueProperty, (object) value);
      }
    }

    public double RightThumbValue
    {
      get
      {
        return (double) this.GetValue(RangeSlider.RightThumbValueProperty);
      }
      set
      {
        this.SetValue(RangeSlider.RightThumbValueProperty, (object) value);
      }
    }

    public ICommand Command
    {
      get
      {
        return (ICommand) this.GetValue(RangeSlider.CommandProperty);
      }
      set
      {
        this.SetValue(RangeSlider.CommandProperty, (object) value);
      }
    }

    public double SmallChange
    {
      get
      {
        return (double) this.GetValue(RangeSlider.SmallChangeProperty);
      }
      set
      {
        this.SetValue(RangeSlider.SmallChangeProperty, (object) value);
      }
    }

    public Brush AccentBrush
    {
      get
      {
        return (Brush) this.GetValue(RangeSlider.AccentBrushProperty);
      }
      set
      {
        this.SetValue(RangeSlider.AccentBrushProperty, (object) value);
      }
    }

    public double TrackLength
    {
      get
      {
        return this.m_thumbCanvas.ActualWidth - this.MinGridWidth;
      }
    }

    internal double EffectiveRightThumbValue
    {
      get
      {
        return RangeSlider.EnsureMaxMin(this.RightThumbValue, this.EffectiveMaximum, this.EffectiveMinimum);
      }
    }

    internal double EffectiveLeftThumbValue
    {
      get
      {
        return RangeSlider.EnsureMaxMin(this.LeftThumbValue, this.EffectiveRightThumbValue, this.EffectiveMinimum);
      }
    }

    private RangeAdjustmentType AdjustmentType { get; set; }

    private double EffectiveMaximum
    {
      get
      {
        return this.Maximum;
      }
    }

    private double EffectiveMinimum
    {
      get
      {
        return Math.Min(this.Minimum, this.EffectiveMaximum);
      }
    }

    private double EffectiveValueSpan
    {
      get
      {
        return this.EffectiveMaximum - this.EffectiveMinimum;
      }
    }

    private double MinGridWidth
    {
      get
      {
        return this.m_leftThumb.ActualWidth + this.m_rightThumb.ActualWidth;
      }
    }

    private double LeftThumbPosition
    {
      get
      {
        return Canvas.GetLeft((UIElement) this.m_thumbGrid);
      }
    }

    private double RightThumbPosition
    {
      get
      {
        return this.LeftThumbPosition + this.m_thumbGrid.ActualWidth;
      }
    }

    public RangeSlider()
    {
      this.DefaultStyleKey = (object) typeof (RangeSlider);
      this.KeyUp += new KeyEventHandler(this.OnKeyUp);
      this.KeyDown += new KeyEventHandler(this.OnKeyDown);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.m_leftThumb = (Thumb) this.GetTemplateChild("PART_LeftThumb");
      this.m_middleThumb = (Thumb) this.GetTemplateChild("PART_MiddleThumb");
      this.m_rightThumb = (Thumb) this.GetTemplateChild("PART_RightThumb");
      this.m_leftThumbContainer = (NarratorContextPanel) this.GetTemplateChild("LeftThumbContainer");
      this.m_middleThumbContainer = (NarratorContextPanel) this.GetTemplateChild("MiddleThumbContainer");
      this.m_rightThumbContainer = (NarratorContextPanel) this.GetTemplateChild("RightThumbContainer");
      this.m_thumbGrid = (Grid) this.GetTemplateChild("PART_ThumbGrid");
      this.m_thumbCanvas = (Canvas) this.GetTemplateChild("PART_ThumbCanvas");
      this.m_sliderTrack = (Border) this.GetTemplateChild("PART_SliderTrack");
      this.m_leftThumb.DragStarted += new DragStartedEventHandler(this.LeftThumb_DragStarted);
      this.m_rightThumb.DragStarted += new DragStartedEventHandler(this.RightThumb_DragStarted);
      this.m_middleThumb.DragStarted += new DragStartedEventHandler(this.MiddleThumb_DragStarted);
      this.m_leftThumb.DragCompleted += new DragCompletedEventHandler(this.Thumb_DragCompleted);
      this.m_middleThumb.DragCompleted += new DragCompletedEventHandler(this.Thumb_DragCompleted);
      this.m_rightThumb.DragCompleted += new DragCompletedEventHandler(this.Thumb_DragCompleted);
      this.m_leftThumb.DragDelta += new System.Windows.Controls.Primitives.DragDeltaEventHandler(this.LeftThumb_DragDelta);
      this.m_rightThumb.DragDelta += new System.Windows.Controls.Primitives.DragDeltaEventHandler(this.RightThumb_DragDelta);
      this.m_middleThumb.DragDelta += new System.Windows.Controls.Primitives.DragDeltaEventHandler(this.MiddleThumb_DragDelta);
      this.m_middleThumb.MouseEnter += new MouseEventHandler(this.Thumb_MouseEnter);
      this.m_middleThumb.MouseLeave += new MouseEventHandler(this.Thumb_MouseLeave);
      this.m_leftThumb.MouseEnter += new MouseEventHandler(this.Thumb_MouseEnter);
      this.m_leftThumb.MouseLeave += new MouseEventHandler(this.Thumb_MouseLeave);
      this.m_rightThumb.MouseEnter += new MouseEventHandler(this.Thumb_MouseEnter);
      this.m_rightThumb.MouseLeave += new MouseEventHandler(this.Thumb_MouseLeave);
      this.m_leftThumb.MouseLeftButtonUp += new MouseButtonEventHandler(this.Thumb_MouseLeftButtonUp);
      this.m_rightThumb.MouseLeftButtonUp += new MouseButtonEventHandler(this.Thumb_MouseLeftButtonUp);
      this.m_middleThumb.MouseLeftButtonUp += new MouseButtonEventHandler(this.Thumb_MouseLeftButtonUp);
      this.m_sliderTrack.MouseLeftButtonDown += new MouseButtonEventHandler(this.SliderTrack_MouseDown);
      this.SizeChanged += new SizeChangedEventHandler(this.RangeSlider_SizeChanged);
      this.m_templateApplied = true;
    }

    private void Thumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      Thumb thumb = sender as Thumb;
      if (thumb == null)
        return;
      thumb.Focus();
    }

    private void LeftThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
      this.AdjustmentType = RangeAdjustmentType.LowerBoundChanged;
      this.SynchronizeDragValues();
    }

    private void RightThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
      this.AdjustmentType = RangeAdjustmentType.UpperBoundChanged;
      this.SynchronizeDragValues();
    }

    private void MiddleThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
      this.AdjustmentType = RangeAdjustmentType.RangeMoved;
      this.SynchronizeDragValues();
    }

    private void Thumb_MouseEnter(object sender, MouseEventArgs args)
    {
      if (sender == this.m_middleThumb)
        VisualStateManager.GoToState((FrameworkElement) this, "MouseOverMiddle", true);
      else if (sender == this.m_leftThumb)
      {
        VisualStateManager.GoToState((FrameworkElement) this, "MouseOverLeft", true);
      }
      else
      {
        if (sender != this.m_rightThumb)
          return;
        VisualStateManager.GoToState((FrameworkElement) this, "MouseOverRight", true);
      }
    }

    private void Thumb_MouseLeave(object sender, MouseEventArgs args)
    {
      VisualStateManager.GoToState((FrameworkElement) this, "Normal", true);
    }

    private void RangeSlider_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.UpdateSliderInternal();
    }

    private static void OnSliderPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
      RangeSlider rangeSlider = sender as RangeSlider;
      if (rangeSlider == null)
        return;
      rangeSlider.InvalidateMeasure();
      rangeSlider.UpdateSliderInternal();
    }

    private void SliderTrack_MouseDown(object sender, MouseButtonEventArgs e)
    {
      Thumb thumb = (Thumb) null;
      Point position = e.GetPosition((IInputElement) this.m_sliderTrack);
      if (position.X < this.LeftThumbPosition)
        thumb = this.m_leftThumb;
      else if (position.X > this.RightThumbPosition)
        thumb = this.m_rightThumb;
      if (thumb == null)
        return;
      this.BeginFakeThumbDrag(thumb, e);
      this.MoveThumbToPosition(thumb, position.X);
    }

    private void SliderTrack_MouseUp(object sender, MouseButtonEventArgs e)
    {
      this.MoveThumbToPosition(this.m_draggedThumb, e.GetPosition((IInputElement) this.m_sliderTrack).X);
      this.EndFakeThumbDrag();
      this.FireCommand();
    }

    private void SliderTrack_MouseMove(object sender, MouseEventArgs e)
    {
      this.MoveThumbToPosition(this.m_draggedThumb, e.GetPosition((IInputElement) this.m_sliderTrack).X);
    }

    private void MiddleThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
      this.MoveMiddleThumb(e.HorizontalChange);
    }

    private void MoveThumbToPosition(Thumb thumb, double position)
    {
      if (thumb == this.m_leftThumb)
      {
        this.m_leftDragValue = this.LeftThumbValue;
        this.MoveLeftThumb(position - this.LeftThumbPosition);
      }
      else
      {
        this.m_rightDragValue = this.RightThumbValue;
        this.MoveRightThumb(position - this.RightThumbPosition);
      }
    }

    private void RightThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
      this.MoveRightThumb(e.HorizontalChange);
      this.AdjustmentType = RangeAdjustmentType.UpperBoundChanged;
    }

    private void LeftThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
      this.MoveLeftThumb(e.HorizontalChange);
      this.AdjustmentType = RangeAdjustmentType.LowerBoundChanged;
    }

    private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
      this.FireCommand();
    }

    private void SynchronizeDragValues()
    {
      this.m_rightDragValue = this.EffectiveRightThumbValue;
      this.m_leftDragValue = this.EffectiveLeftThumbValue;
    }

    private void UpdateSliderInternal()
    {
      if (!this.m_templateApplied)
        return;
      if (LayoutUtilities.ValueGreaterThan(this.TrackLength, this.MinGridWidth, LayoutUtilities.SmallTolerance))
      {
        if (LayoutUtilities.ValueGreaterThan(this.EffectiveValueSpan, 0.0, LayoutUtilities.SmallTolerance))
        {
          try
          {
            if (double.IsInfinity(this.EffectiveRightThumbValue) || double.IsInfinity(this.EffectiveLeftThumbValue))
              this.m_thumbGrid.Width = this.MinGridWidth + this.TrackLength;
            else
              this.m_thumbGrid.Width = this.MinGridWidth + (this.EffectiveRightThumbValue - this.EffectiveLeftThumbValue) / this.EffectiveValueSpan * this.TrackLength;
          }
          catch (ArgumentException ex)
          {
          }
          Canvas.SetLeft((UIElement) this.m_thumbGrid, ValueHelper.CanGraph(this.EffectiveLeftThumbValue) ? (this.EffectiveLeftThumbValue - this.EffectiveMinimum) / this.EffectiveValueSpan * this.TrackLength : 0.0);
        }
        else
        {
          this.m_thumbGrid.Width = this.MinGridWidth + this.TrackLength;
          Canvas.SetLeft((UIElement) this.m_thumbGrid, 0.0);
        }
      }
      else
      {
        this.m_thumbGrid.Width = this.MinGridWidth;
        Canvas.SetLeft((UIElement) this.m_thumbGrid, 0.0);
      }
    }

    private void BeginFakeThumbDrag(Thumb element, MouseButtonEventArgs e)
    {
      VisualStateManager.GoToState((FrameworkElement) element, "MouseOver", true);
      if (element == this.m_rightThumb)
        this.AdjustmentType = RangeAdjustmentType.UpperBoundChanged;
      else if (element == this.m_leftThumb)
        this.AdjustmentType = RangeAdjustmentType.LowerBoundChanged;
      this.m_oldCursor = this.Cursor;
      this.Cursor = this.m_newCursor = Cursors.SizeWE;
      this.m_draggedThumb = element;
      MouseCaptureManager.CaptureMouse((UIElement) element);
      element.MouseMove += new MouseEventHandler(this.SliderTrack_MouseMove);
      element.AddHandler(UIElement.MouseLeftButtonUpEvent, (Delegate) new MouseButtonEventHandler(this.SliderTrack_MouseUp), true);
    }

    private void EndFakeThumbDrag()
    {
      VisualStateManager.GoToState((FrameworkElement) this.m_draggedThumb, "Normal", true);
      if (this.Cursor == this.m_newCursor)
        this.Cursor = this.m_oldCursor;
      this.m_draggedThumb.ReleaseMouseCapture();
      this.m_draggedThumb.MouseMove -= new MouseEventHandler(this.SliderTrack_MouseMove);
      this.m_draggedThumb.RemoveHandler(UIElement.MouseLeftButtonUpEvent, (Delegate) new MouseButtonEventHandler(this.SliderTrack_MouseUp));
      this.m_draggedThumb = (Thumb) null;
    }

    private void MoveMiddleThumb(double distance)
    {
      double num1 = RangeSlider.EnsureMaxMin(distance, this.m_thumbCanvas.ActualWidth - this.RightThumbPosition, -this.LeftThumbPosition);
      double num2 = this.DistanceToTrackDistance(num1);
      double num3 = this.DistanceToTrackDistance(distance);
      if (this.m_leftDragValue >= this.EffectiveMinimum && this.m_rightDragValue <= this.EffectiveMaximum)
      {
        if (num1 < 0.0)
        {
          this.MoveLeftThumb(num1);
          this.MoveRightThumb(num1);
        }
        else if (num1 > 0.0)
        {
          this.MoveRightThumb(num1);
          this.MoveLeftThumb(num1);
        }
        this.m_leftDragValue = this.m_leftDragValue - num2 + num3;
        this.m_rightDragValue = this.m_rightDragValue - num2 + num3;
      }
      else
      {
        this.m_leftDragValue += num3;
        this.m_rightDragValue += num3;
      }
      this.AdjustmentType = RangeAdjustmentType.RangeMoved;
    }

    private void MoveLeftThumb(double distance)
    {
      this.m_leftDragValue += this.DistanceToTrackDistance(distance);
      if (distance + this.LeftThumbPosition <= 0.0 && this.m_leftDragValue > this.EffectiveMinimum)
        this.m_leftDragValue = this.EffectiveMinimum;
      this.LeftThumbValue = RangeSlider.EnsureMaxMin(this.m_leftDragValue, this.EffectiveRightThumbValue, this.EffectiveMinimum);
    }

    private void MoveRightThumb(double distance)
    {
      this.m_rightDragValue += this.DistanceToTrackDistance(distance);
      if (distance + this.RightThumbPosition >= this.m_thumbCanvas.ActualWidth && this.m_rightDragValue < this.EffectiveMaximum)
        this.m_rightDragValue = this.EffectiveMaximum;
      this.RightThumbValue = RangeSlider.EnsureMaxMin(this.m_rightDragValue, this.EffectiveMaximum, this.EffectiveLeftThumbValue);
    }

    private static double EnsureMaxMin(double value, double max, double min)
    {
      value = Math.Min(value, max);
      value = Math.Max(value, min);
      return value;
    }

    private void FireCommand()
    {
      if (this.Command != null && this.Command.CanExecute((object) this.AdjustmentType))
        this.Command.Execute((object) this.AdjustmentType);
      this.AdjustmentType = RangeAdjustmentType.None;
    }

    private double DistanceToTrackDistance(double position)
    {
      return position / this.TrackLength * this.EffectiveValueSpan;
    }

    private double TrackDistanceToDistance(double value)
    {
      return value * this.TrackLength / this.EffectiveValueSpan;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Left || e.Key == Key.Right)
      {
        double d = this.TrackDistanceToDistance(this.Minimum + this.SmallChange) - this.TrackDistanceToDistance(this.Minimum);
        if (double.IsNaN(d))
          return;
        double distance = e.Key == Key.Left ? -d : d;
        this.SynchronizeDragValues();
        this.m_thumbMoved = true;
        DependencyObject descendant = (DependencyObject) e.OriginalSource;
        if (descendant == this.m_leftThumb || descendant == this.m_leftThumbContainer || this.m_leftThumb.IsAncestorOf(descendant))
        {
          this.AdjustmentType = RangeAdjustmentType.LowerBoundChanged;
          this.MoveLeftThumb(distance);
        }
        else if (descendant == this.m_middleThumb || descendant == this.m_middleThumbContainer || this.m_middleThumb.IsAncestorOf(descendant))
        {
          this.AdjustmentType = RangeAdjustmentType.RangeMoved;
          this.MoveMiddleThumb(distance);
        }
        else if (descendant == this.m_rightThumb || descendant == this.m_rightThumbContainer || this.m_rightThumb.IsAncestorOf(descendant))
        {
          this.AdjustmentType = RangeAdjustmentType.UpperBoundChanged;
          this.MoveRightThumb(distance);
        }
        else
          this.m_thumbMoved = false;
        e.Handled = true;
      }
      else
      {
        if (e.Key != Key.Tab)
          return;
        if (!this.m_leftThumb.IsKeyboardFocused && !this.m_rightThumb.IsKeyboardFocused && !this.m_middleThumb.IsKeyboardFocused)
        {
          Keyboard.Focus((IInputElement) this.m_middleThumb);
          e.Handled = true;
        }
        else if (this.m_middleThumb.IsKeyboardFocused)
        {
          Keyboard.Focus((IInputElement) this.m_leftThumb);
          e.Handled = true;
        }
        else
        {
          if (!this.m_leftThumb.IsKeyboardFocused)
            return;
          Keyboard.Focus((IInputElement) this.m_rightThumb);
          e.Handled = true;
        }
      }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Left && e.Key != Key.Right)
        return;
      if (this.m_thumbMoved)
        this.FireCommand();
      this.m_thumbMoved = false;
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return (AutomationPeer) new RangeSliderAutomationPeer(this);
    }
  }
}
