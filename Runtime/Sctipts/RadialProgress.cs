
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
        private const string ussClassName = "radial-progress";
        private const string ussLabelClassName = "radial-progress__label";

        [UxmlAttribute]
        public Color TrackColor
        {
            get => trackColor;
            set
            {
                trackColor = value;
                trackMesh.Color = value;
                MarkDirtyRepaint();
            }
        }

        [UxmlAttribute]
        public Color ProgressColor
        {
            get => progressColor;
            set
            {
                progressColor = value;
                progressMesh.Color = value;
                MarkDirtyRepaint();
            }
        }


        private Color trackColor = Color.gray;
        private Color progressColor = Color.white;
        
        private readonly EllipseMesh trackMesh;
        private readonly EllipseMesh progressMesh;
        private readonly Label label;
        
        private const int numSteps = 200;
        private float progress;

        /// <summary>
        /// A value between 0 and 100
        /// </summary>
        [UxmlAttribute]
        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                label.text = Mathf.Clamp(Mathf.Round(value), 0, 100) + "%";
                MarkDirtyRepaint();
            }
        }

        [UxmlAttribute]
        public bool showProgress
        {
            get => label.style.display == DisplayStyle.Flex;
            set
            {
                label.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                MarkDirtyRepaint();
            }
        }

        public RadialProgress()
        {
            label = new Label();
            label.AddToClassList(ussLabelClassName);
            Add(label);

            progressMesh = new EllipseMesh(numSteps);
            trackMesh = new EllipseMesh(numSteps);

            AddToClassList(ussClassName);
            generateVisualContent += GenerateVisualContent;
            Progress = 0.0f;
        }

        private static void GenerateVisualContent(MeshGenerationContext context)
        {
            RadialProgress element = (RadialProgress)context.visualElement;
            element.DrawMeshes(context);
        }

        private void DrawMeshes(MeshGenerationContext context)
        {
            var halfWidth = contentRect.width * 0.5f;
            var halfHeight = contentRect.height * 0.5f;

            if (halfWidth < 2.0f || halfHeight < 2.0f)
                return;

            progressMesh.Width = halfWidth;
            progressMesh.Height = halfHeight;
            progressMesh.BorderSize = 10;
            progressMesh.UpdateMesh();

            trackMesh.Width = halfWidth;
            trackMesh.Height = halfHeight;
            trackMesh.BorderSize = 10;
            trackMesh.UpdateMesh();

            var trackMeshWriteData = context.Allocate(trackMesh.Vertices.Length, trackMesh.Indices.Length);
            trackMeshWriteData.SetAllVertices(trackMesh.Vertices);
            trackMeshWriteData.SetAllIndices(trackMesh.Indices);

            var clampedProgress = Mathf.Clamp(progress, 0.0f, 100.0f);

            var sliceSize = Mathf.FloorToInt((numSteps * clampedProgress) / 100.0f);

            if (sliceSize == 0)
                return;

            sliceSize *= 6;

            var progressMeshWriteData = context.Allocate(progressMesh.Vertices.Length, sliceSize);
            progressMeshWriteData.SetAllVertices(progressMesh.Vertices);

            var tempIndicesArray = new NativeArray<ushort>(progressMesh.Indices, Allocator.Temp);
            progressMeshWriteData.SetAllIndices(tempIndicesArray.Slice(0, sliceSize));
            tempIndicesArray.Dispose();
        }

    }
}
