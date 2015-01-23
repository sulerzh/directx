// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.InstanceStreams
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class InstanceStreams : DisposableResource
  {
    private InstanceProcessingTechnique technique = new InstanceProcessingTechnique((RenderParameters) null);
    private readonly InstanceBlockQueryType[] QueryTypes = (InstanceBlockQueryType[]) Enum.GetValues(typeof (InstanceBlockQueryType));
    private readonly InstanceStreams.InstanceStreamType[] StreamTypes = (InstanceStreams.InstanceStreamType[]) Enum.GetValues(typeof (InstanceStreams.InstanceStreamType));
    private int[] transientBufferSize = new int[4];
    private Dictionary<InstanceBlockQueryType, Dictionary<InstanceStreams.InstanceStreamType, StreamBuffer>> streams;

    public InstanceStreams()
    {
      this.streams = new Dictionary<InstanceBlockQueryType, Dictionary<InstanceStreams.InstanceStreamType, StreamBuffer>>();
      foreach (InstanceBlockQueryType key1 in Enum.GetValues(typeof (InstanceBlockQueryType)))
      {
        Dictionary<InstanceStreams.InstanceStreamType, StreamBuffer> dictionary = new Dictionary<InstanceStreams.InstanceStreamType, StreamBuffer>();
        foreach (InstanceStreams.InstanceStreamType key2 in Enum.GetValues(typeof (DrawStyle)))
          dictionary.Add(key2, (StreamBuffer) null);
        this.streams.Add(key1, dictionary);
      }
    }

    public StreamBuffer GetStream(InstanceBlockQueryType InstanceBlockQueryType, DrawStyle drawStyle, bool hitTest)
    {
      InstanceStreams.InstanceStreamType index;
      if (hitTest)
      {
        switch (drawStyle)
        {
          case DrawStyle.HitTest:
            index = InstanceStreams.InstanceStreamType.ColorHitTest;
            break;
          case DrawStyle.AnnotationHitTest:
            index = InstanceStreams.InstanceStreamType.AnnotationHitTest;
            break;
          default:
            index = InstanceStreams.InstanceStreamType.ColorHitTest;
            break;
        }
      }
      else
      {
        switch (drawStyle)
        {
          case DrawStyle.Color:
          case DrawStyle.HitTest:
          case DrawStyle.Shadow:
            index = InstanceStreams.InstanceStreamType.Color;
            break;
          case DrawStyle.Selection:
          case DrawStyle.NegativeSelection:
            index = InstanceStreams.InstanceStreamType.Outline;
            break;
          case DrawStyle.Annotation:
          case DrawStyle.AnnotationHitTest:
            index = InstanceStreams.InstanceStreamType.Annotation;
            break;
          default:
            index = InstanceStreams.InstanceStreamType.Color;
            break;
        }
      }
      return this.streams[InstanceBlockQueryType][index];
    }

    public void EnsureStreamBuffers(float visualTimeScale, InstanceBlock instanceBlock, bool showOnlyMaxValues, DrawMode drawMode)
    {
      int val2_1 = instanceBlock.QueryInstanceCount(visualTimeScale, showOnlyMaxValues, InstanceBlockQueryType.PositiveInstances);
      int val2_2 = instanceBlock.QueryInstanceCount(visualTimeScale, showOnlyMaxValues, InstanceBlockQueryType.NegativeInstances);
      int val2_3 = instanceBlock.QueryInstanceCount(visualTimeScale, showOnlyMaxValues, InstanceBlockQueryType.ZeroInstances);
      int val2_4 = instanceBlock.QueryInstanceCount(visualTimeScale, showOnlyMaxValues, InstanceBlockQueryType.NullInstances);
      this.transientBufferSize[0] = val2_1 == 0 ? 0 : Math.Max(128, val2_1);
      this.transientBufferSize[1] = val2_2 == 0 ? 0 : Math.Max(128, val2_2);
      this.transientBufferSize[2] = val2_3 == 0 ? 0 : Math.Max(128, val2_3);
      this.transientBufferSize[3] = val2_4 == 0 ? 0 : Math.Max(128, val2_4);
      bool getDataEnabled = false;
      for (int index1 = 0; index1 < this.QueryTypes.Length; ++index1)
      {
        InstanceBlockQueryType index2 = this.QueryTypes[index1];
        int num;
        switch (index2)
        {
          case InstanceBlockQueryType.NullInstances:
          case InstanceBlockQueryType.ZeroInstances:
            num = 0;
            break;
          default:
            num = (drawMode & DrawMode.PieChart) > DrawMode.None ? 1 : 0;
            break;
        }
        this.technique.Mode = num != 0 ? InstanceProcessingTechniqueMode.Pie : InstanceProcessingTechniqueMode.Default;
        StreamBuffer streamBuffer1 = this.streams[index2][InstanceStreams.InstanceStreamType.Color];
        if (streamBuffer1 != null && (!streamBuffer1.VertexFormat.Equals(this.technique.OutputVertexFormat) || streamBuffer1.VertexCount < this.transientBufferSize[(int) index2]))
        {
          for (int index3 = 0; index3 < this.StreamTypes.Length; ++index3)
          {
            InstanceStreams.InstanceStreamType index4 = this.StreamTypes[index3];
            StreamBuffer streamBuffer2 = this.streams[index2][index4];
            if (streamBuffer2 != null)
            {
              streamBuffer2.Dispose();
              this.streams[index2][index4] = (StreamBuffer) null;
            }
          }
        }
      }
      VertexFormat vertexFormat = (VertexFormat) null;
      for (int index1 = 0; index1 < this.QueryTypes.Length; ++index1)
      {
        InstanceBlockQueryType index2 = this.QueryTypes[index1];
        int num;
        switch (index2)
        {
          case InstanceBlockQueryType.NullInstances:
          case InstanceBlockQueryType.ZeroInstances:
            num = 0;
            break;
          default:
            num = (drawMode & DrawMode.PieChart) > DrawMode.None ? 1 : 0;
            break;
        }
        this.technique.Mode = num != 0 ? InstanceProcessingTechniqueMode.Pie : InstanceProcessingTechniqueMode.Default;
        for (int index3 = 0; index3 < this.StreamTypes.Length; ++index3)
        {
          InstanceStreams.InstanceStreamType index4 = this.StreamTypes[index3];
          if (this.streams[index2][index4] == null && this.transientBufferSize[(int) index2] > 0)
          {
            if (vertexFormat == null)
              vertexFormat = VertexFormat.Create(new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4));
            VertexFormat format = index4 == InstanceStreams.InstanceStreamType.ColorHitTest || index4 == InstanceStreams.InstanceStreamType.AnnotationHitTest ? vertexFormat : this.technique.OutputVertexFormat;
            this.streams[index2][index4] = StreamBuffer.Create(format, this.transientBufferSize[(int) index2], getDataEnabled);
          }
        }
      }
    }

    public void ClearStreams(Renderer renderer)
    {
      foreach (Dictionary<InstanceStreams.InstanceStreamType, StreamBuffer> dictionary in this.streams.Values)
      {
        foreach (StreamBuffer streamBuffer in dictionary.Values)
        {
          if (streamBuffer != null)
          {
            using (VertexBuffer vertexBuffer = streamBuffer.PeekVertexBuffer())
            {
              if (vertexBuffer != null)
              {
                vertexBuffer.Zero();
                renderer.SetVertexSource(vertexBuffer);
                renderer.SetVertexSource((VertexBuffer) null);
              }
            }
          }
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      foreach (Dictionary<InstanceStreams.InstanceStreamType, StreamBuffer> dictionary in this.streams.Values)
      {
        foreach (StreamBuffer streamBuffer in dictionary.Values)
        {
          if (streamBuffer != null)
            streamBuffer.Dispose();
        }
      }
      this.technique.Dispose();
    }

    private enum InstanceStreamType
    {
      Color,
      Outline,
      Annotation,
      ColorHitTest,
      AnnotationHitTest,
    }
  }
}
