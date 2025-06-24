using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    [UxmlElement]
    public partial class TriangleCutoutElement : MeshShadowElement
    {
        [UxmlAttribute]
        public Side CutoutSide
        {
            get => cutoutSide;
            set => CompareAndWrite(ref cutoutSide, value);
        }

        [UxmlAttribute, Range(0f, 1f)]
        public float CutoutOffset
        {
            get => cutoutOffset;
            set => CompareAndWrite(ref cutoutOffset, value);
        }

        [UxmlAttribute, Range(0f, 1f)]
        public float BaseSize
        {
            get => baseSize;
            set => CompareAndWrite(ref baseSize, value);
        }


        [UxmlAttribute, Range(0f, 1f)]
        public float Depth
        {
            get => depth;
            set => CompareAndWrite(ref depth, value);
        }

        private Side cutoutSide = Side.Top;
        private float cutoutOffset = 0.5f;
        private float baseSize = 0.3f;
        private float depth = 0.1f;
        private bool debugState;

        public TriangleCutoutElement()
        {
            customMesh = new CutoutMesh();
        }
        
        protected override MeshShadowElement GetMeshElement(MeshGenerationContext ctx)
        {
            return (TriangleCutoutElement)ctx.visualElement;
        }

        protected override CustomMesh AssignMesh()
        {
            return new CutoutMesh();
        }

        protected override void DrawMeshes(MeshGenerationContext ctx)
        {
            var rect = contentRect;
            if (rect.width <= 1 || rect.height <= 1) return;
            customMesh ??= AssignMesh();
            var cutoutMesh = (CutoutMesh)customMesh;
            cutoutMesh.СutoutSide = cutoutSide;
            cutoutMesh.BaseSize = baseSize;
            cutoutMesh.CutoutOffset = cutoutOffset;
            cutoutMesh.Depth = depth;
            base.DrawMeshes(ctx);
        }
    }
}