using System.Collections.Generic;
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
            get => tailBasePx;
            set => CompareAndWrite(ref tailBasePx, value);
        }

        public float TailDepthPx
        {
            get => tailDepth;
            set => CompareAndWrite(ref tailDepth, value);
        }

        private TailedCutoutElement.TailSide tailSide;
        private float tailBasePx;
        private float tailDepth;

        public override void UpdateMesh()
        {
            if (!isDirty)
                return;

            var w = Width;
            var h = Height;
            var radii = CornerRadii;

            w -= TailDepthPx;
            var contour = CutoutMeshBuilder.GenerateCutoutContour(w, h, radii, СutoutSide, BaseSize, CutoutOffset, Depth);
            
            if (TailedSide is TailedCutoutElement.TailSide.BottomLeft or TailedCutoutElement.TailSide.TopLeft)
                for (var index = 0; index < contour.Count; index++)
                {
                    var c = contour[index];
                    c.x += TailDepthPx;
                    contour[index] = c;
                }

            CutoutMeshBuilder.BuildMesh(contour, Width, Height, TintColor, out var vertices, out var indices);
            var aPoint = tailSide switch
            {
                TailedCutoutElement.TailSide.TopLeft => new Vector2(TailDepthPx, TailBasePx),
                TailedCutoutElement.TailSide.TopRight => new Vector2(Width - TailDepthPx, 0),
                TailedCutoutElement.TailSide.BottomRight => new Vector2(Width - TailDepthPx, Height - TailBasePx),
                TailedCutoutElement.TailSide.BottomLeft => new Vector2(TailDepthPx, Height),
                _ => default
            };
            var bPoint = tailSide switch
            {
                TailedCutoutElement.TailSide.TopLeft => new Vector2(0, 0),
                TailedCutoutElement.TailSide.TopRight => new Vector2(Width, 0),
                TailedCutoutElement.TailSide.BottomRight => new Vector2(Width, Height),
                TailedCutoutElement.TailSide.BottomLeft => new Vector2(0, Height),
                _ => default
            };
            var cPoint = tailSide switch
            {
                TailedCutoutElement.TailSide.TopLeft => new Vector2(TailDepthPx, 0),
                TailedCutoutElement.TailSide.TopRight => new Vector2(Width - TailDepthPx, TailBasePx),
                TailedCutoutElement.TailSide.BottomRight => new Vector2(Width - TailDepthPx, Height),
                TailedCutoutElement.TailSide.BottomLeft => new Vector2(TailDepthPx, Height - TailBasePx),
                _ => default
            };
            ushort aIndex = (ushort)(vertices.Count);
            ushort bIndex = (ushort)(aIndex + 1);
            ushort cIndex = (ushort)(aIndex + 2);
            vertices.Add(new Vertex
            {
                position = aPoint,
                tint = TintColor,
                uv = new Vector2(aPoint.x / Width, 1f - aPoint.y / Height)
            });

            vertices.Add(new Vertex
            {
                position = bPoint,
                tint = TintColor,
                uv = new Vector2(bPoint.x / Width, 1f - bPoint.y / Height)
            });

            vertices.Add(new Vertex
            {
                position = cPoint,
                tint = TintColor,
                uv = new Vector2(cPoint.x / Width, 1f - cPoint.y / Height)
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