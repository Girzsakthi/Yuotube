using UnityEngine;

namespace SkyPlaneAR.Tracking
{
    public class AnchorData
    {
        public string Id { get; }
        public Pose WorldPose { get; internal set; }
        public string PlaneId { get; }
        public bool IsTracking { get; internal set; }
        public GameObject AttachedGameObject { get; internal set; }

        public AnchorData(string id, Pose pose, string planeId)
        {
            Id = id;
            WorldPose = pose;
            PlaneId = planeId;
            IsTracking = true;
        }

        public override string ToString() =>
            $"Anchor[{Id}] plane={PlaneId} pos={WorldPose.position} tracking={IsTracking}";
    }
}
