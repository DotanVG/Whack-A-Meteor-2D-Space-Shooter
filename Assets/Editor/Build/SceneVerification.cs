using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Read-only serialized scene/prefab smoke check, safe for headless batch use.</summary>
public static class SceneVerification
{
    public static void Run()
    {
        int components = 0;
        foreach (string path in new[] { "Assets/Scenes/Game.unity", "Assets/Scenes/HyperdriveShop.unity" })
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                foreach (var root in scene.GetRootGameObjects()) components += Check(root, path);
                Debug.Log("[Phase2 QA] Scene PASS: " + path);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
        int prefabs = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) throw new BuildFailedException("Failed to load " + path);
            components += Check(prefab, path);
            prefabs++;
        }
        Debug.Log("[Phase2 QA] PASS scenes=2 prefabs=" + prefabs + " components=" + components);
    }

    private static int Check(GameObject root, string path)
    {
        var components = root.GetComponentsInChildren<Component>(true);
        foreach (var component in components)
        {
            if (!component) throw new BuildFailedException("Missing script: " + path);
            using (var serialized = new SerializedObject(component))
            {
                var property = serialized.GetIterator();
                while (property.Next(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                        throw new BuildFailedException("Missing reference: " + path + " " + component.name + "." + property.propertyPath);
            }
        }
        return components.Length;
    }
}
