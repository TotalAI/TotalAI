using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [Serializable]
    public class ItemVisualEffects
    {
        public enum Tag { None, Success, Failure }
        public ItemCondition itemCondition;

        [Serializable]
        public class GameObjectTag
        {
            public GameObject gameObject;
            public Tag tag;
        }

        public List<GameObjectTag> defaultGameObjectTags;

        [Serializable]
        public class PrefabVariantGameObjectsMappings
        {
            public int prefabVariantIndex;
            public List<GameObjectTag> gameObjectTags;
        }
        public List<PrefabVariantGameObjectsMappings> prefabVariantGameObjectsMappings;

        public List<GameObject> GetGameObjects(int prefabVariantIndex)
        {
            if (prefabVariantGameObjectsMappings == null || prefabVariantGameObjectsMappings.Count == 0)
                return defaultGameObjectTags.Select(x => x.gameObject).ToList();

            PrefabVariantGameObjectsMappings objectsMapping = prefabVariantGameObjectsMappings.Find(x => x.prefabVariantIndex == prefabVariantIndex);
            if (objectsMapping == null)
                return defaultGameObjectTags.Select(x => x.gameObject).ToList();

            return objectsMapping.gameObjectTags.Select(x => x.gameObject).ToList();
        }
    }
}
