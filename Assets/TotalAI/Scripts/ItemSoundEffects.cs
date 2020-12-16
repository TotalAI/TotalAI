using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [Serializable]
    public class ItemSoundEffects
    {
        public enum Tag { None, Success, Failure }
        public ItemCondition itemCondition;

        [Serializable]
        public class AudioClipTag
        {
            public AudioClip audioClip;
            public Tag tag;
        }

        public List<AudioClipTag> defaultAudioClipTags;

        [Serializable]
        public class PrefabVariantClipsMapping
        {
            public int prefabVariantIndex;
            public List<AudioClipTag> audioClipTags;
        }
        public List<PrefabVariantClipsMapping> prefabVariantClipsMappings;

        public List<AudioClip> GetAudioClips(int prefabVariantIndex, int tagEnumIndex)
        {
            if (prefabVariantClipsMappings == null || prefabVariantClipsMappings.Count == 0)
                return defaultAudioClipTags.Where(x => tagEnumIndex == -1 || x.tag == (Tag)tagEnumIndex).Select(x => x.audioClip).ToList();

            PrefabVariantClipsMapping clipsMapping = prefabVariantClipsMappings.Find(x => x.prefabVariantIndex == prefabVariantIndex);
            if (clipsMapping == null)
                return defaultAudioClipTags.Where(x => tagEnumIndex == -1 || x.tag == (Tag)tagEnumIndex).Select(x => x.audioClip).ToList();

            return clipsMapping.audioClipTags.Where(x => tagEnumIndex == -1 || x.tag == (Tag)tagEnumIndex).Select(x => x.audioClip).ToList();
        }
    }
}
