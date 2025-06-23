using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    public abstract class BaseCustomElement : VisualElement
    {
        protected BaseCustomElement()
        {
            generateVisualContent += GenerateMesh;
        }

        /// <summary>
        /// Returns the main contour (outline) of the shape for border generation.
        /// Override for custom shapes.
        /// </summary>
        protected virtual List<Vector2> GetShapeOutline(Rect rect)
        {
            // Default: rectangle with radius (not true ellipse, but four points for corners)
            // For full border-radius support, add arcs (see earlier answers)
            return new List<Vector2>
            {
                new Vector2(rect.xMin, rect.yMin),
                new Vector2(rect.xMax, rect.yMin),
                new Vector2(rect.xMax, rect.yMax),
                new Vector2(rect.xMin, rect.yMax)
            };
        }

        protected virtual void FillShape(MeshGenerationContext ctx, List<Vector2> outline)
        {
            Color fillColor = resolvedStyle.backgroundColor;
            Texture2D fillTexture = resolvedStyle.backgroundImage.texture;
            int n = outline.Count;
            if (n < 3) return;
            var mesh = ctx.Allocate(n, (n - 2) * 3, fillTexture);
            for (int i = 0; i < n; i++)
                mesh.SetNextVertex(new Vertex
                {
                    position = outline[i],
                    tint = fillColor,
                    uv = fillTexture ? new Vector2(outline[i].x / contentRect.width, 1f - outline[i].y / contentRect.height) : Vector2.zero
                });
            for (int i = 1; i < n - 1; i++)
            {
                mesh.SetNextIndex((ushort)0);
                mesh.SetNextIndex((ushort)i);
                mesh.SetNextIndex((ushort)(i + 1));
            }
        }

        protected virtual void GenerateMesh(MeshGenerationContext ctx)
        {
            var rect = contentRect;
            if (rect.width <= 1 || rect.height <= 1) return;
            var outline = GetShapeOutline(rect);
            DrawBorderForOutline(ctx, outline);
        }

        /// <summary>
        /// Draws border mesh along the given outline.
        /// </summary>
        protected void DrawBorderForOutline(MeshGenerationContext ctx, List<Vector2> outline)
        {
            float borderWidth = resolvedStyle.borderLeftWidth; // For simplicity, use one width
            Color borderColor = resolvedStyle.borderLeftColor;
            int n = outline.Count;
            if (n < 3 || borderWidth <= 0) return;

            // Inner outline (shrinked inside by borderWidth, per-vertex normal)
            List<Vector2> inner = new List<Vector2>(n);
            for (int i = 0; i < n; i++)
            {
                Vector2 prev = outline[(i - 1 + n) % n];
                Vector2 next = outline[(i + 1) % n];
                Vector2 tangent = (next - prev).normalized;
                Vector2 normal = new Vector2(-tangent.y, tangent.x);
                inner.Add(outline[i] + normal * borderWidth);
            }

            // Allocate mesh
            var mesh = ctx.Allocate(n * 2, n * 6);
            // Outer
            for (int i = 0; i < n; i++)
                mesh.SetNextVertex(new Vertex { position = outline[i], tint = borderColor });
            // Inner
            for (int i = 0; i < n; i++)
                mesh.SetNextVertex(new Vertex { position = inner[i], tint = borderColor });

            // Triangles
            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                mesh.SetNextIndex((ushort)i);
                mesh.SetNextIndex((ushort)next);
                mesh.SetNextIndex((ushort)(n + next));

                mesh.SetNextIndex((ushort)i);
                mesh.SetNextIndex((ushort)(n + next));
                mesh.SetNextIndex((ushort)(n + i));
            }
        }
    }
}
