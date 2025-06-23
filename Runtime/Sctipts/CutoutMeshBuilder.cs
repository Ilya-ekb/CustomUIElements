using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    public static class CutoutMeshBuilder
    {
        private static ushort bIndex = 0;

        /// <summary>
        /// Генерирует контур (список Vector2) для скругленного прямоугольника с треугольным вырезом на любой стороне.
        /// </summary>s
        public static List<Vector2> GenerateContour(float w, float h,
            CornerRadii radii,
            Side cutoutSide,
            float baseNorm,
            float offsetNorm,
            float depthNorm, int arcSegments = 4)
        {
            float rTL = Mathf.Clamp(radii.TopLeft, 0, Mathf.Min(w, h) / 2);
            float rTR = Mathf.Clamp(radii.TopRight, 0, Mathf.Min(w, h) / 2);
            float rBR = Mathf.Clamp(radii.BottomRight, 0, Mathf.Min(w, h) / 2);
            float rBL = Mathf.Clamp(radii.BottomLeft, 0, Mathf.Min(w, h) / 2);

            float sideWidth = cutoutSide is Side.Top or Side.Bottom ? w : h;
            float sideHeight = cutoutSide is Side.Top or Side.Bottom ? h : w;

            var basePx = Mathf.Clamp(baseNorm * sideWidth, 0, sideWidth);
            var offsetPx = Mathf.Clamp(offsetNorm * sideWidth - basePx, 0, sideWidth - basePx);
            var depthPx = Mathf.Clamp(depthNorm * sideHeight, 0, sideHeight);
            var baseStart = offsetPx;
            var baseEnd = baseStart + basePx;
            var baseMid = baseStart + basePx * 0.5f;
            var center = cutoutSide switch
            {
                Side.Top => new Vector2(baseMid, depthPx),
                Side.Bottom => new Vector2(baseMid, h - depthPx),
                Side.Left => new Vector2(depthPx, baseMid),
                Side.Right => new Vector2(w - depthPx, baseMid),
                _ => default
            };


            var points = ComputeCorners(w, h, center, rTL, rTR, rBR, rBL, arcSegments);

            bIndex = (ushort)((arcSegments + 1) * (int)cutoutSide + 1);
            var first = cutoutSide is Side.Top or Side.Right ? baseStart : baseEnd;
            var last = cutoutSide is Side.Top or Side.Right ? baseEnd : baseStart;
            var fPoint = cutoutSide switch
            {
                Side.Top => new Vector2(first, 0),
                Side.Bottom => new Vector2(first, h),
                Side.Right => new Vector2(w, first),
                Side.Left => new Vector2(0, first),
                _ => default
            };

            var lPoint = cutoutSide switch
            {
                Side.Top => new Vector2(last, 0),
                Side.Bottom => new Vector2(last, h),
                Side.Right => new Vector2(w, last),
                Side.Left => new Vector2(0, last),
                _ => default
            };
            
            points.Insert(bIndex, fPoint);
            points.Insert(bIndex + 1, lPoint);
            return points;
        }

        private static List<Vector2> ComputeCorners(
            float w, float h, Vector2 center,
            float rTL, float rTR,
            float rBR, float rBL, int arcSegments)
        {
            var points = new List<Vector2>();
            points.Clear();
            points.Add(center);
            if (rTL > 0)
                AddArc(rTL, rTL, Mathf.PI, 1.5f * Mathf.PI, rTL, arcSegments);
            else
                points.Add(new Vector2(0, 0));
            if (rTR > 0)
                AddArc(w - rTR, rTR, 1.5f * Mathf.PI, 2 * Mathf.PI, rTR, arcSegments);
            else
                points.Add(new Vector2(w, 0));
            if (rBR > 0)
                AddArc(w - rBR, h - rBR, 0, 0.5f * Mathf.PI, rBR, arcSegments);
            else
                points.Add(new Vector2(w, h));
            if (rBL > 0)
                AddArc(rBL, h - rBL, 0.5f * Mathf.PI, Mathf.PI, rBL, arcSegments);
            else
                points.Add(new Vector2(0, h));
            points.Add(new Vector2(0, rTL));
            return points;
            
            void AddArc(float cx, float cy, float fromAngle, float toAngle, float radius, int arcSegments)
            {
                if (radius < 1e-3f)
                {
                    return;
                }

                for (int i = 0; i <= arcSegments; i++)
                {
                    float t = i / (float)arcSegments;
                    float angle = Mathf.Lerp(fromAngle, toAngle, t);
                    points.Add(new Vector2(cx + Mathf.Cos(angle) * radius, cy + Mathf.Sin(angle) * radius));
                }
            }
        }

        /// <summary>
        /// Построить меш (вершины и индексы) для UI Toolkit (или Mesh API) на основании точек контура
        /// </summary>
        public static void BuildMesh(List<Vector2> contour, float w, float h, out Vertex[] vertices,
            out ushort[] indices)
        {
            int n = contour.Count;
            vertices = new Vertex[n];
            for (int i = 0; i < n; i++)
            {
                vertices[i] = new Vertex
                {
                    position = new Vector3(contour[i].x, contour[i].y, 0),
                    tint = Color.white,
                    uv = new Vector2(contour[i].x / w, 1f - contour[i].y / h)
                };
            }

            // Триангуляция фаном (работает если контур выпуклый/почти-выпуклый и не пересекается!)
            var inds = new List<ushort>();
            for (ushort i = 1; i < n - 1; i++)
            {
                ushort a = 0;
                ushort b = i;
                if (b == bIndex) continue;
                ushort c = (ushort)(i + 1);
                inds.Add(a);
                inds.Add(b);
                inds.Add(c);
            }

            indices = inds.ToArray();
        }
    }
}