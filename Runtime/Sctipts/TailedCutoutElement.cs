using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    [UxmlElement]
    public partial class TailedCutoutElement : TriangleCutoutElement
    {
        [UxmlAttribute]
        public TailSide TailedSide{ get => tailSide; set => CompareAndWrite(ref tailSide, value); }
        [UxmlAttribute]
        public float TailBasePx{ get => tailBasePx; set => CompareAndWrite(ref tailBasePx, value); }
        [UxmlAttribute]
        public float TailDepthPx{ get => tailDepth; set => CompareAndWrite(ref tailDepth, value); }

        private TailSide tailSide;
        private float tailBasePx;
        private float tailDepth;

        public TailedCutoutElement()
        {
            customMesh = new TailedMesh();
        }

        protected override MeshShadowElement GetMeshElement(MeshGenerationContext ctx)
        {
            return (TailedCutoutElement)ctx.visualElement;
        }

        protected override CustomMesh AssignMesh()
        {
            return new TailedMesh();
        }

        protected override void DrawMeshes(MeshGenerationContext ctx)
        {
            customMesh ??= AssignMesh();
            var tailedMesh = (TailedMesh)customMesh;
            tailedMesh.TailBasePx = tailBasePx;
            tailedMesh.TailedSide = TailedSide;
            tailedMesh.TailDepthPx = TailDepthPx;
            base.DrawMeshes(ctx);
        }

        protected override void AssignCornerRadius()
        {
            customMesh.CornerRadii = tailSide switch
            {
                TailSide.TopLeft => new CornerRadii(
                    0,
                    resolvedStyle.borderTopRightRadius,
                    resolvedStyle.borderBottomRightRadius,
                    resolvedStyle.borderBottomLeftRadius,
                    CornerSmooth),
                TailSide.BottomLeft => new CornerRadii(
                    resolvedStyle.borderTopLeftRadius,
                    resolvedStyle.borderTopRightRadius,
                    resolvedStyle.borderBottomRightRadius,
                    0,
                    CornerSmooth),
                TailSide.TopRight => new CornerRadii(
                    resolvedStyle.borderTopLeftRadius,
                    0,
                    resolvedStyle.borderBottomRightRadius,
                    resolvedStyle.borderBottomLeftRadius,
                    CornerSmooth),
                TailSide.BottomRight => new CornerRadii(
                    resolvedStyle.borderTopLeftRadius,
                    resolvedStyle.borderTopRightRadius,
                    0,
                    resolvedStyle.borderBottomLeftRadius,
                    CornerSmooth),
                _ => new CornerRadii(
                    resolvedStyle.borderTopLeftRadius,
                    resolvedStyle.borderTopRightRadius,
                    resolvedStyle.borderBottomRightRadius,
                    resolvedStyle.borderBottomLeftRadius,
                    CornerSmooth)
            };
        }
        
        

        public enum TailSide
        {
            None = 0,
            TopLeft = 1,
            TopRight = 2,
            BottomLeft = 3,
            BottomRight = 4,
        }
    }
}