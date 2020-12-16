using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "TagType", menuName = "Total AI/Level Types/Tag Type", order = 0)]
    public class TagType : LevelType
    {
        [Header("Does this tag type have a level?")]
        public bool hasLevel;

        [Header("Does this tag type's level move between min and max values?")]
        public bool usesMinMax;
    }
}
