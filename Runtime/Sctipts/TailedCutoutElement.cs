using UnityEditor.TerrainTools;
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

        [UxmlAttribute, Range(0f, 1f)]
        public float TailBase
        {
            get => tailBase;
            set => CompareAndWrite(ref tailBase, value);
        }
        
        public float TailBasePx { private get; set; }

        private TailSide tailSide;
        private float tailBase;

        public TailedCutoutElement()
        {
            customMesh = new TailedMesh();
            RegisterCallback<GeometryChangedEvent>(OnFirstGeometryChanged);
            return;

            void OnFirstGeometryChanged(GeometryChangedEvent evt)
            {
                UnregisterCallback<GeometryChangedEvent>(OnFirstGeometryChanged);
                if (customMesh is TailedMesh tailMesh)
                    TailBasePx = TailBase * Mathf.Min(contentRect.width, contentRect.height);
            }
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
            tailedMesh.TailBasePx = Mathf.Min(TailBasePx, contentRect.height);
            tailedMesh.TailedSide = TailedSide;
            base.DrawMeshes(ctx);
        }

        protected override void AssignCornerRadius()
        {
            var minSide = Mathf.Min(contentRect.height, contentRect.width);
            customMesh.CornerRadii = tailSide switch
            {
                TailSide.TopLeft => new CornerRadii(
                    0,
                    resolvedStyle.borderTopRightRadius,
                    resolvedStyle.borderBottomRightRadius,
                    Mathf.Min(resolvedStyle.borderBottomLeftRadius, contentRect.height - TailBasePx),
                    CornerSmooth),
                TailSide.BottomLeft => new CornerRadii(
                    Mathf.Min(resolvedStyle.borderTopLeftRadius, TailBasePx),
                    resolvedStyle.borderTopRightRadius,
                    resolvedStyle.borderBottomRightRadius,
                    0,
                    CornerSmooth),
                TailSide.TopRight => new CornerRadii(
                    resolvedStyle.borderTopLeftRadius,
                    0,
                    Mathf.Min(resolvedStyle.borderBottomRightRadius, contentRect.height - TailBasePx),
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