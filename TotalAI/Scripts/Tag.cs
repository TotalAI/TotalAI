using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public class Tag : Level
    {
        private TagType tagType;
        public Entity relatedEntity;

        public Tag(TagType tagType, Entity relatedEntity, float level)
        {
            levelType = tagType;
            this.tagType = tagType;
            this.relatedEntity = relatedEntity;
            this.level = level;
        }

        public override float GetLevel()
        {
            return level;
        }

        public override float ChangeLevel(float amount)
        {
            return amount;
        }

        public override string ToString()
        {
            return tagType.name + ": " + (relatedEntity == null ? "None" : relatedEntity.name);
        }
    }
}
