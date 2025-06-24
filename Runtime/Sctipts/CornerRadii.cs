using System;

namespace CustomUIElements
{
    public readonly struct CornerRadii : IEquatable<CornerRadii>
    {
        public readonly float TopLeft;
        public readonly float TopRight;
        public readonly float BottomRight;
        public readonly float BottomLeft;
        public readonly int Smooth;

        public CornerRadii(float tl, float tr, float br, float bl,int smooth)
        {
            TopLeft = tl;
            TopRight = tr;
            BottomRight = br;
            BottomLeft = bl;
            Smooth = smooth;
        }

        public bool Equals(CornerRadii other)
        {
            return TopLeft.Equals(other.TopLeft) && TopRight.Equals(other.TopRight) && BottomRight.Equals(other.BottomRight) && BottomLeft.Equals(other.BottomLeft) && Smooth == other.Smooth;
        }

        public override bool Equals(object obj)
        {
            return obj is CornerRadii other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TopLeft, TopRight, BottomRight, BottomLeft, Smooth);
        }
    }
}