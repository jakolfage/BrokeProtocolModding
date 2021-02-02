using UnityEngine;
using UnityEditor;
using BrokeProtocol.Required;

namespace BrokeProtocol.EditorTools
{
    [InitializeOnLoad]
    public static class LayerUtils
    {
        static readonly string[] tagNames = new string[]
        {
            ObjectTag.nonStaticTag,
            ObjectTag.grassTag,
            ObjectTag.climbableTag,
            ObjectTag.junctionTag,
            ObjectTag.interSectionTag,
            ObjectTag.trafficLightTag,
            ObjectTag.folderTag,
        };

        static readonly string[] layerNames = new string[]
        {
            "OnlyRaycast",
            "Player",
            "Physical",
            "Simple",
            "Map",
            "Prop",
            "Shield",
            "Trigger",
            "Bullet",
            "Fire",
            "OnlyRaycastFP"
        };

        static LayerUtils()
        {
            CreateTagsLayers();
        }

        [MenuItem("Tools/Create Tags and Layers")]
        static void CreateTagsLayers()
        {
            Object tagManager = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset");

            if(!tagManager) return;


            SerializedObject manager = new SerializedObject(tagManager);
            SerializedProperty tagsProp = manager.FindProperty("tags");

            tagsProp.ClearArray();
            tagsProp.arraySize = tagNames.Length;

            int index = 0;
            foreach (string name in tagNames)
            {
                SerializedProperty p = tagsProp.GetArrayElementAtIndex(index);
                p.stringValue = name;
                index++;
            }

            SerializedProperty layersProp = manager.FindProperty("layers");

            layersProp.ClearArray();
            layersProp.arraySize = layerNames.Length + Consts.fixedLayerCount;

            index = 0;
            foreach (string name in layerNames)
            {
                SerializedProperty p = layersProp.GetArrayElementAtIndex(index + Consts.fixedLayerCount);
                p.stringValue = name;
                index++;
            }

            manager.ApplyModifiedProperties();

            manager.Dispose();
        }
    }
}