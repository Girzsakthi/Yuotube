using UnityEngine;

namespace SkyPlaneAR.PlaneDetection
{
    public enum PlaneAlignment
    {
        HorizontalUp,
        HorizontalDown,
        Vertical,
        NotAxisAligned
    }

    public readonly struct PlaneData
    {
        public readonly string Id;
        public readonly Vector3 Center;
        public readonly Vector3 Normal;

        /// <summary>Width and height of the plane in meters.</summary>
        public readonly Vector2 Size;

        public readonly PlaneAlignment Alignment;
        public readonly Pose Pose;

        /// <summary>Boundary polygon in local plane space (XZ plane).</summary>
        public readonly Vector2[] BoundaryPoints;

        public PlaneData(string id, Vector3 center, Vector3 normal,
                         Vector2 size, PlaneAlignment alignment,
                         Pose pose, Vector2[] boundaryPoints)
        {
            Id = id;
            Center = center;
            Normal = normal;
            Size = size;
            Alignment = alignment;
            Pose = pose;
            BoundaryPoints = boundaryPoints;
        }

        public float Area => Size.x * Size.y;

        public override string ToString() =>
            $"Plane[{Id}] {Alignment} center={Center} size={Size}";
    }

    public readonly struct PlaneDetectionConfig
    {
        public readonly bool DetectHorizontal;
        public readonly bool DetectVertical;
        public readonly float MinimumArea;

        public PlaneDetectionConfig(bool detectHorizontal, bool detectVertical, float minimumArea)
        {
            DetectHorizontal = detectHorizontal;
            DetectVertical = detectVertical;
            MinimumArea = minimumArea;
        }
    }
}
