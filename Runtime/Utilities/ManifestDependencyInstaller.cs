using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomUIElements.Utilities
{
#if UNITY_EDITOR

    [InitializeOnLoad]
    public static class ManifestDependencyInstaller
    {
        // List of dependencies: name => git-url or version
        private static readonly Dictionary<string, string> dependenciesToAdd = new Dictionary<string, string>
        {
            // Add your dependencies here
        };

        static ManifestDependencyInstaller()
        {
            try
            {
                var projectPath = Directory.GetParent(Application.dataPath)?.FullName;
                if (projectPath == null) return;
                var manifestPath = Path.Combine(projectPath, "Packages", "manifest.json");
                if (!File.Exists(manifestPath))
                {
                    Debug.LogWarning("[ManifestDependencyInstaller] manifest.json not found.");
                    return;
                }

                string manifestText = File.ReadAllText(manifestPath);

                // Parse manifest.json using Newtonsoft.Json
                JObject manifest = JObject.Parse(manifestText);

                if (manifest["dependencies"] is not JObject deps)
                {
                    Debug.LogWarning("[ManifestDependencyInstaller] No 'dependencies' section found in manifest.json.");
                    return;
                }

                bool changed = false;
                foreach (var pair in dependenciesToAdd)
                {
                    if (!deps.ContainsKey(pair.Key))
                    {
                        deps[pair.Key] = pair.Value;
                        changed = true;
                        Debug.Log($"[ManifestDependencyInstaller] Added dependency: {pair.Key} => {pair.Value}");
                    }
                }

                if (changed)
                {
                    // Write updated manifest.json with indentation
                    string newJson = manifest.ToString(Formatting.Indented);
                    File.WriteAllText(manifestPath, newJson);
                    AssetDatabase.Refresh();
                    Debug.Log("[ManifestDependencyInstaller] manifest.json updated. Please restart Unity.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ManifestDependencyInstaller] Error: " + e);
            }
        }
    }
#endif
}