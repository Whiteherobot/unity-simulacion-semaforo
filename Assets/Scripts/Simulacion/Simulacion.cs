using System;
using SimulacionSemaforo.Logica;
using UnityEngine;

namespace SimulacionSemaforo.Simulacion
{
    /// <summary>
    /// Arma la intersección (calles, semáforos, cámara y luz) y la hace andar.
    /// El carril A va de oeste a este y el B de sur a norte; cada uno tiene una
    /// "cámara" que cuenta su cola y el semáforo reparte el verde con ese conteo.
    /// </summary>
    public class Simulacion : MonoBehaviour
    {
        public const float MedioCruce = 4f;   // la intersección mide 8 x 8 m

        [Tooltip("Segundos promedio entre autos que llegan por el carril A.")]
        public float IntervaloA = 2.6f;
        [Tooltip("Segundos promedio entre autos que llegan por el carril B.")]
        public float IntervaloB = 6f;
        public int Semilla = 7;

        public static Simulacion Instancia { get; private set; }
        public Semaforo Semaforo { get; private set; }
        public Via ViaA { get; private set; }
        public Via ViaB { get; private set; }

        private PosteSemaforo _posteA, _posteB;

        private void Awake()
        {
            Instancia = this;
            ConstruirEntorno();
            float linea = Via.Largo / 2 - MedioCruce - 1f;
            ViaA = CrearVia(Carril.A, new Vector3(-Via.Largo / 2, 0, -1.8f), Vector3.right, linea, IntervaloA, Semilla);
            ViaB = CrearVia(Carril.B, new Vector3(1.8f, 0, -Via.Largo / 2), Vector3.forward, linea, IntervaloB, Semilla + 1);
            Semaforo = new Semaforo(new Parametros(), () => (ViaA.Cola(), ViaB.Cola()));
            _posteA = PosteSemaforo.Crear(transform, Carril.A, new Vector3(-MedioCruce - 1.5f, 0, -MedioCruce - 0.8f), Vector3.left);
            _posteB = PosteSemaforo.Crear(transform, Carril.B, new Vector3(MedioCruce + 0.8f, 0, -MedioCruce - 1.5f), Vector3.back);
            gameObject.AddComponent<Hud>();
            Capturas.ActivarSiCorresponde(this);
        }

        private void Update()
        {
            Semaforo.Avanzar(Time.deltaTime);
            _posteA.Mostrar(Semaforo.LuzDe(Carril.A));
            _posteB.Mostrar(Semaforo.LuzDe(Carril.B));
        }

        public Luz LuzDe(Carril carril) => Semaforo.LuzDe(carril);

        /// <summary>Solo para pruebas: cambia el semáforo por uno preparado.</summary>
        public void UsarSemaforo(Semaforo semaforo) => Semaforo = semaforo;

        private Via CrearVia(Carril carril, Vector3 inicio, Vector3 direccion, float linea, float intervalo, int semilla)
        {
            var via = new GameObject($"Vía {carril}").AddComponent<Via>();
            via.transform.SetParent(transform, false);
            via.Iniciar(carril, inicio, direccion, linea, intervalo, LuzDe, semilla);
            return via;
        }

        private void ConstruirEntorno()
        {
            var entorno = new GameObject("Entorno").transform;
            entorno.SetParent(transform, false);
            var pasto = new Color(0.36f, 0.5f, 0.3f);
            var asfalto = new Color(0.2f, 0.2f, 0.22f);
            var pintura = new Color(0.92f, 0.92f, 0.88f);
            var vereda = new Color(0.62f, 0.62f, 0.6f);

            Formas.Caja("Suelo", entorno, new Vector3(0, -0.06f, 0), new Vector3(200, 0.1f, 200), pasto);
            Formas.Caja("Calle A", entorno, Vector3.zero, new Vector3(Via.Largo, 0.02f, 8), asfalto);
            Formas.Caja("Calle B", entorno, new Vector3(0, 0.001f, 0), new Vector3(8, 0.02f, Via.Largo), asfalto);

            // Veredas en las cuatro esquinas.
            foreach (int sx in new[] { -1, 1 })
            foreach (int sz in new[] { -1, 1 })
            {
                float centro = (Via.Largo / 2 + MedioCruce) / 2;
                Formas.Caja("Vereda", entorno, new Vector3(sx * centro, 0.06f, sz * 5.5f), new Vector3(Via.Largo / 2 - MedioCruce, 0.15f, 3), vereda);
                Formas.Caja("Vereda", entorno, new Vector3(sx * 5.5f, 0.06f, sz * centro), new Vector3(3, 0.15f, Via.Largo / 2 - MedioCruce), vereda);
            }

            // Línea central discontinua y líneas de pare.
            for (float d = MedioCruce + 2; d < Via.Largo / 2; d += 6)
            foreach (int s in new[] { -1, 1 })
            {
                Formas.Caja("Raya", entorno, new Vector3(s * d, 0.015f, 0), new Vector3(3, 0.01f, 0.15f), pintura);
                Formas.Caja("Raya", entorno, new Vector3(0, 0.015f, s * d), new Vector3(0.15f, 0.01f, 3), pintura);
            }
            Formas.Caja("Pare A", entorno, new Vector3(-MedioCruce - 1f, 0.016f, -2f), new Vector3(0.4f, 0.01f, 4), pintura);
            Formas.Caja("Pare B", entorno, new Vector3(2f, 0.016f, -MedioCruce - 1f), new Vector3(4, 0.01f, 0.4f), pintura);

            // Edificios para dar escala, lejos de las vías de llegada (oeste y sur), que es lo que
            // mira la cámara.
            var azar = new System.Random(3);
            foreach (var (sx, sz) in new[] { (1, 1), (-1, 1), (1, -1) })
            for (int i = 0; i < 3; i++)
            {
                float alto = 5 + (float)azar.NextDouble() * 10;
                float tono = 0.6f + (float)azar.NextDouble() * 0.25f;
                Formas.Caja("Edificio", entorno, new Vector3(sx * (16 + i * 13), alto / 2, sz * 17), new Vector3(10, alto, 9),
                    new Color(tono, tono * 0.95f, tono * 0.88f));
            }

            var sol = new GameObject("Sol").AddComponent<Light>();
            sol.type = LightType.Directional;
            sol.intensity = 1.1f;
            sol.shadows = LightShadows.Soft;
            sol.transform.rotation = Quaternion.Euler(50, -35, 0);
            sol.transform.SetParent(entorno, false);
            RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);

            var camara = Camera.main != null ? Camera.main : new GameObject("Cámara").AddComponent<Camera>();
            camara.tag = "MainCamera";
            camara.transform.position = new Vector3(-42, 40, -42);
            camara.transform.LookAt(new Vector3(-12, 0, -12));
            camara.fieldOfView = 50;
            camara.backgroundColor = new Color(0.62f, 0.75f, 0.88f);
            camara.clearFlags = CameraClearFlags.SolidColor;
        }
    }
}
