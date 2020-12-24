using UnityEngine;
using UnityEditor;

namespace TotalAI
{
    // These two Asset callbacks handle keeping the actionType.mappingTypes lists up to date

    public class TAIAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
        {
            if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(MappingType))
            {
                MappingType mappingType = AssetDatabase.LoadAssetAtPath<MappingType>(path);
                ActionType actionType = mappingType.actionType;

                if (actionType != null)
                {
                    if (actionType.mappingTypes.Contains(mappingType))
                    {
                        actionType.mappingTypes.Remove(mappingType);
                        EditorUtility.SetDirty(actionType);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
            return AssetDeleteResult.DidNotDelete;
        }
    }

    class TAIPostProcessAssets : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(MappingType))
                {
                    MappingType mappingType = AssetDatabase.LoadAssetAtPath<MappingType>(path);
                    ActionType actionType = mappingType.actionType;

                    if (actionType != null)
                    {
                        if (!actionType.mappingTypes.Contains(mappingType))
                        {
                            actionType.mappingTypes.Add(mappingType);
                            EditorUtility.SetDirty(actionType);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }
                }
            }
        }
    }
}