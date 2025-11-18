// Assets/Editor/ARnatomySupportDump.cs
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class ARnatomySupportDump : EditorWindow
{
    string outPath = "ARnatomy_SupportDump.txt";
    bool includeInactive = true;
    bool dumpAllScenesInBuild = true;

    [MenuItem("Tools/ARnatomy/Gerar Suporte TXT")]
    static void Open() => GetWindow<ARnatomySupportDump>("ARnatomy Dump");

    void OnGUI()
    {
        GUILayout.Label("ARnatomy Support Dump", EditorStyles.boldLabel);
        outPath = EditorGUILayout.TextField("Arquivo de saída", outPath);
        includeInactive = EditorGUILayout.Toggle("Incluir objetos inativos", includeInactive);
        dumpAllScenesInBuild = EditorGUILayout.Toggle("Todas as cenas do Build", dumpAllScenesInBuild);

        if (GUILayout.Button("Gerar TXT"))
        {
            try
            {
                var sb = new StringBuilder(1<<20);
                DumpHeader(sb);
                DumpEnv(sb);
                DumpBuildSettings(sb);
                DumpPlayerSettings(sb);
                DumpTagsLayers(sb);
                DumpQuality(sb);
                DumpGraphics(sb);
                DumpVuforiaHints(sb);

                var scenesToDump = new List<string>();
                if (dumpAllScenesInBuild)
                {
                    foreach (var s in EditorBuildSettings.scenes)
                        if (s.enabled) scenesToDump.Add(s.path);
                }
                else
                {
                    scenesToDump.Add(EditorSceneManager.GetActiveScene().path);
                }

                foreach (var spath in scenesToDump.Distinct())
                {
                    if (string.IsNullOrEmpty(spath)) continue;
                    var scene = EditorSceneManager.OpenScene(spath, OpenSceneMode.Single);
                    sb.AppendLine($"\n=== SCENE: {scene.name} ({spath}) ===");
                    foreach (var root in scene.GetRootGameObjects())
                        DumpGameObjectRecursive(sb, root, 0, includeInactive);
                }

                File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);
                EditorUtility.DisplayDialog("OK", $"Dump salvo em: {Path.GetFullPath(outPath)}", "Fechar");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Erro", ex.ToString(), "Fechar");
            }
        }
    }

    void DumpHeader(StringBuilder sb)
    {
        sb.AppendLine("===== ARnatomy Support Dump =====");
        sb.AppendLine($"Data: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Unity: {Application.unityVersion}");
        sb.AppendLine($"Platform: {EditorUserBuildSettings.activeBuildTarget}");
        sb.AppendLine(new string('=', 40));
    }
    void DumpEnv(StringBuilder sb)
    {
        sb.AppendLine("\n--- Environment ---");
        sb.AppendLine($"ColorSpace: {PlayerSettings.colorSpace}");
        sb.AppendLine($"ScriptingBackend: {PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup)}");
        sb.AppendLine($"IL2CPP: {PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup) == ScriptingImplementation.IL2CPP}");
        sb.AppendLine($"ApiCompatibility: {PlayerSettings.GetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup)}");
    }
    void DumpBuildSettings(StringBuilder sb)
    {
        sb.AppendLine("\n--- Build Settings ---");
        sb.AppendLine($"Scenes in Build:");
        foreach (var s in EditorBuildSettings.scenes)
            sb.AppendLine($"  - {(s.enabled ? "[x]" : "[ ]")} {s.path}");
        sb.AppendLine($"Active Target: {EditorUserBuildSettings.activeBuildTarget}");
        sb.AppendLine($"Architecture: ARMv7={PlayerSettings.Android.targetArchitectures.HasFlag(AndroidArchitecture.ARMv7)} ARM64={PlayerSettings.Android.targetArchitectures.HasFlag(AndroidArchitecture.ARM64)}");
        sb.AppendLine($"InstallLocation: {PlayerSettings.Android.preferredInstallLocation}");
        sb.AppendLine($"MinSdk: {PlayerSettings.Android.minSdkVersion}  TargetSdk: {PlayerSettings.Android.targetSdkVersion}");
    }
    void DumpPlayerSettings(StringBuilder sb)
    {
        sb.AppendLine("\n--- Player Settings ---");
        sb.AppendLine($"CompanyName: {PlayerSettings.companyName}");
        sb.AppendLine($"ProductName: {PlayerSettings.productName}");
        sb.AppendLine($"PackageName: {PlayerSettings.applicationIdentifier}");
        sb.AppendLine($"MultithreadedRendering: {PlayerSettings.MTRendering}");
        sb.AppendLine($"GraphicsJobs: {PlayerSettings.graphicsJobs}");
        sb.AppendLine($"VSync: {QualitySettings.vSyncCount}");
        sb.AppendLine($"ResolutionDialog: (n/a mobile)");
    }
    void DumpTagsLayers(StringBuilder sb)
    {
        sb.AppendLine("\n--- Tags & Layers ---");
        sb.AppendLine("Layers:");
        for (int i=0;i<32;i++){
            string ln = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(ln)) sb.AppendLine($"  {i}: {ln}");
        }
        sb.AppendLine("Tags:");
        foreach (var t in UnityEditorInternal.InternalEditorUtility.tags)
            sb.AppendLine($"  - {t}");
    }
    void DumpQuality(StringBuilder sb)
    {
        sb.AppendLine("\n--- Quality ---");
        sb.AppendLine($"ActiveLevel: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        sb.AppendLine($"MSAA: {QualitySettings.antiAliasing}");
        sb.AppendLine($"Shadows: {QualitySettings.shadows}");
    }
    void DumpGraphics(StringBuilder sb)
    {
        sb.AppendLine("\n--- Graphics ---");
        sb.AppendLine($"ColorSpace: {PlayerSettings.colorSpace}");
#if UNITY_ANDROID
        sb.AppendLine($"GraphicsAPIs (Android): {string.Join(", ", PlayerSettings.GetGraphicsAPIs(BuildTarget.Android))}");
#endif
    }
    void DumpVuforiaHints(StringBuilder sb)
    {
        sb.AppendLine("\n--- Vuforia (hints) ---");
        // Não há API pública simples pra tudo; deixo lembretes:
        sb.AppendLine("Verifique no Inspector do ImageTarget: Database/Target/Scale.");
        sb.AppendLine("Confirme que Vuforia está habilitado nas XR Settings (se aplicável à sua versão).");
    }

    void DumpGameObjectRecursive(StringBuilder sb, GameObject go, int depth, bool includeInactiveGO)
    {
        if (!includeInactiveGO && !go.activeInHierarchy) return;
        string indent = new string(' ', depth*2);
        sb.AppendLine($"{indent}- {go.name} (active={go.activeSelf}, layer={LayerMask.LayerToName(go.layer)})");

        foreach (var c in go.GetComponents<Component>())
        {
            if (c == null) { sb.AppendLine($"{indent}  * Missing Script"); continue; }
            var t = c.GetType();
            sb.AppendLine($"{indent}  * {t.Name}");
            // campos públicos/serializáveis
            var fields = t.GetFields(System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance);
            foreach (var f in fields)
            {
                try
                {
                    object val = f.GetValue(c);
                    sb.AppendLine($"{indent}      - {f.Name} = {FormatValue(val)}");
                } catch {}
            }
            // propriedades simples legíveis
            var props = t.GetProperties(System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance)
                         .Where(p=>p.CanRead && p.GetIndexParameters().Length==0);
            int pcount=0;
            foreach (var p in props)
            {
                if (pcount>8) break; // evita flood
                try
                {
                    object val = p.GetValue(c, null);
                    if (val==null) continue;
                    if (val is string || val.GetType().IsValueType)
                    {
                        sb.AppendLine($"{indent}      - {p.Name} (prop) = {FormatValue(val)}");
                        pcount++;
                    }
                } catch {}
            }
        }
        // filhos
        foreach (Transform child in go.transform)
            DumpGameObjectRecursive(sb, child.gameObject, depth+1, includeInactiveGO);
    }

    string FormatValue(object v)
    {
        if (v == null) return "null";
        if (v is UnityEngine.Object uo) return uo ? $"{uo.name} ({uo.GetType().Name})" : "null (UnityObj)";
        if (v is string s) return $"\"{s}\"";
        if (v is Vector3 vec) return $"({vec.x:0.###},{vec.y:0.###},{vec.z:0.###})";
        if (v is Vector2 v2) return $"({v2.x:0.###},{v2.y:0.###})";
        if (v is Quaternion q) return $"({q.x:0.###},{q.y:0.###},{q.z:0.###},{q.w:0.###})";
        if (v is Color col) return $"RGBA({col.r:0.###},{col.g:0.###},{col.b:0.###},{col.a:0.###})";
        return v.ToString();
    }
}
