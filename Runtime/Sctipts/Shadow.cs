using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    [UxmlElement]
    public partial class Shadow : VisualElement
    {
        [UxmlAttribute] public Side CutoutSide
        {
            get => cutoutSide;
            set { if (cutoutSide != value) { cutoutSide = value; MarkDirtyRepaint(); } }
        }
        [UxmlAttribute, Range(0f, 1f)]
        public float CutoutOffset
        {
            get => cutoutOffset;
            set { if (!Mathf.Approximately(cutoutOffset, value)) { cutoutOffset = value; MarkDirtyRepaint(); } }
        }
        [UxmlAttribute, Range(0f, 1f)]
        public float BaseSize
        {
            get => baseSize;
            set { if (!Mathf.Approximately(baseSize, value)) { baseSize = value; MarkDirtyRepaint(); } }
        }
        [UxmlAttribute, Range(0f, 1f)]
        public float Depth
        {
            get => depth;
            set { if (!Mathf.Approximately(depth, value)) { depth = value; MarkDirtyRepaint(); } }
        }

        [UxmlAttribute] public float ShadowScale
        {
            get => shadowScale;
            set { if (!Mathf.Approximately(shadowScale, value)) { shadowScale = value; MarkDirtyRepaint(); } }
        }
        [UxmlAttribute] public float ShadowOffsetX
        {
            get => shadowOffsetX;
            set { if (!Mathf.Approximately(shadowOffsetX, value)) { shadowOffsetX = value; MarkDirtyRepaint(); } }
        }
        [UxmlAttribute] public float ShadowOffsetY
        {
            get => shadowOffsetY;
            set { if (!Mathf.Approximately(shadowOffsetY, value)) { shadowOffsetY = value; MarkDirtyRepaint(); } }
        }
        [UxmlAttribute] public Color ShadowColor
        {
            get => shadowColor;
            set { if (shadowColor != value) { shadowColor = value; MarkDirtyRepaint(); } }
        }
        
        private Side cutoutSide = Side.Top;
        private float cutoutOffset = 0.5f;
        private float baseSize = 0.3f;
        private float depth = 0.1f;
        private float shadowScale = 1.08f;
        private float shadowOffsetX = 4f;
        private float shadowOffsetY = -4f;
        private Color shadowColor = new Color(0, 0, 0, 0.20f);

        public Shadow()
        {
            generateVisualContent += GenerateShadowMesh;
            pickingMode = PickingMode.Ignore;
            style.backgroundColor = Color.clear;
        }

        private void GenerateShadowMesh(MeshGenerationContext ctx)
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

            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);

            var mesh = ctx.Allocate(7, 15);
            Vector2 ScaleAndOffset(Vector2 p)
            {
                p -= center;
                p *= shadowScale;
                p += center + new Vector2(shadowOffsetX, shadowOffsetY);
                return p;
            }

            if (cutoutSide == Side.Top)
            {
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(0, 0)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(baseStart, 0)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(baseMid, depthPx)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(baseEnd, 0)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w, 0)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w, h)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(0, h)), w, h));

                mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(6);
                mesh.SetNextIndex(1); mesh.SetNextIndex(2); mesh.SetNextIndex(6);
                mesh.SetNextIndex(2); mesh.SetNextIndex(3); mesh.SetNextIndex(5);
                mesh.SetNextIndex(3); mesh.SetNextIndex(4); mesh.SetNextIndex(5);
                mesh.SetNextIndex(2); mesh.SetNextIndex(5); mesh.SetNextIndex(6);
            }
            else if (cutoutSide == Side.Bottom)
            {
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(0, 0)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w, 0)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w, h)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(baseEnd, h)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(baseMid, h - depthPx)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(baseStart, h)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(0, h)), w, h));

                mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(4);
                mesh.SetNextIndex(1); mesh.SetNextIndex(2); mesh.SetNextIndex(4);
                mesh.SetNextIndex(2); mesh.SetNextIndex(3); mesh.SetNextIndex(4);
                mesh.SetNextIndex(4); mesh.SetNextIndex(5); mesh.SetNextIndex(6);
                mesh.SetNextIndex(0); mesh.SetNextIndex(4); mesh.SetNextIndex(6);
            }
            else if (cutoutSide == Side.Left)
            {
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(0, 0)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w, 0)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w, h)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(0, h)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(0, baseEnd)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(depthPx, baseMid)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(0, baseStart)), w, h));

                mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(5);
                mesh.SetNextIndex(1); mesh.SetNextIndex(2); mesh.SetNextIndex(5);
                mesh.SetNextIndex(2); mesh.SetNextIndex(3); mesh.SetNextIndex(5);
                mesh.SetNextIndex(3); mesh.SetNextIndex(4); mesh.SetNextIndex(5);
                mesh.SetNextIndex(0); mesh.SetNextIndex(5); mesh.SetNextIndex(6);
            }
            else // Right
            {
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(0, 0)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w, 0)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w, baseStart)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w - depthPx, baseMid)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w, baseEnd)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(w, h)), w, h));
                mesh.SetNextVertex(VertexShadow(ScaleAndOffset(new Vector2(0, h)), w, h));

                mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(3);
                mesh.SetNextIndex(1); mesh.SetNextIndex(2); mesh.SetNextIndex(3);
                mesh.SetNextIndex(0); mesh.SetNextIndex(3); mesh.SetNextIndex(6);
                mesh.SetNextIndex(3); mesh.SetNextIndex(5); mesh.SetNextIndex(6);
                mesh.SetNextIndex(3); mesh.SetNextIndex(4); mesh.SetNextIndex(5);
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private Vertex VertexShadow(Vector2 p, float w, float h)
        {
            return new Vertex
            {
                position = new Vector3(p.x, p.y, 0),
                tint = shadowColor,
                uv = Vector2.zero
            };
        }
    }
}
