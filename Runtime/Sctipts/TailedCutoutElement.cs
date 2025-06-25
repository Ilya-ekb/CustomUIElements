using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    [UxmlElement]
    public partial class TailedCutoutElement : TriangleCutoutElement
    {
        [UxmlAttribute]
        public TailSide TailedSide
        {
            get => tailSide;
            set => CompareAndWrite(ref tailSide, value);
        }

        [UxmlAttribute]
        public float TailBasePx
        {
            get => tailBasePx;
            set => CompareAndWrite(ref tailBasePx, value);
        }

        [UxmlAttribute]
        public float TailDepthPx
        {
            get => tailDepth;
            set => CompareAndWrite(ref tailDepth, value);
        }

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

        protected override void PaintVerticesBasedShadow(MeshGenerationContext ctx, Rect rect)
        {
            if (ShadowVertexPositions is not null && ShadowVertexPositions.Length > 2 && ShadowScale > 0f &&
                ShadowColor.a > 0.01f)
            {
                var center = rect.center;
                var shadowPainter = ctx.painter2D;

                shadowPainter.fillColor = ShadowColor;
                var startPoint = ShadowVertexPositions[1];
                startPoint = center + (startPoint - center) * ShadowScale;
                startPoint.x += ShadowOffsetX;
                startPoint.y += ShadowOffsetY;

                shadowPainter.BeginPath();
                shadowPainter.MoveTo(startPoint);
                for (int i = 2; i < ShadowVertexPositions.Length - 3; i++)
                    shadowPainter.LineTo(ComputeShadowPointPosition(ShadowVertexPositions[i], center));


                var aPoint = ComputeShadowPointPosition(ShadowVertexPositions[^3], center);
                var bPoint = ComputeShadowPointPosition(ShadowVertexPositions[^2], center);
                var cPoint = ComputeShadowPointPosition(ShadowVertexPositions[^1], center);
                shadowPainter.MoveTo(aPoint);
                shadowPainter.LineTo(bPoint);
                shadowPainter.LineTo(cPoint);

                shadowPainter.ClosePath();
                shadowPainter.Fill();
            }
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