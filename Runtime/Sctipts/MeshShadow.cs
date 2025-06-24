using System.Collections.Generic;
using System.Linq;
using CustomUIElements.Utilities.EarcutNet;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    [UxmlElement]
    public partial class MeshShadow : VisualElement
    {
        [UxmlAttribute]
        public ShadowType ShowShadow
        {
            get => showShadow;
            set
            {
                if (showShadow != value)
                {
                    showShadow = value;
                    MarkDirtyRepaint();
                }
            }
        }

        [UxmlAttribute]
        public float ShadowScale
        {
            get => shadowScale;
            set
            {
                if (!Mathf.Approximately(shadowScale, value))
                {
                    shadowScale = value;
                    MarkDirtyRepaint();
                }
            }
        }

        [UxmlAttribute]
        public float ShadowOffsetX
        {
            get => shadowOffsetX;
            set
            {
                if (!Mathf.Approximately(shadowOffsetX, value))
                {
                    shadowOffsetX = value;
                    MarkDirtyRepaint();
                }
            }
        }

        [UxmlAttribute]
        public float ShadowOffsetY
        {
            get => shadowOffsetY;
            set
            {
                if (!Mathf.Approximately(shadowOffsetY, value))
                {
                    shadowOffsetY = value;
                    MarkDirtyRepaint();
                }
            }
        }

        [UxmlAttribute]
        public Color ShadowColor
        {
            get => shadowColor;
            set
            {
                if (shadowColor != value)
                {
                    shadowColor = value;
                    MarkDirtyRepaint();
                }
            }
        }

        [UxmlAttribute]
        public int CornerSmooth
        {
            get => cornerSmooth;
            set
            {
                if (Mathf.Approximately(cornerSmooth, value)) return;
                cornerSmooth = value;
                MarkDirtyRepaint();
            }
        }


        public Vector2[] ShadowVertexPositions { get; set; }

        private ShadowType showShadow;
        private float shadowScale;
        private float shadowOffsetX;
        private float shadowOffsetY;
        private Color shadowColor;
        private int cornerSmooth;


        public MeshShadow()
        {
            generateVisualContent += GenerateMesh;
        }

        protected virtual void GenerateMesh(MeshGenerationContext ctx)
        {
            if (ShowShadow is ShadowType.Base)
            {
                var radii = new CornerRadii(resolvedStyle.borderTopLeftRadius, resolvedStyle.borderTopRightRadius,
                    resolvedStyle.borderBottomRightRadius, resolvedStyle.borderBottomLeftRadius, cornerSmooth);
                ShadowVertexPositions = GenerateRoundedRectPath(contentRect, radii);
                PaintVerticesBasedShadow(ctx, contentRect);
            }

            if (showShadow is ShadowType.TextureBased)
            {
                PaintTexturedShadow(ctx, contentRect);
            }
        }

        protected void PaintVerticesBasedShadow(MeshGenerationContext ctx, Rect rect)
        {
            if (ShadowVertexPositions is not null && ShadowVertexPositions.Length > 2 && shadowScale > 0f &&
                shadowColor.a > 0.01f)
            {
                var center = rect.center;
                var shadowPainter = ctx.painter2D;

                shadowPainter.fillColor = shadowColor;

                shadowPainter.BeginPath();
                for (int i = 1; i < ShadowVertexPositions.Length; i++)
                {
                    // Смещение и масштаб тени
                    Vector2 pos = ShadowVertexPositions[i];
                    pos = center + (pos - center) * shadowScale;
                    pos.x += shadowOffsetX;
                    pos.y += shadowOffsetY;
                    shadowPainter.LineTo(pos);
                }

                shadowPainter.ClosePath();
                shadowPainter.Fill();
            }
        }

        protected void PaintTexturedShadow(MeshGenerationContext ctx, Rect rect)
        {
            var tex = resolvedStyle.backgroundImage.texture;
            if (tex is null) return;

            var radii = new CornerRadii(
                resolvedStyle.borderTopLeftRadius,
                resolvedStyle.borderTopRightRadius,
                resolvedStyle.borderBottomRightRadius,
                resolvedStyle.borderBottomLeftRadius,
                CornerSmooth
            );

            var shape = GenerateRoundedRectPath(rect, radii).ToList();

            // Shadow
            DrawShadowImageMesh(ctx, tex, shape, rect.center, shadowScale, shadowOffsetX, shadowOffsetY, shadowColor);

            // Image
            DrawShadowImageMesh(ctx, tex, shape, rect.center, 1.0f, 0, 0, resolvedStyle.unityBackgroundImageTintColor);
        }

        private Vector2[] GenerateRoundedRectPath(Rect rect, CornerRadii radii)
        {
            int smooth = Mathf.Max(1, radii.Smooth);

            Vector2 topLeft = rect.min + new Vector2(radii.TopLeft, radii.TopLeft);
            Vector2 topRight = new Vector2(rect.xMax - radii.TopRight, rect.yMin + radii.TopRight);
            Vector2 bottomRight = new Vector2(rect.xMax - radii.BottomRight, rect.yMax - radii.BottomRight);
            Vector2 bottomLeft = new Vector2(rect.xMin + radii.BottomLeft, rect.yMax - radii.BottomLeft);

            var path = new List<Vector2>(smooth * 4);

            // Top edge (left to right)
            if (radii.TopLeft > 0)
                AddArc(path, topLeft, radii.TopLeft, 180f, 270f, smooth);
            else
                path.Add(new Vector2(rect.xMin, rect.yMin));

            // Right edge (top to bottom)
            if (radii.TopRight > 0)
                AddArc(path, topRight, radii.TopRight, 270f, 360f, smooth);
            else
                path.Add(new Vector2(rect.xMax, rect.yMin));

            // Bottom edge (right to left)
            if (radii.BottomRight > 0)
                AddArc(path, bottomRight, radii.BottomRight, 0f, 90f, smooth);
            else
                path.Add(new Vector2(rect.xMax, rect.yMax));

            // Left edge (bottom to top)
            if (radii.BottomLeft > 0)
                AddArc(path, bottomLeft, radii.BottomLeft, 90f, 180f, smooth);
            else
                path.Add(new Vector2(rect.xMin, rect.yMax));

            return path.ToArray();
        }

        private static void AddArc(List<Vector2> path, Vector2 center, float radius, float startAngle, float endAngle,
            int segments)
        {
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                path.Add(point);
            }
        }

        public static void DrawShadowImageMesh(
            MeshGenerationContext ctx,
            Texture2D tex,
            List<Vector2> shapePoints,
            Vector2 center,
            float scale,
            float offsetX,
            float offsetY,
            Color color)
        {
            if (shapePoints == null || shapePoints.Count < 3) return;

            // Генерируем вершины
            var verts = new Vertex[shapePoints.Count];
            var rectMin = new Vector2(float.MaxValue, float.MaxValue);
            var rectMax = new Vector2(float.MinValue, float.MinValue);
            // Найти bounds для UV
            for (int i = 0; i < shapePoints.Count; i++)
            {
                if (shapePoints[i].x < rectMin.x) rectMin.x = shapePoints[i].x;
                if (shapePoints[i].y < rectMin.y) rectMin.y = shapePoints[i].y;
                if (shapePoints[i].x > rectMax.x) rectMax.x = shapePoints[i].x;
                if (shapePoints[i].y > rectMax.y) rectMax.y = shapePoints[i].y;
            }

            for (int i = 0; i < shapePoints.Count; i++)
            {
                // Масштабируем и смещаем относительно центра (для тени)
                var pos = (shapePoints[i] - center) * scale + center;
                pos.x += offsetX;
                pos.y += offsetY;

                // Вычисляем UV в прямоугольнике исходной формы
                var uv = new Vector2(
                    Mathf.InverseLerp(rectMin.x, rectMax.x, shapePoints[i].x),
                    1f - Mathf.InverseLerp(rectMin.y, rectMax.y, shapePoints[i].y)
                );

                verts[i] = new Vertex()
                {
                    position = new Vector3(pos.x, pos.y, 0),
                    tint = color,
                    uv = uv
                };
            }

            var flat = new List<double>(shapePoints.Count * 2);
            for (int i = 0; i < shapePoints.Count; i++)
            {
                flat.Add(shapePoints[i].x);
                flat.Add(shapePoints[i].y);
            }

            var hole = new List<int>(shapePoints.Count * 2);
            // Триангуляция shapePoints (можно сторонними либами, тут — EarcutLite)
            var indices = Earcut.Tessellate(flat, hole);

            // Convert indices to ushort[]
            var tris = new ushort[indices.Count];
            for (int i = 0; i < indices.Count; i++)
                tris[i] = (ushort)indices[i];

            var mesh = ctx.Allocate(verts.Length, tris.Length, tex);
            mesh.SetAllVertices(verts);
            mesh.SetAllIndices(tris);
        }

        public enum ShadowType
        {
            None,
            Base,
            VerticesBased,
            TextureBased,
        }
    }
}