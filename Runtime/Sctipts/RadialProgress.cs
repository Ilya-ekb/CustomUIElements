
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUIElements
{
    /// <summary>
    /// An element that displays progress inside a partially filled circle
    /// </summary>
    [UxmlElement]
    public partial class RadialProgress : VisualElement
    {
        // These are USS class names for the control overall and the label.
        public static readonly string ussClassName = "radial-progress";
        public static readonly string ussLabelClassName = "radial-progress__label";

        // These objects allow C# code to access custom USS properties.
        static CustomStyleProperty<Color> s_TrackColor = new CustomStyleProperty<Color>("--track-color");
        static CustomStyleProperty<Color> s_ProgressColor = new CustomStyleProperty<Color>("--progress-color");

        // These are the meshes this control uses.
        EllipseMesh m_TrackMesh;
        EllipseMesh m_ProgressMesh;

        // This is the label that displays the percentage.
        Label m_Label;

        // This is the number of outer vertices to generate the circle.
        const int k_NumSteps = 200;

        // This is the number that the Label displays as a percentage.
        float m_Progress;

        /// <summary>
        /// A value between 0 and 100
        /// </summary>
        [UxmlAttribute]
        public float progress
        {
            // The progress property is exposed in C#.
            get => m_Progress;
            set
            {
                // Whenever the progress property changes, MarkDirtyRepaint() is named. This causes a call to the
                // generateVisualContents callback.
                m_Progress = value;
                m_Label.text = Mathf.Clamp(Mathf.Round(value), 0, 100) + "%";
                MarkDirtyRepaint();
            }
        }

        [UxmlAttribute]
        public bool showProgress
        {
            set
            {
                m_Label.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                MarkDirtyRepaint();
            }
        }

        // This default constructor is RadialProgress's only constructor.
        public RadialProgress()
        {
            // Create a Label, add a USS class name, and add it to this visual tree.
            m_Label = new Label();
            m_Label.AddToClassList(ussLabelClassName);
            Add(m_Label);

            // Create meshes for the track and the progress.
            m_ProgressMesh = new EllipseMesh(k_NumSteps);
            m_TrackMesh = new EllipseMesh(k_NumSteps);

            // Add the USS class name for the overall control.
            AddToClassList(ussClassName);

            // Register a callback after custom style resolution.
            RegisterCallback<CustomStyleResolvedEvent>(CustomStylesResolved);

            // Register a callback to generate the visual content of the control.
            generateVisualContent += GenerateVisualContent;

            progress = 0.0f;
        }

        static void CustomStylesResolved(CustomStyleResolvedEvent evt)
        {
            RadialProgress element = (RadialProgress)evt.currentTarget;
            element.UpdateCustomStyles();
        }

        // After the custom colors are resolved, this method uses them to color the meshes and (if necessary) repaint
        // the control.
        void UpdateCustomStyles()
        {
            if (customStyle.TryGetValue(s_ProgressColor, out var progressColor))
            {
                m_ProgressMesh.color = progressColor;
            }

            if (customStyle.TryGetValue(s_TrackColor, out var trackColor))
            {
                m_TrackMesh.color = trackColor;
            }

            if (m_ProgressMesh.isDirty || m_TrackMesh.isDirty)
                MarkDirtyRepaint();
        }

        // The GenerateVisualContent() callback method calls DrawMeshes().
        static void GenerateVisualContent(MeshGenerationContext context)
        {
            RadialProgress element = (RadialProgress)context.visualElement;
            element.DrawMeshes(context);
        }

        void DrawMeshes(MeshGenerationContext context)
        {
            float halfWidth = contentRect.width * 0.5f;
            float halfHeight = contentRect.height * 0.5f;

            if (halfWidth < 2.0f || halfHeight < 2.0f)
                return;

            m_ProgressMesh.width = halfWidth;
            m_ProgressMesh.height = halfHeight;
            m_ProgressMesh.borderSize = 10;
            m_ProgressMesh.UpdateMesh();

            m_TrackMesh.width = halfWidth;
            m_TrackMesh.height = halfHeight;
            m_TrackMesh.borderSize = 10;
            m_TrackMesh.UpdateMesh();

            // Draw track mesh first
            var trackMeshWriteData = context.Allocate(m_TrackMesh.vertices.Length, m_TrackMesh.indices.Length);
            trackMeshWriteData.SetAllVertices(m_TrackMesh.vertices);
            trackMeshWriteData.SetAllIndices(m_TrackMesh.indices);

            // Keep progress between 0 and 100
            float clampedProgress = Mathf.Clamp(m_Progress, 0.0f, 100.0f);

            // Determine how many triangle are used to depending on progress, to achieve a partially filled circle
            int sliceSize = Mathf.FloorToInt((k_NumSteps * clampedProgress) / 100.0f);

            if (sliceSize == 0)
                return;

            // Every step is 6 indices in the corresponding array
            sliceSize *= 6;

            var progressMeshWriteData = context.Allocate(m_ProgressMesh.vertices.Length, sliceSize);
            progressMeshWriteData.SetAllVertices(m_ProgressMesh.vertices);

            var tempIndicesArray = new NativeArray<ushort>(m_ProgressMesh.indices, Allocator.Temp);
            progressMeshWriteData.SetAllIndices(tempIndicesArray.Slice(0, sliceSize));
            tempIndicesArray.Dispose();
        }

    }
    
    public class EllipseMesh
    {
        int m_NumSteps;
        float m_Width;
        float m_Height;
        Color m_Color;
        float m_BorderSize;
        bool m_IsDirty;
        public Vertex[] vertices { get; private set; }
        public ushort[] indices { get; private set; }

        public EllipseMesh(int numSteps)
        {
            m_NumSteps = numSteps;
            m_IsDirty = true;
        }

        public void UpdateMesh()
        {
            if (!m_IsDirty)
                return;

            int numVertices = numSteps * 2;
            int numIndices = numVertices * 6;

            if (vertices == null || vertices.Length != numVertices)
                vertices = new Vertex[numVertices];

            if (indices == null || indices.Length != numIndices)
                indices = new ushort[numIndices];

            float stepSize = 360.0f / (float)numSteps;
            float angle = -180.0f;

            for (int i = 0; i < numSteps; ++i)
            {
                angle -= stepSize;
                float radians = Mathf.Deg2Rad * angle;

                float outerX = Mathf.Sin(radians) * width;
                float outerY = Mathf.Cos(radians) * height;
                Vertex outerVertex = new Vertex();
                outerVertex.position = new Vector3(width + outerX, height + outerY, Vertex.nearZ);
                outerVertex.tint = color;
                vertices[i * 2] = outerVertex;

                float innerX = Mathf.Sin(radians) * (width - borderSize);
                float innerY = Mathf.Cos(radians) * (height - borderSize);
                Vertex innerVertex = new Vertex();
                innerVertex.position = new Vector3(width + innerX, height + innerY, Vertex.nearZ);
                innerVertex.tint = color;
                vertices[i * 2 + 1] = innerVertex;

                indices[i * 6] = (ushort)((i == 0) ? vertices.Length - 2 : (i - 1) * 2); // previous outer vertex
                indices[i * 6 + 1] = (ushort)(i * 2); // current outer vertex
                indices[i * 6 + 2] = (ushort)(i * 2 + 1); // current inner vertex

                indices[i * 6 + 3] = (ushort)((i == 0) ? vertices.Length - 2 : (i - 1) * 2); // previous outer vertex
                indices[i * 6 + 4] = (ushort)(i * 2 + 1); // current inner vertex
                indices[i * 6 + 5] = (ushort)((i == 0) ? vertices.Length - 1 : (i - 1) * 2 + 1); // previous inner vertex
            }

            m_IsDirty = false;
        }

        public bool isDirty => m_IsDirty;

        void CompareAndWrite(ref float field, float newValue)
        {
            if (Mathf.Abs(field - newValue) > float.Epsilon)
            {
                m_IsDirty = true;
                field = newValue;
            }
        }

        public int numSteps
        {
            get => m_NumSteps;
            set
            {
                m_IsDirty = value != m_NumSteps;
                m_NumSteps = value;
            }
        }

        public float width
        {
            get => m_Width;
            set => CompareAndWrite(ref m_Width, value);
        }

        public float height
        {
            get => m_Height;
            set => CompareAndWrite(ref m_Height, value);
        }
        
        

        public Color color
        {
            get => m_Color;
            set
            {
                m_IsDirty = value != m_Color;
                m_Color = value;
            }
        }

        public float borderSize
        {
            get => m_BorderSize;
            set => CompareAndWrite(ref m_BorderSize, value);
        }
        //
        //         private void GenerateVisualContent(MeshGenerationContext ctx)
        // {
        //     var rect = contentRect;
        //     var p2d = ctx.painter2D;
        //     
        //     // 1. Фон
        //     var bgColor = resolvedStyle.backgroundColor;
        //     if (bgColor.a > 0f)
        //     {
        //         p2d.fillColor = bgColor;
        //         p2d.BeginPath();
        //         p2d.MoveTo(new Vector2(rect.xMin, rect.yMin));
        //         p2d.LineTo(new Vector2(rect.xMax, rect.yMin));
        //         p2d.LineTo(new Vector2(rect.xMax, rect.yMax));
        //         p2d.LineTo(new Vector2(rect.xMin, rect.yMax));
        //         p2d.ClosePath();
        //         p2d.Fill();
        //     }
        //
        //     // 2. Вырез (треугольник нужного цвета, как "дыра" или просто прозрачный)
        //     var cutoutPoly = GetCutoutPolygon(rect);
        //      // Задай нужный альфа
        //     p2d.BeginPath();
        //     p2d.MoveTo(cutoutPoly[0]);
        //     for (int i = 1; i < cutoutPoly.Length; i++)
        //     {
        //         p2d.fillColor = cutoutColor;
        //         p2d.LineTo(cutoutPoly[i]);
        //     }
        //     p2d.ClosePath();
        //     p2d.Fill();
        //
        //     // 3. Бордер (по всему элементу)
        //     var borderColor = resolvedStyle.borderTopColor;
        //     float borderWidth = resolvedStyle.borderTopWidth;
        //     if (borderWidth > 0 && borderColor.a > 0)
        //     {
        //         p2d.strokeColor = borderColor;
        //         p2d.lineWidth = borderWidth;
        //         p2d.BeginPath();
        //         p2d.MoveTo(new Vector2(rect.xMin, rect.yMin));
        //         p2d.LineTo(new Vector2(rect.xMax, rect.yMin));
        //         p2d.LineTo(new Vector2(rect.xMax, rect.yMax));
        //         p2d.LineTo(new Vector2(rect.xMin, rect.yMax));
        //         p2d.ClosePath();
        //         p2d.Stroke();
        //     }        
        // }
        //
        // private Vector2[] GetCutoutPolygon(Rect rect)
        // {
        //     float w = rect.width, h = rect.height;
        //     float sideLength = (CutoutSide == Side.Top || CutoutSide == Side.Bottom) ? w : h;
        //     float basePx = Mathf.Clamp(BaseSize * sideLength, 0, sideLength);
        //     float offsetPx = Mathf.Clamp(CutoutOffset * sideLength - basePx, 0, sideLength - basePx);
        //     float depthPx = Mathf.Clamp(Depth * (CutoutSide is Side.Top or Side.Bottom ? h : w), 0,
        //         (CutoutSide is Side.Top or Side.Bottom) ? h : w);
        //
        //     float baseStart = offsetPx;
        //     float baseEnd = baseStart + basePx;
        //     float baseMid = baseStart + basePx * 0.5f;
        //
        //     if (CutoutSide == Side.Top)
        //     {
        //         return new[]
        //         {
        //             new Vector2(rect.xMin + baseStart, rect.yMin),
        //             new Vector2(rect.xMin + baseMid, rect.yMin + depthPx),
        //             new Vector2(rect.xMin + baseEnd, rect.yMin)
        //         };
        //     }
        //     else if (CutoutSide == Side.Bottom)
        //     {
        //         return new[]
        //         {
        //             new Vector2(rect.xMin + baseStart, rect.yMax),
        //             new Vector2(rect.xMin + baseMid, rect.yMax - depthPx),
        //             new Vector2(rect.xMin + baseEnd, rect.yMax)
        //         };
        //     }
        //     else if (CutoutSide == Side.Left)
        //     {
        //         return new[]
        //         {
        //             new Vector2(rect.xMin, rect.yMin + baseStart),
        //             new Vector2(rect.xMin + depthPx, rect.yMin + baseMid),
        //             new Vector2(rect.xMin, rect.yMin + baseEnd)
        //         };
        //     }
        //     else // Right
        //     {
        //         return new[]
        //         {
        //             new Vector2(rect.xMax, rect.yMin + baseStart),
        //             new Vector2(rect.xMax - depthPx, rect.yMin + baseMid),
        //             new Vector2(rect.xMax, rect.yMin + baseEnd)
        //         };
        //     }
        // }

    }
}
