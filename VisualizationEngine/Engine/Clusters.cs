// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.Clusters
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class Clusters
  {
    private List<Cluster> data = new List<Cluster>();
    private List<InstanceData> instanceList;

    internal int Count
    {
      get
      {
        return this.data.Count;
      }
    }

    internal Cluster this[int i]
    {
      get
      {
        return this.data[i];
      }
    }

    internal Clusters(List<InstanceData> instanceList)
    {
      this.instanceList = instanceList;
    }

    internal void SetInstanceList(List<InstanceData> instanceList)
    {
      this.instanceList = instanceList;
    }

    internal void AddCluster(int instanceCount)
    {
      if (this.data.Count == 0)
      {
        this.data.Add(new Cluster(0, instanceCount));
      }
      else
      {
        Cluster cluster = this.data[this.data.Count - 1];
        this.data.Add(new Cluster(cluster.First + cluster.Count, instanceCount));
      }
    }

    internal int InstanceCount(int fromCluster, int toCluster)
    {
      int num = 0;
      for (int index = fromCluster; index < toCluster; ++index)
        num += this.data[index].Count;
      return num;
    }

    internal void Reset()
    {
      this.data.Clear();
    }

    internal void PartitionByMedian(int firstCluster, int clusterCount, int split, int direction)
    {
      int num1 = firstCluster;
      int num2 = num1 + clusterCount - 1;
      while (num1 < num2)
      {
        double location = this.GetLocation(split, direction);
        int i = num1;
        int num3 = num2;
        do
        {
          while (this.GetLocation(i, direction) < location)
            ++i;
          while (location < this.GetLocation(num3, direction))
            --num3;
          if (i <= num3)
          {
            this.Swap(i, num3);
            ++i;
            --num3;
          }
        }
        while (i <= num3);
        if (num3 < split)
          num1 = i;
        if (split < i)
          num2 = num3;
      }
    }

    private double GetLocation(int i, int direction)
    {
      if (direction == 0)
        return this.instanceList[this.data[i].First].Location.Longitude;
      else
        return this.instanceList[this.data[i].First].Location.Latitude;
    }

    private void Swap(int i, int j)
    {
      Cluster cluster = this.data[i];
      this.data[i] = this.data[j];
      this.data[j] = cluster;
    }

    internal Box2D GetBounds(int firstCluster, int lastCluster)
    {
      Box2D box2D = new Box2D();
      box2D.Initialize();
      for (int index = firstCluster; index <= lastCluster; ++index)
      {
        Coordinates coordinates = this.instanceList[this.data[index].First].Location;
        box2D.UpdateWith(coordinates.Longitude, coordinates.Latitude);
      }
      return box2D;
    }
  }
}
