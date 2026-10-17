using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SimulacionSemaforo.Editor
{
    /// <summary>
    /// Crea el material, la escena y compila la simulación. Se usa desde el menú
    /// "Simulación" o por línea de comandos:
    /// Unity -batchmode -quit -executeMethod SimulacionSemaforo.Editor.Construir.Linux
    /// </summary>
    public static class Construir
    {
        public const string Escena = "Assets/Escenas/Interseccion.unity";
        private const string Material = "Assets/Resources/Materiales/Base.mat";

        [MenuItem("Simulación/Crear escena")]
        public static void CrearEscena()
        {
            CrearMaterial();
            Directory.CreateDirectory(Path.GetDirectoryName(Escena));
            var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Simulación").AddComponent<SimulacionSemaforo.Simulacion.Simulacion>();
            EditorSceneManager.SaveScene(escena, Escena);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Escena, true) };
            AssetDatabase.SaveAssets();
        }

        /// <summary>Un solo material Standard; cada objeto lo colorea con MaterialPropertyBlock.</summary>
        private static void CrearMaterial()
        {
            if (File.Exists(Material)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(Material));
            var material = new Material(Shader.Find("Standard")) { enableInstancing = true };
            material.SetFloat("_Glossiness", 0.25f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            AssetDatabase.CreateAsset(material, Material);
        }

        [MenuItem("Simulación/Compilar para Linux")]
        public static void Linux() => Compilar(BuildTarget.StandaloneLinux64, "Builds/Linux/Simulacion.x86_64");

        [MenuItem("Simulación/Compilar para Windows")]
        public static void Windows() => Compilar(BuildTarget.StandaloneWindows64, "Builds/Windows/Simulacion.exe");

        private static void Compilar(BuildTarget destino, string salida)
        {
            if (!File.Exists(Escena)) CrearEscena();
            PlayerSettings.productName = "Simulación de semáforo";
            PlayerSettings.companyName = "Michael Lata";
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            var resultado = BuildPipeline.BuildPlayer(new[] { Escena }, salida, destino, BuildOptions.None);
            Debug.Log($"Compilación {destino}: {resultado.summary.result}, {resultado.summary.totalSize / 1024} KB");
            if (Application.isBatchMode && resultado.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }
    }
}
