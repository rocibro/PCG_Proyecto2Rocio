using UnityEngine;
using UnityEngine.UI; // necesario para usar RawImage

// Este script junta todos los algoritmos (ruido, autómata, L-System) y pinta el mapa final
public class MapRenderer : MonoBehaviour
{
    public RawImage pantalla; //el objeto de la UI donde se muestra la imagen del mapa
    public int ancho = 128;   //tamaño del mapa en pixeles
    public int alto = 128;
    public int seed = 1;      //semilla: misma seed = mismo mapa

    [Header("Ruido")]
    public NoiseSettings ruido; //parámetros del generador de terreno (otro script)

    [Header("Autómata celular")]
    public CellularSettings automata;
    public Color colorHueco = new Color(0.1f, 0.1f, 0.1f); //color de las cuevas

    [Header("L-System")]
    public LSystemSettings lsystem;
    public Color colorTronco = new Color(0.4f, 0.25f, 0.1f, 1f);
    public Color colorHoja = new Color(0.05f, 0.4f, 0.1f, 1f);

    [Header("Umbrales de altura (0 a 1)")]
    //si la altura es menor que el umbral, se pinta con ese color (se ve más abajo en ColorPorAltura)
    [Range(0f, 1f)] public float nivelAgua = 0.40f;
    [Range(0f, 1f)] public float nivelArena = 0.45f;
    [Range(0f, 1f)] public float nivelPasto = 0.70f;
    [Range(0f, 1f)] public float nivelRoca = 0.88f;

    [Header("Colores")]
    public Color colorAguaProfunda = new Color(0.1f, 0.2f, 0.6f);
    public Color colorAgua = new Color(0.2f, 0.4f, 0.8f);
    public Color colorArena = new Color(0.9f, 0.85f, 0.55f);
    public Color colorPasto = new Color(0.2f, 0.6f, 0.2f);
    public Color colorRoca = new Color(0.5f, 0.5f, 0.5f);
    public Color colorNieve = Color.white;

    void Start()
    {
        Generar(); //al darle play se genera el primer mapa
    }

    void Update()
    {
        // Barra espaciadora = nueva seed al azar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            seed = Random.Range(0, 100000);
            Generar();
        }
        // Tecla G = regenerar con los valores actuales
        if (Input.GetKeyDown(KeyCode.G))
        {
            Generar();
        }
    }

    public void Generar()
    {
        //terrenos con ruido
        // devuelve una matriz con la altura (0 a 1) de cada pixel
        float[,] alturas = NoiseGenerator.Generar(ancho, alto, seed, ruido);

        //cuevas: el autómata solo cuenta dentro de su rango de altura
        bool[,] cuevas = new bool[ancho, alto]; // si el autómata está apagado queda todo en false
        if (automata.activar)
        {
            bool[,] huecos = CellularAutomata.Generar(ancho, alto, seed, automata);
            for (int x = 0; x < ancho; x++)
            {
                for (int y = 0; y < alto; y++)
                {
                    float h = alturas[x, y];
                    // es cueva solo si hay hueco Y la altura está dentro del rango permitido
                    cuevas[x, y] = huecos[x, y]
                        && h >= automata.alturaMinima && h <= automata.alturaMaxima;
                }
            }
        }

        // los árboles necesitan las alturas y saber dónde hay cuevas
        // (por eso se generan después de las cuevas, para no poner árboles adentro)
        int[,] arboles = null; // null = no hay árboles
        if (lsystem.activar)
        {
            arboles = LSystemGenerator.Generar(ancho, alto, seed, lsystem, alturas, cuevas);
        }

        //pintamos todo, de abajo hacia arriba: terreno, cuevas, árboles
        // (lo que se pinta después tapa a lo anterior)
        Texture2D tex = new Texture2D(ancho, alto);
        tex.filterMode = FilterMode.Point; // Point para que se vean los pixeles nítidos y no borrosos

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                float h = alturas[x, y];
                Color c = ColorPorAltura(alturas[x, y]); // 1) color base del terreno

                if (cuevas[x, y]) c = colorHueco; // 2) si es cueva se pinta encima

                // 3) árboles encima de todo (solo si el L-System estaba activado)
                if (arboles != null)
                {
                    if (arboles[x, y] == LSystemGenerator.RAMA) c = colorTronco;
                    if (arboles[x, y] == LSystemGenerator.HOJA) c = colorHoja;
                }

                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(); // sin esto los SetPixel no se aplican a la textura
        pantalla.texture = tex; // mostramos la textura en la pantalla
    }

    // devuelve el color según la altura, se va revisando de menor a mayor
    Color ColorPorAltura(float h)
    {
        if (h < nivelAgua * 0.6f) return colorAguaProfunda; // la parte más baja del agua
        if (h < nivelAgua) return colorAgua;
        if (h < nivelArena) return colorArena;
        if (h < nivelPasto) return colorPasto;
        if (h < nivelRoca) return colorRoca;
        return colorNieve; // lo más alto
    }
}