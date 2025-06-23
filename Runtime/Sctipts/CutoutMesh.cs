using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    public struct CornerRadii
    {
        public float TopLeft, TopRight, BottomRight, BottomLeft;
        public CornerRadii(float tl, float tr, float br, float bl)
        {
            TopLeft = tl;
            TopRight = tr;
            BottomRight = br;
            BottomLeft = bl;
        }
    }
    public class CutoutMesh
    {
        public Vertex[] Vertices { get; private set; }
        public ushort[] Indices { get; private set; }
        public bool IsDirty => isDirty;

        public CornerRadii CornerRadii
        {
            get => cornerRadii;
            set
            {
                if (!cornerRadii.Equals(value))
                {
                    isDirty = true;
                    cornerRadii = value;
                    
                }
            }
        }

        public CutoutMesh(int vertexCount, int indicesCount)
        {
            Vertices = new Vertex[vertexCount];
            Indices = new ushort[indicesCount];
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


        public Color Color
        {
            get => color;
            set
            {
                isDirty = value != color;
                color = value;
            }
        }
        

        public Side СutoutSide
        {
            get => cutoutSide;
            set
            {
                if (cutoutSide != value)
                {
                    isDirty = true;
                    cutoutSide = value;
                }
            }
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

        public Texture2D Texture
        {
            get => texture;
            set
            {
                if (texture != value)
                {
                    isDirty = true;
                    texture = value;
                }
            }
        }


        private float width;
        private float height;
        private Color color;
        private float borderSize;
        private bool isDirty = true;
        private Side cutoutSide;
        private float baseSize;
        private float cutoutOffset;
        private float depth;
        private Texture2D texture;
        private CornerRadii cornerRadii;


        public void UpdateMesh()
        {
            if (!isDirty)
                return;

            float w = width;
            float h = height;
            var radii = cornerRadii;
            
            var contour = CutoutMeshBuilder.GenerateContour(w, h, radii, cutoutSide, baseSize, cutoutOffset, depth);
            CutoutMeshBuilder.BuildMesh(contour, width, height, out var vertices, out var indices);
            Vertices = vertices;
            Indices = indices;
            
            isDirty = false;
        }

        private void CompareAndWrite(ref float field, float newValue)
        {
            if (Mathf.Abs(field - newValue) > float.Epsilon)
            {
                isDirty = true;
                field = newValue;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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