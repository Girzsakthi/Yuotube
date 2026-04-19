using System;
using UnityEngine;

namespace ARMilitary.Data
{
    [Serializable]
    public class CatalogueEntry
    {
        public ObjectType objectType;
        public Category category;
        public string displayName;
        [TextArea(1, 2)]
        public string description;
        public Color tintColor = Color.white;
        public float defaultSpawnDistance = 5f;
        public float defaultHeightOffset = 0f;
    }
}
