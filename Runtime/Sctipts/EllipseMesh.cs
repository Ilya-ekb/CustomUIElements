using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    public class EllipseMesh
    {
        
        public int NumSteps
        {
            get => numSteps;
            set
            {
                isDirty = value != numSteps;
                numSteps = value;
            }
        }

        public float Width
        {
            get => width;
            set => CompareAndWrite(ref width, value);
        }

        public float Height
        {
            get => height;
            set => CompareAndWrite(ref height, value);
        }
        
        public Vertex[] Vertices { get; private set; }
        public ushort[] Indices { get; private set; }
        
        public Color Color
        {
            get => color;
            set
            {
                isDirty = value != color;
                color = value;
            }
        }

        public float BorderSize
        {
            get => borderSize;
            set => CompareAndWrite(ref borderSize, value);
        }

        private int numSteps;
        private float width;
        private float height;
        private Color color;
        private float borderSize;
        private bool isDirty;
        
        public EllipseMesh(int numSteps)
        {
            this.numSteps = numSteps;
            isDirty = true;
        }

        public void UpdateMesh()
        {
            if (!isDirty)
                return;

            var numVertices = NumSteps * 2;
            var numIndices = numVertices * 6;

            if (Vertices == null || Vertices.Length != numVertices)
                Vertices = new Vertex[numVertices];

            if (Indices == null || Indices.Length != numIndices)
                Indices = new ushort[numIndices];

            var stepSize = 360.0f / NumSteps;
            var angle = -180.0f;

            for (var i = 0; i < NumSteps; ++i)
            {
                angle -= stepSize;
                var radians = Mathf.Deg2Rad * angle;

                var outerX = Mathf.Sin(radians) * Width;
                var outerY = Mathf.Cos(radians) * Height;
                var outerVertex = new Vertex
                {
                    position = new Vector3(Width + outerX, Height + outerY, Vertex.nearZ),
                    tint = Color
                };
                Vertices[i * 2] = outerVertex;

                var innerX = Mathf.Sin(radians) * (Width - BorderSize);
                var innerY = Mathf.Cos(radians) * (Height - BorderSize);
                var innerVertex = new Vertex
                {
                    position = new Vector3(Width + innerX, Height + innerY, Vertex.nearZ),
                    tint = Color
                };
                Vertices[i * 2 + 1] = innerVertex;

                Indices[i * 6] = (ushort)((i == 0) ? Vertices.Length - 2 : (i - 1) * 2); // previous outer vertex
                Indices[i * 6 + 1] = (ushort)(i * 2); // current outer vertex
                Indices[i * 6 + 2] = (ushort)(i * 2 + 1); // current inner vertex

                Indices[i * 6 + 3] = (ushort)((i == 0) ? Vertices.Length - 2 : (i - 1) * 2); // previous outer vertex
                Indices[i * 6 + 4] = (ushort)(i * 2 + 1); // current inner vertex
                Indices[i * 6 + 5] =
                    (ushort)((i == 0) ? Vertices.Length - 1 : (i - 1) * 2 + 1); // previous inner vertex
            }

            isDirty = false;
        }

        private void CompareAndWrite(ref float field, float newValue)
        {
            if (Mathf.Abs(field - newValue) <= float.Epsilon) return;
            isDirty = true;
            field = newValue;
        }
    }
}