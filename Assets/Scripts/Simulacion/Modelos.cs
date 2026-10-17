using UnityEngine;

namespace SimulacionSemaforo.Simulacion
{
    /// <summary>Autos, camionetas y buses armados con cajas y cilindros.</summary>
    public static class Modelos
    {
        public sealed class Tipo
        {
            public readonly string Nombre;
            public readonly float Largo, Ancho, Alto, Probabilidad;

            public Tipo(string nombre, float largo, float ancho, float alto, float probabilidad)
            {
                Nombre = nombre;
                Largo = largo;
                Ancho = ancho;
                Alto = alto;
                Probabilidad = probabilidad;
            }
        }

        public static readonly Tipo Auto = new Tipo("Auto", 4.2f, 1.8f, 1.4f, 0.7f);
        public static readonly Tipo Camioneta = new Tipo("Camioneta", 5.2f, 2f, 1.9f, 0.2f);
        public static readonly Tipo Bus = new Tipo("Bus", 11f, 2.5f, 3f, 0.1f);

        private static readonly Color[] Colores =
        {
            new Color(0.75f, 0.12f, 0.12f), new Color(0.15f, 0.3f, 0.65f), new Color(0.92f, 0.92f, 0.9f),
            new Color(0.2f, 0.2f, 0.22f), new Color(0.55f, 0.57f, 0.6f), new Color(0.95f, 0.75f, 0.1f),
        };

        public static Tipo Elegir(System.Random azar)
        {
            double r = azar.NextDouble();
            if (r < Bus.Probabilidad) return Bus;
            if (r < Bus.Probabilidad + Camioneta.Probabilidad) return Camioneta;
            return Auto;
        }

        public static Vehiculo Crear(Tipo tipo, Transform padre, System.Random azar)
        {
            var raiz = new GameObject(tipo.Nombre);
            raiz.transform.SetParent(padre, false);
            var color = tipo == Bus ? new Color(0.1f, 0.45f, 0.3f) : Colores[azar.Next(Colores.Length)];
            float alturaRueda = 0.35f;
            float carroceria = tipo == Auto ? tipo.Alto * 0.5f : tipo.Alto * 0.8f;

            Formas.Caja("Carrocería", raiz.transform, new Vector3(0, alturaRueda + carroceria / 2, 0),
                new Vector3(tipo.Ancho, carroceria, tipo.Largo), color);
            if (tipo != Bus)
            {
                float cabina = tipo.Alto - carroceria;
                Formas.Caja("Cabina", raiz.transform,
                    new Vector3(0, alturaRueda + carroceria + cabina / 2, tipo == Auto ? -0.2f : -0.6f),
                    new Vector3(tipo.Ancho * 0.85f, cabina, tipo.Largo * (tipo == Auto ? 0.5f : 0.6f)),
                    new Color(0.18f, 0.22f, 0.27f));
            }
            else
            {
                Formas.Caja("Ventanas", raiz.transform, new Vector3(0, alturaRueda + carroceria + 0.15f, 0),
                    new Vector3(tipo.Ancho * 1.01f, 0.6f, tipo.Largo * 0.9f), new Color(0.18f, 0.22f, 0.27f));
            }

            float ejes = tipo.Largo / 2 - 0.9f;
            foreach (float z in new[] { -ejes, ejes })
            foreach (float x in new[] { -tipo.Ancho / 2, tipo.Ancho / 2 })
            {
                var rueda = Formas.Caja("Rueda", raiz.transform, new Vector3(x, alturaRueda, z),
                    new Vector3(0.7f, 0.12f, 0.7f), new Color(0.08f, 0.08f, 0.08f), PrimitiveType.Cylinder);
                rueda.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            return raiz.AddComponent<Vehiculo>();
        }
    }
}
