using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace SimulacionSemaforo.Simulacion
{
    /// <summary>
    /// Con "-capturas carpeta" la simulación guarda imágenes en momentos fijos y
    /// se cierra. Sirve para revisarla sin abrirla y para el README.
    /// </summary>
    public class Capturas : MonoBehaviour
    {
        private string _carpeta;

        public static void ActivarSiCorresponde(Simulacion sim)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-capturas");
            if (i < 0 || i + 1 >= args.Length) return;
            sim.gameObject.AddComponent<Capturas>()._carpeta = args[i + 1];
        }

        private IEnumerator Start()
        {
            Directory.CreateDirectory(_carpeta);
            Time.captureFramerate = 30;   // paso fijo: la simulación corre igual en cualquier máquina
            foreach (var (segundo, nombre) in new[] { (20f, "inicio"), (45f, "cola-en-rojo"), (80f, "nuevo-ciclo") })
            {
                while (Time.time < segundo) yield return null;
                yield return new WaitForEndOfFrame();
                Guardar(Path.Combine(_carpeta, nombre + ".png"));
                Debug.Log($"[Capturas] {nombre} en {Time.time:0} s: {Simulacion.Instancia.Semaforo.Fase}, " +
                          $"colas {Simulacion.Instancia.ViaA.Cola()}/{Simulacion.Instancia.ViaB.Cola()}");
                yield return null;
            }
            yield return null;
            Application.Quit();
        }

        /// <summary>Renderiza la cámara a una textura: funciona también con -batchmode, sin ventana.</summary>
        private static void Guardar(string ruta)
        {
            const int ancho = 1280, alto = 720;
            var camara = Camera.main;
            var rt = new RenderTexture(ancho, alto, 24) { antiAliasing = 4 };
            camara.targetTexture = rt;
            camara.Render();
            RenderTexture.active = rt;
            var imagen = new Texture2D(ancho, alto, TextureFormat.RGB24, false);
            imagen.ReadPixels(new Rect(0, 0, ancho, alto), 0, 0);
            imagen.Apply();
            camara.targetTexture = null;
            RenderTexture.active = null;
            File.WriteAllBytes(ruta, imagen.EncodeToPNG());
            Destroy(rt);
            Destroy(imagen);
        }
    }
}
