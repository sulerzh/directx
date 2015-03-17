using Microsoft.Data.Visualization.Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
    internal class InstanceStreams : DisposableResource
    {
        private InstanceProcessingTechnique technique = new InstanceProcessingTechnique(null);
        private readonly InstanceBlockQueryType[] QueryTypes = (InstanceBlockQueryType[])Enum.GetValues(typeof(InstanceBlockQueryType));
        private readonly InstanceStreamType[] StreamTypes = (InstanceStreamType[])Enum.GetValues(typeof(InstanceStreamType));
        private int[] transientBufferSize = new int[4];
        private Dictionary<InstanceBlockQueryType, Dictionary<InstanceStreamType, StreamBuffer>> streams;

        public InstanceStreams()
        {
            streams = new Dictionary<InstanceBlockQueryType, Dictionary<InstanceStreamType, StreamBuffer>>();
            foreach (InstanceBlockQueryType queryType in Enum.GetValues(typeof(InstanceBlockQueryType)))
            {
                Dictionary<InstanceStreamType, StreamBuffer> dictionary = new Dictionary<InstanceStreamType, StreamBuffer>();
                foreach (InstanceStreamType streamType in Enum.GetValues(typeof(DrawStyle)))
                    dictionary.Add(streamType, null);
                streams.Add(queryType, dictionary);
            }
        }

        public StreamBuffer GetStream(InstanceBlockQueryType InstanceBlockQueryType, DrawStyle drawStyle, bool hitTest)
        {
            InstanceStreamType index;
            if (hitTest)
            {
                switch (drawStyle)
                {
                    case DrawStyle.HitTest:
                        index = InstanceStreamType.ColorHitTest;
                        break;
                    case DrawStyle.AnnotationHitTest:
                        index = InstanceStreamType.AnnotationHitTest;
                        break;
                    default:
                        index = InstanceStreamType.ColorHitTest;
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
                        index = InstanceStreamType.Color;
                        break;
                    case DrawStyle.Selection:
                    case DrawStyle.NegativeSelection:
                        index = InstanceStreamType.Outline;
                        break;
                    case DrawStyle.Annotation:
                    case DrawStyle.AnnotationHitTest:
                        index = InstanceStreamType.Annotation;
                        break;
                    default:
                        index = InstanceStreamType.Color;
                        break;
                }
            }
            return streams[InstanceBlockQueryType][index];
        }

        public void EnsureStreamBuffers(float visualTimeScale, InstanceBlock instanceBlock, bool showOnlyMaxValues, DrawMode drawMode)
        {
            int positiveCount = instanceBlock.QueryInstanceCount(visualTimeScale, showOnlyMaxValues, InstanceBlockQueryType.PositiveInstances);
            int negativeCount = instanceBlock.QueryInstanceCount(visualTimeScale, showOnlyMaxValues, InstanceBlockQueryType.NegativeInstances);
            int zeroCount = instanceBlock.QueryInstanceCount(visualTimeScale, showOnlyMaxValues, InstanceBlockQueryType.ZeroInstances);
            int nullCount = instanceBlock.QueryInstanceCount(visualTimeScale, showOnlyMaxValues, InstanceBlockQueryType.NullInstances);
            transientBufferSize[0] = positiveCount == 0 ? 0 : Math.Max(128, positiveCount);
            transientBufferSize[1] = negativeCount == 0 ? 0 : Math.Max(128, negativeCount);
            transientBufferSize[2] = zeroCount == 0 ? 0 : Math.Max(128, zeroCount);
            transientBufferSize[3] = nullCount == 0 ? 0 : Math.Max(128, nullCount);
            bool getDataEnabled = false;
            for (int i = 0; i < QueryTypes.Length; ++i)
            {
                InstanceBlockQueryType queryType = QueryTypes[i];
                if (queryType != InstanceBlockQueryType.NullInstances &&
                    queryType != InstanceBlockQueryType.ZeroInstances &&
                    (drawMode & DrawMode.PieChart) > DrawMode.None)
                {
                    this.technique.Mode = InstanceProcessingTechniqueMode.Pie;
                }
                else
                {
                    this.technique.Mode = InstanceProcessingTechniqueMode.Default;
                }
                StreamBuffer colorBuffer = streams[queryType][InstanceStreamType.Color];
                if (colorBuffer != null && 
                    (!colorBuffer.VertexFormat.Equals(technique.OutputVertexFormat) ||
                    colorBuffer.VertexCount < transientBufferSize[(int)queryType]))
                {
                    for (int j = 0; j < StreamTypes.Length; ++j)
                    {
                        InstanceStreamType streamType = StreamTypes[j];
                        StreamBuffer instanceBuffer = streams[queryType][streamType];
                        if (instanceBuffer != null)
                        {
                            instanceBuffer.Dispose();
                            streams[queryType][streamType] = null;
                        }
                    }
                }
            }
            VertexFormat vertexFormat = null;
            for (int i = 0; i < QueryTypes.Length; ++i)
            {
                InstanceBlockQueryType queryType = QueryTypes[i];

                if (queryType != InstanceBlockQueryType.NullInstances &&
                    queryType != InstanceBlockQueryType.ZeroInstances &&
                    (drawMode & DrawMode.PieChart) > DrawMode.None)
                {
                    this.technique.Mode = InstanceProcessingTechniqueMode.Pie;
                }
                else
                {
                    this.technique.Mode = InstanceProcessingTechniqueMode.Default;
                }

                for (int j = 0; j < StreamTypes.Length; ++j)
                {
                    InstanceStreamType streamType = StreamTypes[j];
                    if (streams[queryType][streamType] == null && transientBufferSize[(int)queryType] > 0)
                    {
                        if (vertexFormat == null)
                            vertexFormat = VertexFormat.Create(new VertexComponent(VertexSemantic.Color, VertexComponentDataType.Float4));
                        VertexFormat format = streamType == InstanceStreamType.ColorHitTest || streamType == InstanceStreamType.AnnotationHitTest ? vertexFormat : technique.OutputVertexFormat;
                        streams[queryType][streamType] = StreamBuffer.Create(format, transientBufferSize[(int)queryType], getDataEnabled);
                    }
                }
            }
        }

        public void ClearStreams(Renderer renderer)
        {
            foreach (Dictionary<InstanceStreamType, StreamBuffer> dictionary in streams.Values)
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
                                renderer.SetVertexSource((VertexBuffer)null);
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
            foreach (Dictionary<InstanceStreamType, StreamBuffer> dictionary in streams.Values)
            {
                foreach (StreamBuffer streamBuffer in dictionary.Values)
                {
                    if (streamBuffer != null)
                        streamBuffer.Dispose();
                }
            }
            technique.Dispose();
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
