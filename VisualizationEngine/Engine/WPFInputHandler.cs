// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.WPFInputHandler
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
    public class WPFInputHandler : InputHandler
    {
        private readonly Vector2D MinScaleManipulation = new Vector2D(0.02, 0.02);
        private Dictionary<int, Point> lastCursorPosition = new Dictionary<int, Point>();
        private List<SingleTouchPoint> latestManipPositions = new List<SingleTouchPoint>();
        private int currentTapAndHoldId = -1;
        private HashSet<int> tapCandidates = new HashSet<int>();
        private Vector2D accumulatedTranslationDelta = Vector2D.Empty;
        private Vector2D accumulatedScaleDelta = Vector2D.Empty;
        private const double TouchDragInertiaFactor = 100.0;
        private const double MaxDoubleTapDistance = 50.0;
        private const double MinTranslationManipulation = 15.0;
        private const double MinRotationManipulation = 0.261799387799149;
        private const double MinTwoContactsTranslationManipulation = 70.0;
        private const double MinTouchDistanceForRotation = 144.0;
        private const int DoubleTapTime = 500;
        private const int TapAndHoldTime = 1000;
        private const int MinTouchMoveWhenTapAndHold = 48;
        private const int MinTouchMoveForTap = 10;
        private UIElement inputElement;
        private bool lastEventHadTwoContactPoints;
        private TouchPoint lastTapPoint;
        private int lastTapTimeStamp;
        private Timer tapAndHoldTimer;
        private TouchPoint tapAndHoldPoint;
        private bool gotTapsDuringTapAndHold;
        private double accumulatedRotationDelta;
        private bool manipulationTranslationEnabled;
        private bool manipulationScaleEnabled;
        private bool manipulationRotationEnabled;
        private bool manipulationTranslationTwoContactsEnabled;

        public override Point? CursorPosition
        {
            get
            {
                if (!this.lastCursorPosition.ContainsKey(0))
                    return new Point?();
                else
                    return new Point?(this.lastCursorPosition[0]);
            }
        }

        public event EventHandler OnTapAndHoldLeave;

        public event EventHandler OnTouchDown;

        public WPFInputHandler(UIElement element)
        {
            if (element == null)
                return;
            this.inputElement = element;
            this.inputElement.KeyDown += new KeyEventHandler(this.KeyDown);
            this.inputElement.KeyUp += new KeyEventHandler(this.KeyUp);
            this.inputElement.MouseLeftButtonDown += new MouseButtonEventHandler(this.MouseLeftDown);
            this.inputElement.MouseLeftButtonUp += new MouseButtonEventHandler(this.MouseLeftUp);
            this.inputElement.MouseRightButtonDown += new MouseButtonEventHandler(this.MouseRightDown);
            this.inputElement.MouseRightButtonUp += new MouseButtonEventHandler(this.MouseRightUp);
            this.inputElement.MouseMove += new MouseEventHandler(this.MouseMove);
            this.inputElement.MouseWheel += new MouseWheelEventHandler(this.MouseWheel);
            this.inputElement.MouseLeave += new MouseEventHandler(this.MouseLeave);
            this.inputElement.IsManipulationEnabled = true;
            this.inputElement.ManipulationStarting += new EventHandler<ManipulationStartingEventArgs>(this.ManipulationStarting);
            this.inputElement.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this.ManipulationDelta);
            this.inputElement.ManipulationInertiaStarting += new EventHandler<ManipulationInertiaStartingEventArgs>(this.ManipulationInertiaStarting);
            this.inputElement.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this.ManipulationCompleted);
            this.inputElement.TouchDown += new EventHandler<TouchEventArgs>(this.TouchDown);
            this.inputElement.TouchUp += new EventHandler<TouchEventArgs>(this.TouchUp);
            this.inputElement.TouchMove += new EventHandler<TouchEventArgs>(this.TouchMove);
            this.inputElement.TouchLeave += new EventHandler<TouchEventArgs>(this.TouchLeave);
            element.Focusable = true;
            Keyboard.Focus((IInputElement)element);
        }

        private void TouchLeave(object sender, TouchEventArgs e)
        {
            this.tapCandidates.Remove(e.TouchDevice.Id);
            if (e.TouchDevice.Id == this.currentTapAndHoldId)
                this.currentTapAndHoldId = -1;
            this.lastCursorPosition.Remove(e.TouchDevice.Id);
            this.lastCursorPosition.Remove(0);
        }

        private void TouchMove(object sender, TouchEventArgs e)
        {
            TouchPoint touchPoint = e.GetTouchPoint((IInputElement)this.inputElement);
            double num = this.currentTapAndHoldId != -1 ? 48.0 : 10.0;
            if (this.lastCursorPosition.ContainsKey(touchPoint.TouchDevice.Id) && (touchPoint.Position - this.lastCursorPosition[touchPoint.TouchDevice.Id]).Length > num)
            {
                this.tapCandidates.Remove(e.TouchDevice.Id);
                this.lastCursorPosition[touchPoint.TouchDevice.Id] = touchPoint.Position;
                this.lastCursorPosition[0] = touchPoint.Position;
                if (this.currentTapAndHoldId != -1)
                {
                    this.currentTapAndHoldId = -1;
                    this.AddEvent((InputEvent)new TapInputEvent(TouchEventType.TapAndHoldLeave, touchPoint.Position, false, 1));
                }
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void TouchUp(object sender, TouchEventArgs e)
        {
            TouchPoint touchPoint = e.GetTouchPoint((IInputElement)this.inputElement);
            if (this.currentTapAndHoldId == touchPoint.TouchDevice.Id)
            {
                if (!this.gotTapsDuringTapAndHold && this.OnTapAndHoldLeave != null)
                    this.OnTapAndHoldLeave((object)this, new EventArgs());
                this.AddEvent((InputEvent)new TapInputEvent(TouchEventType.TapAndHoldLeave, touchPoint.Position, false, 1));
                this.currentTapAndHoldId = -1;
                this.gotTapsDuringTapAndHold = false;
            }
            else if (this.tapCandidates.Remove(touchPoint.TouchDevice.Id))
            {
                if (this.currentTapAndHoldId != -1)
                    this.gotTapsDuringTapAndHold = true;
                int tapCount = 1;
                if (this.lastTapPoint != null && ((this.lastTapPoint.Position - touchPoint.Position).Length < 50.0 && e.Timestamp - this.lastTapTimeStamp < 500))
                {
                    tapCount = 2;
                    this.lastTapPoint = (TouchPoint)null;
                }
                else
                {
                    this.lastTapPoint = touchPoint;
                    this.lastTapTimeStamp = e.Timestamp;
                }
                this.AddEvent((InputEvent)new TapInputEvent(TouchEventType.Tap, touchPoint.Position, this.currentTapAndHoldId != -1, tapCount));
                e.Handled = true;
            }
            this.lastCursorPosition.Remove(touchPoint.TouchDevice.Id);
        }

        private void TouchDown(object sender, TouchEventArgs e)
        {
            TouchPoint touchPoint = e.GetTouchPoint((IInputElement)this.inputElement);
            int id = touchPoint.TouchDevice.Id;
            this.tapCandidates.Add(id);
            this.lastCursorPosition.Add(id, touchPoint.Position);
            this.lastCursorPosition[0] = touchPoint.Position;
            e.Handled = this.currentTapAndHoldId != -1;
            if (this.currentTapAndHoldId == -1 && this.tapCandidates.Count == 1)
            {
                this.tapAndHoldPoint = touchPoint;
                if (this.tapAndHoldTimer == null)
                    this.tapAndHoldTimer = new Timer(new TimerCallback(this.TouchDownTimer), (object)null, 1000, -1);
                else
                    this.tapAndHoldTimer.Change(1000, -1);
            }
            if (this.OnTouchDown == null)
                return;
            this.OnTouchDown((object)this, (EventArgs)e);
        }

        private void TouchDownTimer(object state)
        {
            this.inputElement.Dispatcher.BeginInvoke(
                (Action<object[]>)(param =>
                {
                    if (!this.tapCandidates.Remove(this.tapAndHoldPoint.TouchDevice.Id))
                        return;
                    this.currentTapAndHoldId = this.tapAndHoldPoint.TouchDevice.Id;
                    this.AddEvent((InputEvent)new TapInputEvent(TouchEventType.TapAndHoldEnter, this.tapAndHoldPoint.Position, false, 1));
                }),
                DispatcherPriority.Input,
                new object[1] { state });
        }

        private void ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            int deviceCount = this.GetDeviceCount();
            Vector2D translationDelta = new Vector2D(e.TotalManipulation.Translation.X, e.TotalManipulation.Translation.Y);
            Vector2D scaleDelta = new Vector2D(e.TotalManipulation.Scale.X, e.TotalManipulation.Scale.Y);
            double rotationDelta = e.TotalManipulation.Rotation * (Math.PI / 180.0);
            this.lastEventHadTwoContactPoints = deviceCount == 2;
            this.AddEvent((InputEvent)new GestureEndInputEvent(translationDelta, scaleDelta, rotationDelta, this.lastEventHadTwoContactPoints));
        }

        private int GetDeviceCount()
        {
            int num = 0;
            foreach (TouchDevice touchDevice in this.inputElement.TouchesDirectlyOver)
                ++num;
            return num;
        }

        private void ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 6.0 / 625.0;
            this.GetDeviceCount();
        }

        private List<SingleTouchPoint> UpdateTrackedManipulations(ManipulationDeltaEventArgs mdea, bool isRestart = false)
        {
            return this.UpdateTrackedManipulations(mdea.Manipulators, mdea.ManipulationContainer, isRestart);
        }

        private List<SingleTouchPoint> UpdateTrackedManipulations(IEnumerable<IManipulator> e_manips, IInputElement e_contrainer, bool isRestart)
        {
            List<SingleTouchPoint> list = this.latestManipPositions;
            if (isRestart)
                list.Clear();
            for (int index = 0; index < list.Count; ++index)
            {
                SingleTouchPoint cur = list[index];
                if (!Enumerable.Any<IManipulator>(e_manips, (Func<IManipulator, bool>)(k => k.Id == cur.Id)))
                {
                    list.RemoveAt(index);
                    --index;
                }
            }
            foreach (IManipulator manipulator in e_manips)
            {
                int id = manipulator.Id;
                Point position = manipulator.GetPosition(e_contrainer);
                SingleTouchPoint singleTouchPoint1 = list.Find((Predicate<SingleTouchPoint>)(k => k.Id == id));
                if (singleTouchPoint1 != null)
                {
                    singleTouchPoint1.PosCurrent = position;
                }
                else
                {
                    SingleTouchPoint singleTouchPoint2 = new SingleTouchPoint()
                    {
                        Id = id,
                        PosStart = position,
                        PosCurrent = position,
                        HasFirstWorldSet = false
                    };
                    list.Add(singleTouchPoint2);
                }
            }
            return list;
        }

        private void ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (this.lastEventHadTwoContactPoints && e.IsInertial)
                e.Complete();
            List<SingleTouchPoint> list = this.UpdateTrackedManipulations(e, false);
            int count = list.Count;
            Point[] pointArray = new Point[2];
            Vector2D translation = new Vector2D(e.DeltaManipulation.Translation.X, e.DeltaManipulation.Translation.Y);
            Vector2D scale = new Vector2D(e.DeltaManipulation.Scale.X, e.DeltaManipulation.Scale.Y);
            double rotation = e.DeltaManipulation.Rotation * (Math.PI / 180.0);
            this.lastEventHadTwoContactPoints = count >= 2;
            double distanceBetweenPoints = 0.0;
            Vector2D center = Vector2D.Empty;
            int index = 0;
            foreach (SingleTouchPoint singleTouchPoint in list)
            {
                pointArray[index] = singleTouchPoint.PosCurrent;
                center += new Vector2D(pointArray[index].X, pointArray[index].Y);
                if (++index == 2)
                    break;
            }
            if (this.lastEventHadTwoContactPoints)
                distanceBetweenPoints = (pointArray[1] - pointArray[0]).Length;
            if (this.lastEventHadTwoContactPoints)
                center /= 2.0;
            this.FilterManipulation(ref translation, ref scale, ref rotation, distanceBetweenPoints);
            if (translation == Vector2D.Empty && scale == new Vector2D(1.0, 1.0) && rotation == 0.0)
                return;
            this.AddEvent((InputEvent)new GestureInputEvent(translation, scale, center, rotation, this.lastEventHadTwoContactPoints)
            {
                MultiTouchCurrentPostions = Enumerable.ToList<SingleTouchPoint>((IEnumerable<SingleTouchPoint>)this.latestManipPositions)
            });
        }

        private void ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.Mode = ManipulationModes.All;
            e.ManipulationContainer = (IInputElement)this.inputElement;
            this.UpdateTrackedManipulations(e.Manipulators, e.ManipulationContainer, true);
            this.accumulatedScaleDelta = Vector2D.Empty;
            this.accumulatedRotationDelta = 0.0;
            this.accumulatedTranslationDelta = Vector2D.Empty;
            this.manipulationTranslationEnabled = false;
            this.manipulationScaleEnabled = false;
            this.manipulationRotationEnabled = false;
            this.manipulationTranslationTwoContactsEnabled = false;
        }

        private void FilterManipulation(ref Vector2D translation, ref Vector2D scale, ref double rotation, double distanceBetweenPoints)
        {
            this.accumulatedScaleDelta += scale - new Vector2D(1.0, 1.0);
            this.accumulatedRotationDelta += distanceBetweenPoints < 144.0 ? 0.0 : rotation;
            this.accumulatedTranslationDelta += translation;
            if (!this.manipulationScaleEnabled && Math.Abs(this.accumulatedScaleDelta.X) < this.MinScaleManipulation.X && Math.Abs(this.accumulatedScaleDelta.Y) < this.MinScaleManipulation.Y)
                scale = new Vector2D(0.0, 0.0);
            else
                this.manipulationScaleEnabled = true;
            if (!this.manipulationRotationEnabled && Math.Abs(this.accumulatedRotationDelta) < Math.PI / 12.0)
                rotation = 0.0;
            else
                this.manipulationRotationEnabled = true;
            if (!this.manipulationTranslationEnabled && this.accumulatedTranslationDelta.Length() < 15.0)
            {
                translation = Vector2D.Empty;
            }
            else
            {
                this.manipulationTranslationEnabled = true;
                if (distanceBetweenPoints <= 0.0)
                    return;
                if (!this.manipulationTranslationTwoContactsEnabled && Math.Abs(this.accumulatedTranslationDelta.Y) < 70.0)
                    translation.Y = 0.0;
                else
                    this.manipulationTranslationTwoContactsEnabled = true;
            }
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            this.AddEvent((InputEvent)new KeyInputEvent(KeyEventType.KeyDown, e.Key == Key.System ? e.SystemKey : e.Key, Keyboard.Modifiers));
            e.Handled = true;
        }

        private void KeyUp(object sender, KeyEventArgs e)
        {
            this.AddEvent((InputEvent)new KeyInputEvent(KeyEventType.KeyUp, e.Key == Key.System ? e.SystemKey : e.Key, Keyboard.Modifiers));
            e.Handled = true;
        }

        private void MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
                this.AddMouseEvent(MouseEventType.DoubleClick, (MouseEventArgs)e);
            else
                this.AddMouseEvent(MouseEventType.LeftButtonDown, (MouseEventArgs)e);
            Keyboard.Focus((IInputElement)this.inputElement);
        }

        private void MouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            this.AddMouseEvent(MouseEventType.LeftButtonUp, (MouseEventArgs)e);
        }

        private void MouseRightDown(object sender, MouseButtonEventArgs e)
        {
            this.AddMouseEvent(MouseEventType.RightButtonDown, (MouseEventArgs)e);
            Keyboard.Focus((IInputElement)this.inputElement);
        }

        private void MouseRightUp(object sender, MouseButtonEventArgs e)
        {
            this.AddMouseEvent(MouseEventType.RightButtonUp, (MouseEventArgs)e);
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            this.AddMouseEvent(MouseEventType.Move, e);
        }

        private void MouseLeave(object sender, MouseEventArgs e)
        {
            this.AddMouseEvent(MouseEventType.Leave, e);
            this.lastCursorPosition.Remove(0);
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.AddMouseEvent(MouseEventType.Wheel, (MouseEventArgs)e);
        }

        private void AddMouseEvent(MouseEventType eventType, MouseEventArgs e)
        {
            Point position = e.GetPosition((IInputElement)this.inputElement);
            Vector positionDelta = this.lastCursorPosition.ContainsKey(0) ? position - this.lastCursorPosition[0] : new Vector(0.0, 0.0);
            this.lastCursorPosition[0] = position;
            ModifierKeys modifiers = Keyboard.Modifiers;
            bool leftPressed = e.LeftButton == MouseButtonState.Pressed;
            bool rightPressed = e.RightButton == MouseButtonState.Pressed;
            MouseInputEvent mouseInputEvent = eventType != MouseEventType.Wheel ? new MouseInputEvent(eventType, position, positionDelta, modifiers, leftPressed, rightPressed) : (MouseInputEvent)new MouseWheelInputEvent(((MouseWheelEventArgs)e).Delta, position, modifiers, leftPressed, rightPressed);
            if (eventType == MouseEventType.RightButtonDown || eventType == MouseEventType.RightButtonUp)
                e.Handled = false;
            else
                e.Handled = true;
            this.AddEvent((InputEvent)mouseInputEvent);
        }
    }
}
