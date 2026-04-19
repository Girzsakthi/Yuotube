using System;

namespace ARMilitary.Data
{
    [Serializable]
    public class SpawnPayload
    {
        public string commandId;
        public string objectType;
        public float spawnDistance = 5f;
        public float heightOffset = 0f;

        public static SpawnPayload Create(ObjectType type, float distance = 5f, float height = 0f)
        {
            return new SpawnPayload
            {
                commandId = Guid.NewGuid().ToString(),
                objectType = type.ToString(),
                spawnDistance = distance,
                heightOffset = height
            };
        }
    }

    [Serializable]
    public class UdpEnvelope
    {
        public string messageType;  // "SPAWN" | "REMOVE" | "CLEAR" | "HEARTBEAT"
        public string senderId;
        public string senderRole;   // "INSTRUCTOR" | "PLAYER"
        public long timestamp;
        public string payload;      // JSON-encoded SpawnPayload or commandId for REMOVE
    }
}
