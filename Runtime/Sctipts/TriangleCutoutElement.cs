using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    [UxmlElement]
    public partial class TriangleCutoutElement : VisualElement
    {
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

        [UxmlAttribute]
        public Color BackgrounColor
        {
            get => backgrounColor;
            set
            {
                if (value.Equals(backgrounColor)) return;
                backgrounColor = value;
                MarkDirtyRepaint();
            }
        }

        [UxmlAttribute]
        public Texture2D Texture
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
        private readonly CutoutMesh cutoutMesh;
        private Color backgrounColor;
        private bool debugState;

        public TriangleCutoutElement()
        {
            cutoutMesh = new CutoutMesh(7, 15);
            generateVisualContent += GenerateMesh;
            pickingMode = PickingMode.Position;
            style.backgroundColor = Color.clear;
        }

        private void GenerateMesh(MeshGenerationContext ctx)
        {
            var element = (TriangleCutoutElement)ctx.visualElement;
            element.DrawMeshes(ctx);
        }

        private void DrawMeshes(MeshGenerationContext ctx)
        {
            var rect = contentRect;
            if (rect.width <= 1 || rect.height <= 1) return;
            cutoutMesh.Width = rect.width;
            cutoutMesh.Height = rect.height;
            cutoutMesh.СutoutSide = cutoutSide;
            cutoutMesh.BaseSize = baseSize;
            cutoutMesh.CutoutOffset = cutoutOffset;
            cutoutMesh.Depth = depth;
            cutoutMesh.CornerRadii = new CornerRadii(
                resolvedStyle.borderTopLeftRadius, resolvedStyle.borderTopRightRadius,
                resolvedStyle.borderBottomRightRadius, resolvedStyle.borderBottomLeftRadius);
            cutoutMesh.Texture = backgroundTexture;
            cutoutMesh.UpdateMesh();

            var mesh = ctx.Allocate(cutoutMesh.Vertices.Length, cutoutMesh.Indices.Length, cutoutMesh.Texture);
            mesh.SetAllVertices(cutoutMesh.Vertices);
            mesh.SetAllIndices(cutoutMesh.Indices);

            if (!DebugState) return;
            var p2d = ctx.painter2D;

            var verts = cutoutMesh.Vertices;
            var inds = cutoutMesh.Indices;
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
                p2d.fillColor = i == 1 ? Color.green : i == verts.Length - 1 ? Color.black : Color.blue;
                p2d.BeginPath();
                p2d.MoveTo(pos + new Vector3(3, 0));
                p2d.Arc(pos, 4, 0, 360);
                p2d.ClosePath();
                p2d.Fill();
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
    }
}