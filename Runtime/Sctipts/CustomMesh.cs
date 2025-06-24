using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    public abstract class CustomMesh
    {
        public float Height;
        public Vertex[] Vertices { get; protected set; } = Array.Empty<Vertex>();
        public ushort[] Indices { get; protected set; } = Array.Empty<ushort>();

        public float Width
        {
            get => width;
            set => CompareAndWrite(ref width, value);
        }

        public CornerRadii CornerRadii
        {
            get => cornerRadii;
            set => CompareAndWrite(ref cornerRadii, value);
        }

        public Texture2D Texture
        {
            get => texture;
            set => CompareAndWrite(ref texture, value);
        }

        public Color TintColor
        {
            get => tintColor;
            set => CompareAndWrite(ref tintColor, value);
        }

        protected bool isDirty = true;
        private float width;
        private CornerRadii cornerRadii;
        private Texture2D texture;
        private Color tintColor;

        public abstract void UpdateMesh();

        protected void CompareAndWrite<T>(ref T field, T newValue)
        {
            if (field is not null && field.Equals(newValue)) return;
            isDirty = true;
            field = newValue;
        }
    }
}