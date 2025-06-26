using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    public class TailedMesh : CutoutMesh
    {
        public TailedCutoutElement.TailSide TailedSide
        {
            get => tailSide;
            set => CompareAndWrite(ref tailSide, value);
        }

        public float TailBasePx
        {
            private get => tailBase;
            set => CompareAndWrite(ref tailBase, value);
        }
        
        private TailedCutoutElement.TailSide tailSide;
        private float tailBase;
        private float tailDepth;

        public override void UpdateMesh()
        {
            if (!isDirty)
                return;

            var w = Width;
            var h = Height;
            var radii = CornerRadii;

            float tailBasePx = TailBasePx;
            float tailDepthPx = TailBasePx;

            float baseW = w, baseH = h;
            if (TailedSide is TailedCutoutElement.TailSide.BottomLeft or TailedCutoutElement.TailSide.TopLeft)
                baseW -= tailDepthPx;
            else if (TailedSide is TailedCutoutElement.TailSide.TopRight or TailedCutoutElement.TailSide.BottomRight)
                baseW -= tailDepthPx;

            var contour = CutoutMeshBuilder.GenerateCutoutContour(baseW, baseH, radii, СutoutSide, BaseSize, CutoutOffset, Depth);

            if (TailedSide is TailedCutoutElement.TailSide.BottomLeft or TailedCutoutElement.TailSide.TopLeft)
                for (var index = 0; index < contour.Count; index++)
                {
                    var c = contour[index];
                    c.x += tailDepthPx;
                    contour[index] = c;
                }

            CutoutMeshBuilder.BuildMesh(contour, w, h, TintColor, out var vertices, out var indices);

            Vector2 aPoint = default, bPoint = default, cPoint = default;

            switch (TailedSide)
            {
                case TailedCutoutElement.TailSide.TopLeft:
                    aPoint = new Vector2(tailDepthPx, tailBasePx);
                    bPoint = new Vector2(0, 0);
                    cPoint = new Vector2(tailDepthPx, 0);
                    break;
                case TailedCutoutElement.TailSide.TopRight:
                    aPoint = new Vector2(w - tailDepthPx, 0);
                    bPoint = new Vector2(w, 0);
                    cPoint = new Vector2(w - tailDepthPx, tailBasePx);
                    break;
                case TailedCutoutElement.TailSide.BottomRight:
                    aPoint = new Vector2(w - tailDepthPx, h);
                    bPoint = new Vector2(w, h);
                    cPoint = new Vector2(w - tailDepthPx, h - tailBasePx);
                    break;
                case TailedCutoutElement.TailSide.BottomLeft:
                    aPoint = new Vector2(tailDepthPx, h - tailBasePx);
                    bPoint = new Vector2(0, h);
                    cPoint = new Vector2(tailDepthPx, h);
                    break;
            }

            ushort aIndex = (ushort)(vertices.Count);
            ushort bIndex = (ushort)(aIndex + 1);
            ushort cIndex = (ushort)(aIndex + 2);

            vertices.Add(new Vertex
            {
                position = aPoint,
                tint = TintColor,
                uv = new Vector2(aPoint.x / w, 1f - aPoint.y / h)
            });

            vertices.Add(new Vertex
            {
                position = bPoint,
                tint = TintColor,
                uv = new Vector2(bPoint.x / w, 1f - bPoint.y / h)
            });

            vertices.Add(new Vertex
            {
                position = cPoint,
                tint = TintColor,
                uv = new Vector2(cPoint.x / w, 1f - cPoint.y / h)
            });

            indices.Add(aIndex);
            indices.Add(bIndex);
            indices.Add(cIndex);

            Vertices = vertices.ToArray();
            Indices = indices.ToArray();

            isDirty = false;
        }
    }
}