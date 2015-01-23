// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TimeStep
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace Microsoft.Data.Visualization.Engine
{
    internal class TimeStep : EngineStep, ITimeController, INotifyPropertyChanged
    {
        private long lastElapsedTicks = -1L;
        private object timeLock = new object();
        internal const int EngineTicksPerSecond = 60;
        private ITimeProvider timer;
        private DateTime currentVisualTime;
        private DateTime startVisualTime;
        private DateTime endVisualTime;
        private double baseRealTimeVisualOffset;
        private DateTime baseVisualTime;
        private DateTime visualTimeFreeze;
        private double visualTimeRatio;
        private TimeSpan duration;
        private bool looping;
        private bool visualTimeEnabled;

        public string PropertyVisualTimeEnabled
        {
            get
            {
                return "VisualTimeEnabled";
            }
        }

        public string PropertyLooping
        {
            get
            {
                return "Looping";
            }
        }

        public string PropertyCurrentVisualTime
        {
            get
            {
                return "CurrentVisualTime";
            }
        }

        public string PropertyDuration
        {
            get
            {
                return "Duration";
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return this.duration;
            }
            set
            {
                if (!(value > TimeSpan.Zero))
                    return;
                this.duration = value;
                this.CalculateRatio();
                if (this.EventDispatcher == null)
                    return;
                this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.RaisePropertyChanged(this.PropertyDuration)));
            }
        }

        public bool VisualTimeEnabled
        {
            get
            {
                return this.visualTimeEnabled;
            }
            set
            {
                lock (this.timeLock)
                {
                    this.baseRealTimeVisualOffset = -1.0;
                    this.visualTimeEnabled = value;
                    if (!value)
                        this.visualTimeFreeze = this.currentVisualTime;
                    if (this.EventDispatcher == null)
                        return;
                    this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.RaisePropertyChanged(this.PropertyVisualTimeEnabled)));
                }
            }
        }

        public bool Looping
        {
            get
            {
                return this.looping;
            }
            set
            {
                this.looping = value;
                if (this.EventDispatcher == null)
                    return;
                this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.RaisePropertyChanged(this.PropertyLooping)));
            }
        }

        public DateTime CurrentVisualTime
        {
            get
            {
                return this.visualTimeFreeze;
            }
            set
            {
                lock (this.timeLock)
                {
                    if (value < this.startVisualTime)
                        value = this.startVisualTime;
                    else if (value > this.endVisualTime)
                        value = this.endVisualTime;
                    this.currentVisualTime = value;
                    this.visualTimeFreeze = value;
                    this.baseRealTimeVisualOffset = -1.0;
                    if (this.EventDispatcher == null)
                        return;
                    this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.RaisePropertyChanged(this.PropertyCurrentVisualTime)));
                }
            }
        }

        public TimeStep(IVisualizationEngineDispatcher dispatcher, ITimeProvider timeProvider, Dispatcher eventDispatcher)
            : base(dispatcher, eventDispatcher)
        {
            this.Duration = TimeSpan.Zero;
            this.baseRealTimeVisualOffset = -1.0;
            this.timer = timeProvider;
        }

        public void SetVisualTimeRange(DateTime startTime, DateTime endTime, bool unionWithCurrentRange)
        {
            lock (this.timeLock)
            {
                if (unionWithCurrentRange)
                {
                    if (startTime < this.startVisualTime)
                        this.startVisualTime = startTime;
                    if (endTime > this.endVisualTime)
                        this.endVisualTime = endTime;
                }
                else
                {
                    this.startVisualTime = startTime;
                    this.endVisualTime = endTime;
                }
                if (this.currentVisualTime < this.startVisualTime)
                    this.CurrentVisualTime = this.startVisualTime;
                if (this.currentVisualTime > this.endVisualTime)
                    this.CurrentVisualTime = this.endVisualTime;
                this.baseRealTimeVisualOffset = -1.0;
                this.CalculateRatio();
            }
        }

        internal override bool PreExecute(SceneState state, int phase)
        {
            long elapsedMilliseconds = this.timer.GetElapsedMilliseconds();
            long num1 = this.timer.GetElapsedTicks() * 60L / Stopwatch.Frequency;
            if (num1 <= this.lastElapsedTicks)
                num1 = this.lastElapsedTicks + 1L;
            double num2 = (double)elapsedMilliseconds / 1000.0;
            state.ElapsedMilliseconds = elapsedMilliseconds;
            state.ElapsedTicks = num1;
            state.ElapsedSeconds = num2;
            this.lastElapsedTicks = num1;
            bool flag = false;
            lock (this.timeLock)
            {
                bool local_4 = !this.VisualTimeEnabled && this.currentVisualTime < this.visualTimeFreeze + TimeSpan.FromSeconds(0.25 * this.visualTimeRatio * 3.0);
                if (this.VisualTimeEnabled || local_4)
                {
                    if (this.baseRealTimeVisualOffset < 0.0)
                    {
                        this.baseRealTimeVisualOffset = num2;
                        this.baseVisualTime = this.currentVisualTime;
                    }
                    DateTime local_5;
                    try
                    {
                        local_5 = this.baseVisualTime.AddSeconds((num2 - this.baseRealTimeVisualOffset) * this.visualTimeRatio);
                    }
                    catch (ArgumentOutOfRangeException exception_0)
                    {
                        VisualizationTraceSource.Current.TraceEvent(TraceEventType.Error, 0, "TimeStep DateTime overflow has occurred - elapsedSeconds: {0}, baseRealTimeVisualOffset: {1}, visualTimeRatio: {2}.", (object)num2, (object)this.baseRealTimeVisualOffset, (object)this.visualTimeRatio);
                        local_5 = this.endVisualTime;
                    }
                    if (local_5 >= this.endVisualTime)
                    {
                        if (this.Looping && this.VisualTimeEnabled)
                        {
                            DateTime local_6 = this.endVisualTime + TimeSpan.FromSeconds(0.25 * this.visualTimeRatio * 2.0);
                            if (local_5 > local_6)
                            {
                                local_5 = this.startVisualTime;
                                this.baseRealTimeVisualOffset = -1.0;
                            }
                        }
                        else if (this.VisualTimeEnabled)
                        {
                            local_5 = this.endVisualTime;
                            this.currentVisualTime = local_5;
                            this.VisualTimeEnabled = false;
                        }
                    }
                    this.currentVisualTime = local_5;
                    if (this.VisualTimeEnabled)
                        this.visualTimeFreeze = this.currentVisualTime < this.endVisualTime ? this.currentVisualTime : this.endVisualTime;
                    if (this.EventDispatcher != null)
                        this.EventDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.RaisePropertyChanged(this.PropertyCurrentVisualTime)));
                    flag = true;
                }
                state.VisualTime = this.currentVisualTime;
                state.VisualTimeToRealtimeRatio = this.visualTimeRatio;
                state.VisualTimeFreeze = !this.VisualTimeEnabled || local_4 ? new DateTime?(this.visualTimeFreeze) : new DateTime?();
            }
            return flag;
        }

        internal override void Execute(Renderer renderer, SceneState state, int phase)
        {
            renderer.FrameParameters.ElapsedTime.Value = (float)state.ElapsedSeconds;
            lock (this.timeLock)
            {
                double local_0 = (this.endVisualTime - this.startVisualTime).TotalMilliseconds;
                if (local_0 == 0.0)
                    local_0 = 0.25 * state.VisualTimeToRealtimeRatio * 1000.0;
                double local_1 = (state.VisualTime - this.startVisualTime).TotalMilliseconds / local_0;
                renderer.FrameParameters.VisualTime.Value = (float)local_1;
                renderer.FrameParameters.VisualTimeScale.Value = this.visualTimeRatio == 0.0 ? 0.0f : (float)(local_0 / this.visualTimeRatio / 1000.0);
            }
        }

        private void CalculateRatio()
        {
            if (this.duration == TimeSpan.Zero)
                return;
            this.visualTimeRatio = (this.endVisualTime - this.startVisualTime).TotalMilliseconds / this.duration.TotalMilliseconds;
        }

        public override void Dispose()
        {
        }
    }
}
