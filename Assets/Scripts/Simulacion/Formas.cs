using UnityEngine;

namespace SimulacionSemaforo.Simulacion
{
    /// <summary>
    /// Crea los objetos de la escena con primitivas de Unity y un solo material
    /// (Resources/Materiales/Base), coloreado con MaterialPropertyBlock.
    /// </summary>
    public static class Formas
    {
        private static Material _base;
        private static readonly int Color_ = Shader.PropertyToID("_Color");
        private static readonly int Emision = Shader.PropertyToID("_EmissionColor");

        public static Material Base => _base != null ? _base : _base = Resources.Load<Material>("Materiales/Base");

        public static GameObject Caja(string nombre, Transform padre, Vector3 posicion, Vector3 escala, Color color,
            PrimitiveType tipo = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(tipo);
            go.name = nombre;
            go.transform.SetParent(padre, false);
            go.transform.localPosition = posicion;
            go.transform.localScale = escala;
            // Los choques se resuelven por distancia en Vehiculo; los colliders sobran.
            Object.Destroy(go.GetComponent<Collider>());
            var render = go.GetComponent<Renderer>();
            render.sharedMaterial = Base;
            Pintar(render, color);
            return go;
        }

        public static void Pintar(Renderer render, Color color, float brillo = 0)
        {
            var bloque = new MaterialPropertyBlock();
            render.GetPropertyBlock(bloque);
            bloque.SetColor(Color_, color);
            bloque.SetColor(Emision, color * brillo);
            render.SetPropertyBlock(bloque);
        }
    }
}
