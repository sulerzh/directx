using Microsoft.Data.Visualization.Engine.Graphics;

namespace Microsoft.Data.Visualization.Engine
{
    internal interface IAnnotatable
    {
        AnnotationStyle Style { get; set; }

        bool IsAnnotationDirty { get; }

        void SetAnnotationImageSource(IAnnotationImageSource imageSource);

        void DrawAnnotation(Renderer renderer, SceneState state, bool blockUntilComplete);

        void DrawAnnotationHitTest(Renderer renderer, SceneState state, DepthStencilState depthStencil, BlendState blendState, RasterizerState rasterizer);
    }
}
