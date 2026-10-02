using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Supono.Editor
{
    /// <summary>
    /// Keeps the SUPONO_FIREBASE scripting define in sync with the Firebase Analytics SDK: on when the SDK
    /// (imported as a .unitypackage, so no package version define can see it) is in the project, off when
    /// it's removed. The game then picks the Firebase analytics backend or the console one.
    /// </summary>
    [InitializeOnLoad]
    static class FirebaseDefine
    {
        const string Define = "SUPONO_FIREBASE";
        static FirebaseDefine()
        {
            bool sdkPresent = Type.GetType("Firebase.Analytics.FirebaseAnalytics, Firebase.Analytics") != null;
            // The editor (standalone) and whatever platform is currently selected for builds.
            NamedBuildTarget active = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            foreach (NamedBuildTarget target in new[] { NamedBuildTarget.Standalone, active }.Distinct())
            {
                string[] defines = PlayerSettings.GetScriptingDefineSymbols(target).Split(';', StringSplitOptions.RemoveEmptyEntries);
                bool hasDefine = defines.Contains(Define);
                if (hasDefine == sdkPresent) continue;

                string[] updated = sdkPresent ? defines.Append(Define).ToArray() : defines.Where(d => d != Define).ToArray();
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", updated));
                Debug.Log($"[Supono] {(sdkPresent ? "Enabled" : "Disabled")} {Define} for {target.TargetName}.");
            }
        }
    }
}
