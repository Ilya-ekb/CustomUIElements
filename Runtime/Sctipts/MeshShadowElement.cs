using System.Collections.Generic;
using System.Linq;
using CustomUIElements.Utilities.EarcutNet;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    [UxmlElement]
    public partial class MeshShadowElement : VisualElement

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
        public Color TintColor
        {
            get => tintColor;
            set
            {
                if (tintColor != value)
                {
                    tintColor = value;
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


        [UxmlAttribute]
        public bool DebugState
        {
            get => debugState;
            set
            {
                if (debugState != value)
                {
                    debugState = value;
                    MarkDirtyRepaint();
                }
            }
        }


        public Vector2[] ShadowVertexPositions { get; set; }

        protected CustomMesh customMesh;
        private ShadowType showShadow;
        private float shadowScale = 1;
        private float shadowOffsetX = 4.0f;
        private float shadowOffsetY = 4.0f;
        private Color shadowColor = new(0, 0, 0, 0.5f);
        private int cornerSmooth = 4;
        private bool debugState;
        private Color tintColor;


        public MeshShadowElement()
        {
            generateVisualContent += GenerateMesh;
            pickingMode = PickingMode.Position;
            style.backgroundColor = Color.clear;
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
                PaintTexturedShadow(ctx, contentRect);

            var element = GetMeshElement(ctx);
            if (element is null || customMesh is null) return;
            element.DrawMeshes(ctx);
            AllocateMesh(ctx);
            DrawDebug(ctx);
        }

        protected virtual MeshShadowElement GetMeshElement(MeshGenerationContext ctx)
        {
            return (MeshShadowElement)ctx.visualElement;
        }

        protected virtual CustomMesh AssignMesh()
        {
            return null;
        }

        protected virtual void DrawMeshes(MeshGenerationContext ctx)
        {
            if (customMesh is null) return;
            customMesh.Width = contentRect.width;
            customMesh.Height = contentRect.height;
            customMesh.Texture = resolvedStyle.backgroundImage.texture;
            customMesh.TintColor = TintColor;
            AssignCornerRadius();
            customMesh.UpdateMesh();

            if (ShowShadow is ShadowType.VerticesBased)
            {
                ShadowVertexPositions = new Vector2[customMesh.Vertices.Length];
                for (var index = 0; index < customMesh.Vertices.Length; index++)
                    ShadowVertexPositions[index] = customMesh.Vertices[index].position;

                PaintVerticesBasedShadow(ctx, contentRect);
            }
        }

        protected virtual void AssignCornerRadius()
        {
            customMesh.CornerRadii = new CornerRadii(
                resolvedStyle.borderTopLeftRadius,
                resolvedStyle.borderTopRightRadius,
                resolvedStyle.borderBottomRightRadius,
                resolvedStyle.borderBottomLeftRadius,
                CornerSmooth);
        }

        private void AllocateMesh(MeshGenerationContext ctx)
        {
            var mesh = ctx.Allocate(customMesh.Vertices.Length, customMesh.Indices.Length, customMesh.Texture);
            if (customMesh.Vertices is not null && customMesh.Vertices.Length > 2)
            {
                mesh.SetAllVertices(customMesh.Vertices);
                mesh.SetAllIndices(customMesh.Indices);
            }
        }

        protected virtual void PaintVerticesBasedShadow(MeshGenerationContext ctx, Rect rect)
        {
            if (ShadowVertexPositions is not null && ShadowVertexPositions.Length > 2 && shadowScale > 0f &&
                shadowColor.a > 0.01f)
            {
                var center = rect.center;
                var shadowPainter = ctx.painter2D;

                shadowPainter.fillColor = ShadowColor;
                var startPoint = ShadowVertexPositions[1];
                startPoint = center + (startPoint - center) * shadowScale;
                startPoint.x += shadowOffsetX;
                startPoint.y += shadowOffsetY;

                shadowPainter.BeginPath();
                shadowPainter.MoveTo(startPoint);
                
                for (var i = 2; i < ShadowVertexPositions.Length; i++)
                    shadowPainter.LineTo(ComputeShadowPointPosition(ShadowVertexPositions[i], center));

                shadowPainter.MoveTo(startPoint);
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

        protected void CompareAndWrite<T>(ref T field, T newValue)
        {
            if (field.Equals(newValue)) return;
            field = newValue;
            MarkDirtyRepaint();
        }

        protected Vector2 ComputeShadowPointPosition(in Vector2 inPos, Vector2 center)
        {
            var result = center + (inPos - center) * ShadowScale;
            result.x += ShadowOffsetX;
            result.y += ShadowOffsetY;
            return result;
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

        private static void DrawShadowImageMesh(
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


            var verts = new Vertex[shapePoints.Count];
            var rectMin = new Vector2(float.MaxValue, float.MaxValue);
            var rectMax = new Vector2(float.MinValue, float.MinValue);

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

            var indices = Earcut.Tessellate(flat, hole);

            // Convert indices to ushort[]
            var tris = new ushort[indices.Count];
            for (int i = 0; i < indices.Count; i++)
                tris[i] = (ushort)indices[i];

            var mesh = ctx.Allocate(verts.Length, tris.Length, tex);
            mesh.SetAllVertices(verts);
            mesh.SetAllIndices(tris);
        }

        private void DrawDebug(MeshGenerationContext ctx)
        {
            if (!DebugState) return;
            var p2d = ctx.painter2D;

            var verts = customMesh.Vertices;
            var inds = customMesh.Indices;
            p2d.strokeColor = Color.red;
            p2d.lineWidth = 1.5f;
            for (int i = 0; i < inds.Length; i += 3)
            {
                var a = verts[inds[i]].position;
                var b = verts[inds[i + 1]].position;
                var c = verts[inds[i + 2]].position;
                p2d.BeginPath();
                p2d.MoveTo(a);
                p2d.LineTo(b);
                p2d.LineTo(c);
                p2d.LineTo(a);
                p2d.ClosePath();
                p2d.Stroke();
            }

            for (int i = 0; i < verts.Length; i++)
            {
                var pos = verts[i].position;
                p2d.fillColor = i == 1 ? Color.green :
                    i == verts.Length - 1 ? Color.black :
                    i == verts.Length - 2 ? Color.red :
                    i == verts.Length - 3 ? Color.yellow : Color.blue;
                p2d.BeginPath();
                p2d.MoveTo(pos + new Vector3(3, 0));
                var radius = i == 1 ? 6 : 4;
                p2d.Arc(pos, radius, 0, 360);
                p2d.ClosePath();
                p2d.Fill();
            }
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