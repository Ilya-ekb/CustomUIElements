using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    [UxmlElement]
    public partial class TriangleCutoutElement : VisualElement
    {
        public enum Side
        {
            Top,
            Bottom,
            Left,
            Right
        }

        [UxmlAttribute]
        public Side CutoutSide
        {
            get => cutoutSide;
            set
            {
                if (cutoutSide != value)
                {
                    cutoutSide = value;
                    MarkDirtyRepaint();
                }
            }
        }

        [UxmlAttribute, Range(0f, 1f)]
        public float CutoutOffset
        {
            get => cutoutOffset;
            set
            {
                if (!Mathf.Approximately(cutoutOffset, value))
                {
                    cutoutOffset = value;
                    MarkDirtyRepaint();
                }
            }
        }

        [UxmlAttribute, Range(0f, 1f)]
        public float BaseSize
        {
            get => baseSize;
            set
            {
                if (!Mathf.Approximately(baseSize, value))
                {
                    baseSize = value;
                    MarkDirtyRepaint();
                }
            }
        }


        [UxmlAttribute, Range(0f, 1f)]
        public float Depth
        {
            get => depth;
            set
            {
                if (!Mathf.Approximately(depth, value))
                {
                    depth = value;
                    MarkDirtyRepaint();
                }
            }
        }
        
        [UxmlAttribute] public Texture2D Texture
        {
            get => backgroundTexture;
            set
            {
                backgroundTexture = value;
                MarkDirtyRepaint();
            }
        }

        private Side cutoutSide = Side.Top;
        private float cutoutOffset = 0.5f;
        private float baseSize = 0.3f;
        private float depth = 0.1f;
        private Texture2D backgroundTexture;
        
        public TriangleCutoutElement()
        {
            generateVisualContent += GenerateMesh;
            pickingMode = PickingMode.Position;
            style.backgroundColor = Color.clear;
        }
        
        private void GenerateMesh(MeshGenerationContext ctx)
    {
        var rect = contentRect;
        if (rect.width <= 1 || rect.height <= 1) return;

        float w = rect.width, h = rect.height;
        bool topOrBottom = cutoutSide is Side.Top or Side.Bottom;

        float sideLength = topOrBottom ? w : h;
        float basePx = Mathf.Clamp(baseSize * sideLength, 0, sideLength);
        float offsetPx = Mathf.Clamp(cutoutOffset * sideLength - basePx, 0, sideLength - basePx);
        float depthPx = Mathf.Clamp(depth * (topOrBottom ? h : w), 0, topOrBottom ? h : w);

        float baseStart = offsetPx;
        float baseEnd = baseStart + basePx;
        float baseMid = baseStart + basePx * 0.5f;

        var mesh = ctx.Allocate(7, 15, Texture);

        if (cutoutSide == Side.Top)
        {
            // 0: левый низ; 1: baseStart низ; 2: Tip; 3: baseEnd низ; 4: правый низ; 5: правый верх; 6: левый верх
            mesh.SetNextVertex(VertexAt(0, 0, w, h));                // 0
            mesh.SetNextVertex(VertexAt(baseStart, 0, w, h));        // 1
            mesh.SetNextVertex(VertexAt(baseMid, depthPx, w, h));    // 2
            mesh.SetNextVertex(VertexAt(baseEnd, 0, w, h));          // 3
            mesh.SetNextVertex(VertexAt(w, 0, w, h));                // 4
            mesh.SetNextVertex(VertexAt(w, h, w, h));                // 5
            mesh.SetNextVertex(VertexAt(0, h, w, h));                // 6

            // ---- Индексы ----
            mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(6);
            mesh.SetNextIndex(1); mesh.SetNextIndex(2); mesh.SetNextIndex(6);
            mesh.SetNextIndex(2); mesh.SetNextIndex(3); mesh.SetNextIndex(5);
            mesh.SetNextIndex(3); mesh.SetNextIndex(4); mesh.SetNextIndex(5);
            mesh.SetNextIndex(2); mesh.SetNextIndex(5); mesh.SetNextIndex(6);
        }
        else if (cutoutSide == Side.Bottom)
        {
            mesh.SetNextVertex(VertexAt(0, 0, w, h));                    // 0
            mesh.SetNextVertex(VertexAt(w, 0, w, h));                    // 1
            mesh.SetNextVertex(VertexAt(w, h, w, h));                    // 2
            mesh.SetNextVertex(VertexAt(baseEnd, h, w, h));              // 3
            mesh.SetNextVertex(VertexAt(baseMid, h - depthPx, w, h));    // 4
            mesh.SetNextVertex(VertexAt(baseStart, h, w, h));            // 5
            mesh.SetNextVertex(VertexAt(0, h, w, h));                    // 6

            mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(4);
            mesh.SetNextIndex(1); mesh.SetNextIndex(2); mesh.SetNextIndex(4);
            mesh.SetNextIndex(2); mesh.SetNextIndex(3); mesh.SetNextIndex(4);
            mesh.SetNextIndex(4); mesh.SetNextIndex(5); mesh.SetNextIndex(6);
            mesh.SetNextIndex(0); mesh.SetNextIndex(4); mesh.SetNextIndex(6);
        }
        else if (cutoutSide == Side.Left)
        {
            mesh.SetNextVertex(VertexAt(0, 0, w, h));                // 0
            mesh.SetNextVertex(VertexAt(w, 0, w, h));                // 1
            mesh.SetNextVertex(VertexAt(w, h, w, h));                // 2
            mesh.SetNextVertex(VertexAt(0, h, w, h));                // 3
            mesh.SetNextVertex(VertexAt(0, baseEnd, w, h));          // 4
            mesh.SetNextVertex(VertexAt(depthPx, baseMid, w, h));    // 5
            mesh.SetNextVertex(VertexAt(0, baseStart, w, h));        // 6

            mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(5);
            mesh.SetNextIndex(1); mesh.SetNextIndex(2); mesh.SetNextIndex(5);
            mesh.SetNextIndex(2); mesh.SetNextIndex(3); mesh.SetNextIndex(5);
            mesh.SetNextIndex(3); mesh.SetNextIndex(4); mesh.SetNextIndex(5);
            mesh.SetNextIndex(0); mesh.SetNextIndex(5); mesh.SetNextIndex(6);
        }
        else // Right
        {
            mesh.SetNextVertex(VertexAt(0, 0, w, h));                    // 0
            mesh.SetNextVertex(VertexAt(w, 0, w, h));                    // 1
            mesh.SetNextVertex(VertexAt(w, baseStart, w, h));            // 2
            mesh.SetNextVertex(VertexAt(w - depthPx, baseMid, w, h));    // 3
            mesh.SetNextVertex(VertexAt(w, baseEnd, w, h));              // 4
            mesh.SetNextVertex(VertexAt(w, h, w, h));                    // 5
            mesh.SetNextVertex(VertexAt(0, h, w, h));                    // 6

            mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(3);
            mesh.SetNextIndex(1); mesh.SetNextIndex(2); mesh.SetNextIndex(3);
            mesh.SetNextIndex(0); mesh.SetNextIndex(3); mesh.SetNextIndex(6);
            mesh.SetNextIndex(3); mesh.SetNextIndex(5); mesh.SetNextIndex(6);
            mesh.SetNextIndex(3); mesh.SetNextIndex(4); mesh.SetNextIndex(5);
        }
    }
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static Vertex VertexAt(float x, float y, float w, float h)
        {
            return new Vertex
            {
                position = new Vector3(x, y, 0),
                tint = Color.white,
                uv = new Vector2(x / w, 1 - y / h)
            };
        }
    }
}