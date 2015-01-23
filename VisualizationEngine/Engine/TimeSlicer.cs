// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.TimeSlicer
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
    internal abstract class TimeSlicer
    {
        protected List<int> cursors = new List<int>();
        protected List<InstanceData> instances;
        protected DateTime nextTime;
        protected bool withTime;

        public TimeSlicer(List<InstanceData> instanceList, bool isTimeInvolved)
        {
            this.instances = instanceList;
            this.withTime = isTimeInvolved;
        }

        private bool IncrementCursor(int i, int last)
        {
            if (i + 1 < this.cursors.Count)
            {
                if ((int)this.instances[this.cursors[i]].Shift != (int)this.instances[this.cursors[i] + 1].Shift)
                    return false;
            }
            else if (this.cursors[i] >= last)
                return false;
            List<int> list;
            int index;
            InstanceData instanceData = this.instances[(list = this.cursors)[index = i] = list[index] + 1];
            if (instanceData.StartTime.HasValue)
            {
                DateTime? nullable = instanceData.StartTime;
                DateTime dateTime = this.nextTime;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() < dateTime ? 1 : 0) : 0) != 0)
                    this.nextTime = instanceData.StartTime.Value;
            }
            return true;
        }

        private void ProcessAtLocationWithTime(int first, int last)
        {
            while (first > 0 && !this.instances[first].FirstInstance)
                --first;
            this.cursors.Clear();
            int num = -1;
            for (int index = first; index <= last; ++index)
            {
                if ((int)this.instances[index].Shift != num)
                {
                    ++num;
                    this.cursors.Add(index);
                }
            }
            DateTime dateTime1 = DateTime.MinValue;
            do
            {
                this.nextTime = DateTime.MaxValue;
                this.BeginSlice();
                for (int i = 0; i < this.cursors.Count; ++i)
                {
                    InstanceData instance = this.instances[this.cursors[i]];
                    if (instance.EndTime.HasValue)
                    {
                        DateTime? nullable = instance.EndTime;
                        DateTime dateTime2 = dateTime1;
                        if ((nullable.HasValue ? (nullable.GetValueOrDefault() < dateTime2 ? 1 : 0) : 0) != 0)
                            continue;
                    }
                    if (instance.StartTime.HasValue)
                    {
                        DateTime? nullable1 = instance.StartTime;
                        DateTime dateTime2 = dateTime1;
                        if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() > dateTime2 ? 1 : 0) : 0) != 0)
                        {
                            DateTime? nullable2 = instance.StartTime;
                            DateTime dateTime3 = this.nextTime;
                            if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() < dateTime3 ? 1 : 0) : 0) != 0)
                            {
                                this.nextTime = instance.StartTime.Value;
                                continue;
                            }
                            else
                                continue;
                        }
                    }
                    this.ProcessInstance(instance);
                    if (instance.EndTime.HasValue)
                    {
                        DateTime? nullable1 = instance.EndTime;
                        DateTime dateTime2 = dateTime1;
                        if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() > dateTime2 ? 1 : 0) : 0) != 0)
                        {
                            DateTime? nullable2 = instance.EndTime;
                            DateTime dateTime3 = this.nextTime;
                            if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() < dateTime3 ? 1 : 0) : 0) != 0)
                                this.nextTime = instance.EndTime.Value;
                        }
                    }
                    this.IncrementCursor(i, last);
                }
                this.EndSlice();
                dateTime1 = this.nextTime;
            }
            while (this.nextTime < DateTime.MaxValue);
        }

        private void ProcessAtLocation(int first, int last)
        {
            if (this.withTime)
            {
                this.ProcessAtLocationWithTime(first, last);
            }
            else
            {
                this.BeginSlice();
                for (int index = first; index <= last; ++index)
                    this.ProcessInstance(this.instances[index]);
                this.EndSlice();
            }
        }

        internal void Compute(int first)
        {
            this.Initialize();
            while (first < this.instances.Count)
            {
                int index = first + 1;
                while ((index < this.instances.Count) && !this.instances[index].FirstInstance)
                {
                    index++;
                }
                this.ProcessAtLocation(first, index - 1);
                first = index;
            }
        }

        protected abstract void Initialize();

        protected abstract void BeginSlice();

        protected abstract void ProcessInstance(InstanceData instance);

        protected abstract void EndSlice();
    }
}
