using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ARMilitary.Data
{
    [CreateAssetMenu(fileName = "ObjectCatalogue", menuName = "ARMilitary/Object Catalogue")]
    public class ObjectCatalogue : ScriptableObject
    {
        public List<CatalogueEntry> entries = new List<CatalogueEntry>
        {
            new CatalogueEntry { objectType = ObjectType.Drone,  category = Category.Air,    displayName = "DRONE",  tintColor = new Color(0.4f,0.8f,0.4f), defaultHeightOffset = 2f },
            new CatalogueEntry { objectType = ObjectType.Jet,    category = Category.Air,    displayName = "JET",    tintColor = new Color(0.4f,0.6f,1.0f), defaultHeightOffset = 5f },
            new CatalogueEntry { objectType = ObjectType.Tanker, category = Category.Ground, displayName = "TANKER", tintColor = new Color(0.6f,0.5f,0.3f), defaultSpawnDistance = 8f },
            new CatalogueEntry { objectType = ObjectType.Bunker, category = Category.Ground, displayName = "BUNKER", tintColor = new Color(0.5f,0.5f,0.5f), defaultSpawnDistance = 6f }
        };

        public IEnumerable<CatalogueEntry> Filter(Category category)
        {
            return category == Category.All
                ? entries
                : entries.Where(e => e.category == category);
        }

        public CatalogueEntry Get(ObjectType type) =>
            entries.FirstOrDefault(e => e.objectType == type);

        public static ObjectCatalogue CreateDefault()
        {
            var so = CreateInstance<ObjectCatalogue>();
            return so;
        }
    }
}
