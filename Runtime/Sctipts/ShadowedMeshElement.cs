using UnityEngine;
using UnityEngine.UIElements;

public class MeshData2D
{
    public readonly Vector3[] positions;
    public readonly Vector2[] uvs;
    public readonly Color[] colors;
    public readonly ushort[] indices;

    public MeshData2D(int vertexCount, int indexCount)
    {
        positions = new Vector3[vertexCount];
        uvs = new Vector2[vertexCount];
        colors = new Color[vertexCount];
        indices = new ushort[indexCount];
    }
}

/// <summary>
/// Абстрактный элемент с опциональной тенью, универсальный для любого 2D меша
/// </summary>
[UxmlElement]
public abstract partial class ShadowedMeshElement : VisualElement
{
    // --- Shadow parameters (UXML-compatible) ---
    [UxmlAttribute] public bool ShadowEnabled
    {
        get => shadowEnabled;
        set { if (shadowEnabled != value) { shadowEnabled = value; MarkDirtyRepaint(); } }
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

    // Поля
    private bool shadowEnabled = true;
    private float shadowScale = 1.08f;
    private float shadowOffsetX = 6f;
    private float shadowOffsetY = -6f;
    private Color shadowColor = new Color(0, 0, 0, 0.22f);

    /// <summary>
    /// Виртуальные данные меша (заполняются потомком)
    /// </summary>
    protected MeshData2D MeshData { get; private set; }

    public ShadowedMeshElement()
    {
        generateVisualContent += OnGenerateVisualContent;
        RegisterCallback<GeometryChangedEvent>(_ => { UpdateMeshData(); MarkDirtyRepaint(); });
    }

    /// <summary>
    /// Вызывается при изменении геометрии — потомок обязан заполнить MeshData!
    /// </summary>
    protected abstract void UpdateMeshData();

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        if (MeshData == null || MeshData.positions == null || MeshData.indices == null)
            return;

        // 1. Shadow pass
        if (ShadowEnabled)
        {
            var mwd = ctx.Allocate(MeshData.positions.Length, MeshData.indices.Length);
            Vector2 center = Vector2.zero;
            for (int i = 0; i < MeshData.positions.Length; i++)
                center += (Vector2)MeshData.positions[i];
            center /= MeshData.positions.Length;

            for (int i = 0; i < MeshData.positions.Length; i++)
            {
                Vector2 p = MeshData.positions[i];
                p -= center;
                p *= ShadowScale;
                p += center + new Vector2(ShadowOffsetX, ShadowOffsetY);

                mwd.SetNextVertex(new Vertex
                {
                    position = new Vector3(p.x, p.y, 0),
                    tint = ShadowColor,
                    uv = MeshData.uvs != null && MeshData.uvs.Length == MeshData.positions.Length
                        ? MeshData.uvs[i]
                        : Vector2.zero
                });
            }
            for (int i = 0; i < MeshData.indices.Length; i++)
                mwd.SetNextIndex(MeshData.indices[i]);
        }

        // 2. Main mesh pass (цвет — любой, хоть по массиву, хоть по стилю)
        {
            var mwd = ctx.Allocate(MeshData.positions.Length, MeshData.indices.Length);
            for (int i = 0; i < MeshData.positions.Length; i++)
            {
                mwd.SetNextVertex(new Vertex
                {
                    position = MeshData.positions[i],
                    tint = MeshData.colors != null && MeshData.colors.Length == MeshData.positions.Length
                        ? MeshData.colors[i]
                        : resolvedStyle.backgroundColor,
                    uv = MeshData.uvs != null && MeshData.uvs.Length == MeshData.positions.Length
                        ? MeshData.uvs[i]
                        : Vector2.zero
                });
            }
            for (int i = 0; i < MeshData.indices.Length; i++)
                mwd.SetNextIndex(MeshData.indices[i]);
        }
    }

    /// <summary>
    /// Вызвать потомку для обновления формы (и триггера перерисовки)
    /// </summary>
    protected void SetMeshData(MeshData2D meshData)
    {
        this.MeshData = meshData;
        MarkDirtyRepaint();
    }
}
