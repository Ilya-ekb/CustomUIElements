using UnityEngine;

namespace CustomUIElements
{
    public class CutoutMesh : CustomMesh
    {
        public Side СutoutSide
        {
            get => cutoutSide;
            set => CompareAndWrite(ref cutoutSide, value);
        }

        public float BaseSize
        {
            get => baseSize;
            set => CompareAndWrite(ref baseSize, value);
        }

        public float CutoutOffset
        {
            get => cutoutOffset;
            set => CompareAndWrite(ref cutoutOffset, value);
        }

        public float Depth
        {
            get => depth;
            set => CompareAndWrite(ref depth, value);
        }

        private Side cutoutSide;
        private float baseSize;
        private float cutoutOffset;
        private float depth;
        private Texture2D texture;
        private Color tintColor;


        public override void UpdateMesh()
        {
            if (!isDirty)
                return;

            var w = Width;
            var h = Height;
            var radii = CornerRadii;
            
            var contour = CutoutMeshBuilder.GenerateCutoutContour(w, h, radii, cutoutSide, baseSize, cutoutOffset, depth);
            CutoutMeshBuilder.BuildMesh(contour, w, h, out var vertices, out var indices);
            Vertices = vertices.ToArray();
            Indices = indices.ToArray();
            
            isDirty = false;
        }
    }
}