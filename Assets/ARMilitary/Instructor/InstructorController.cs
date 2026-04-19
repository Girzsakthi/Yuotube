using System.Collections.Generic;
using System.Linq;
using ARMilitary.Data;
using ARMilitary.Shared;
using UnityEngine;

namespace ARMilitary.Instructor
{
    public class InstructorController : MonoBehaviour
    {
        private readonly HashSet<ObjectType> _selected = new HashSet<ObjectType>();
        private Category _activeCategory = Category.All;
        private ObjectCatalogue _catalogue;

        public ObjectCatalogue Catalogue => _catalogue;
        public Category ActiveCategory => _activeCategory;
        public IReadOnlyCollection<ObjectType> Selected => _selected;

        private void Awake()
        {
            _catalogue = ObjectCatalogue.CreateDefault();
        }

        private void Start()
        {
            if (NetworkManager.Instance != null)
                NetworkManager.Instance.SetRole(AppMode.Instructor);
        }

        public void SetCategory(Category category)
        {
            _activeCategory = category;
            _selected.Clear();
        }

        public void ToggleSelect(ObjectType type)
        {
            if (_selected.Contains(type))
                _selected.Remove(type);
            else
                _selected.Add(type);
        }

        public bool IsSelected(ObjectType type) => _selected.Contains(type);

        public void SendSelected()
        {
            if (_selected.Count == 0 || NetworkManager.Instance == null) return;

            var payloads = new List<SpawnPayload>();
            foreach (var type in _selected)
            {
                var entry = _catalogue.Get(type);
                payloads.Add(SpawnPayload.Create(
                    type,
                    entry?.defaultSpawnDistance ?? 5f,
                    entry?.defaultHeightOffset  ?? 0f));
            }

            NetworkManager.Instance.SendSpawnCommands(payloads);
            Debug.Log("[ARMilitary] Sent " + payloads.Count + " spawn commands.");
        }

        public void ClearAll()
        {
            NetworkManager.Instance?.SendClear();
            _selected.Clear();
        }
    }
}
