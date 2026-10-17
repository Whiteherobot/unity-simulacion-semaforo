using SimulacionSemaforo.Logica;
using UnityEngine;
using UnityEngine.UI;

namespace SimulacionSemaforo.Simulacion
{
    /// <summary>
    /// Panel con la fase, el tiempo restante, las colas y el plan del ciclo. Es un
    /// Canvas en modo cámara para que también salga en las capturas.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        private Text _texto;

        private void Start()
        {
            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1f;
            var escala = canvasGo.GetComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1280, 720);

            var panel = new GameObject("Panel", typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.65f);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(16, -16);
            rect.sizeDelta = new Vector2(330, 172);

            var textoGo = new GameObject("Texto", typeof(Text));
            textoGo.transform.SetParent(panel.transform, false);
            _texto = textoGo.GetComponent<Text>();
            _texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _texto.fontSize = 17;
            _texto.lineSpacing = 1.15f;
            _texto.color = Color.white;
            _texto.supportRichText = true;
            var rt = _texto.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(14, 10);
            rt.offsetMax = new Vector2(-10, -10);
        }

        private void LateUpdate()
        {
            var sim = Simulacion.Instancia;
            if (sim == null || _texto == null) return;
            var s = sim.Semaforo;
            _texto.text =
                "<b><size=19>Semáforo adaptativo</size></b>\n" +
                $"Fase: {Nombre(s.Fase)} ({Mathf.CeilToInt(s.Restante)} s)\n" +
                $"Cola A: {sim.ViaA.Cola(),2}   luz {Nombre(s.LuzDe(Carril.A))}\n" +
                $"Cola B: {sim.ViaB.Cola(),2}   luz {Nombre(s.LuzDe(Carril.B))}\n" +
                $"Plan: verde A {s.Plan.VerdeA:0.#} s, B {s.Plan.VerdeB:0.#} s\n" +
                $"Ciclos completos: {s.Ciclos}";
        }

        private static string Nombre(Fase fase)
        {
            switch (fase)
            {
                case Fase.VerdeA: return "verde para A";
                case Fase.AmarilloA: return "amarillo para A";
                case Fase.VerdeB: return "verde para B";
                case Fase.AmarilloB: return "amarillo para B";
                default: return "todo rojo";
            }
        }

        private static string Nombre(Luz luz) => luz == Luz.Verde ? "verde" : luz == Luz.Amarilla ? "amarilla" : "roja";
    }
}
