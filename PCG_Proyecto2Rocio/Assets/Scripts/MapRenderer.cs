using UnityEngine;
using UnityEngine.UI; // necesario para usar RawImage
using System.Collections.Generic;   //para crear la lista de reglas del L-System

// Este script junta todos los algoritmos (ruido, autómata, L-System) y pinta el mapa final
public class MapRenderer : MonoBehaviour
{
    public RawImage pantalla; //el objeto de la UI donde se muestra la imagen del mapa
    public int ancho = 128;   //tamaño del mapa en pixeles
    public int alto = 128;
    public int seed = 1;      //semilla: misma seed = mismo mapa
    Texture2D texturaActual;   // para borrar la textura anterior y no acumular memoria

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

    void Awake()
    {
        CargarIsla(); // el programa parte con el contexto de la isla
    }

    public void CargarIsla()
    {
        // Ruido: pocas octavas = formas suaves. Nivel de agua alto = archipiélago
        ruido.escala = 45f;
        ruido.octavas = 3;
        ruido.persistencia = 0.45f;
        ruido.lacunaridad = 2f;
        ruido.desplazamiento = Vector2.zero;

        nivelAgua = 0.52f;
        nivelArena = 0.56f;
        nivelPasto = 0.74f;
        nivelRoca = 0.88f;

        // Autómata: lagunas interiores dentro de la selva
        automata.activar = true;
        automata.relleno = 0.42f;
        automata.iteraciones = 5;
        automata.limiteNacimiento = 5;
        automata.limiteSupervivencia = 4;
        automata.alturaMinima = 0.62f;
        automata.alturaMaxima = 0.74f;

        // L-System: palmeras (tronco recto + copa abierta). 
        lsystem.activar = true;
        lsystem.axioma = "TTTTX";
        lsystem.reglas = new List<ReglaLSystem>()
        {
            // Copa grande (se usa la mitad de las veces)
            new ReglaLSystem { simbolo = "X",
                reemplazo = "[+Lf+L][-Lf-L][++L][--L]L",
                probabilidad = 0.5f },
            // Copa chica (el resto de las veces)
            new ReglaLSystem { simbolo = "X",
                reemplazo = "[+Lf+L][-Lf-L]L",
                probabilidad = 1f }
        };
        lsystem.iteraciones = 2;
        lsystem.angulo = 60.18f;
        lsystem.variacionAngulo = 0.0f;
        lsystem.largoPaso = 1.67f;
        lsystem.anguloInicial = 90f;
        lsystem.radioHoja = 0;
        lsystem.cantidad = 22;
        lsystem.distanciaMinima = 9f;
        lsystem.alturaMinima = 0.56f;
        lsystem.alturaMaxima = 0.68f;

        // Colores
        colorAguaProfunda = Hex("1F5F8B");
        colorAgua = Hex("3FA7D6");
        colorArena = Hex("F2E2A8");
        colorPasto = Hex("3DAA4F");
        colorRoca = Hex("7D7A6B");
        colorNieve = Hex("D9D3C0");
        colorHueco = Hex("6FD0E8");   // laguna
        colorTronco = Hex("6B4423");
        colorHoja = Hex("1B6E2B");

        Generar();
    }

    public void CargarVolcan()
    {
        // Ruido: muchas octavas y persistencia alta = terreno rugoso. El "agua" es lava
        ruido.escala = 36f;
        ruido.octavas = 5;
        ruido.persistencia = 0.5f;
        ruido.lacunaridad = 2.2f;
        ruido.desplazamiento = Vector2.zero;

        nivelAgua = 0.32f;
        nivelArena = 0.36f;
        nivelPasto = 0.72f;
        nivelRoca = 0.88f;

        // Autómata: cavernas de magma, más iteraciones y relleno más denso
        automata.activar = true;
        automata.relleno = 0.47f;
        automata.iteraciones = 6;
        automata.limiteNacimiento = 5;
        automata.limiteSupervivencia = 4;
        automata.alturaMinima = 0.68f;
        automata.alturaMaxima = 1f;

        // L-System: árboles secos. Si la regla de X no "sale", esa rama se corta
        lsystem.activar = true;
        lsystem.axioma = "TX";
        lsystem.reglas = new List<ReglaLSystem>()
        {
            new ReglaLSystem { simbolo = "F", reemplazo = "FF", probabilidad = 1f },
            new ReglaLSystem { simbolo = "X", reemplazo = "F[+X][-X]", probabilidad = 0.8f }
        };
        lsystem.iteraciones = 4;
        lsystem.angulo = 42f;
        lsystem.variacionAngulo = 15f;
        lsystem.largoPaso = 0.9f;
        lsystem.anguloInicial = 90f;
        lsystem.radioHoja = 0;
        lsystem.cantidad = 16;
        lsystem.distanciaMinima = 12f;
        lsystem.alturaMinima = 0.38f;
        lsystem.alturaMaxima = 0.70f;

        // Colores
        colorAguaProfunda = Hex("8C1608");  // lava profunda
        colorAgua = Hex("E0401A");          // lava
        colorArena = Hex("3E2B2B");         // costra enfriada
        colorPasto = Hex("34343C");         // roca basáltica
        colorRoca = Hex("5A5A64");
        colorNieve = Hex("8E8884");         // ceniza en la cumbre
        colorHueco = Hex("FFD23F");         // magma
        colorTronco = Hex("CDBB9B");        // árbol seco, color hueso
        colorHoja = Hex("7A3E1D");

        Generar();
    }

    // Convierte un código como "3DAA4F" en un Color            
    Color Hex(string codigo)
    {
        Color c;
        if (!ColorUtility.TryParseHtmlString("#" + codigo, out c))
            c = Color.magenta;   // si el código está mal escrito, se ve magenta
        return c;
    }

    public void Generar()
    {
        float inicio = Time.realtimeSinceStartup; //para medir cuánto demora

        //terrenos con ruido: devuelve una matriz con la altura (0 a 1) de cada pixel
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

        int[] bandas = new int[5];   // cuántas celdas hay en cada banda de altura
        int celdasCueva = 0;

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                float h = alturas[x, y];
                Color c = ColorPorAltura(alturas[x, y]); // 1) color base del terreno
                bandas[IndiceBanda(h)]++;

                if (cuevas[x, y]) { c = colorHueco; celdasCueva++;  }// 2) si es cueva se pinta encima

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

        // Borramos la textura anterior para no acumular memoria
        if (texturaActual != null) Destroy(texturaActual);
        texturaActual = tex;
        pantalla.texture = tex; // mostramos la textura en la pantalla

        //Métricas: se escriben en la Console
        float ms = (Time.realtimeSinceStartup - inicio) * 1000f;
        float total = ancho * alto;

        Debug.Log("MÉTRICAS (" + ancho + "x" + alto + ", seed " + seed + ")\n" +
            "Agua/lava: " + (100f * bandas[0] / total).ToString("F1") + "%\n" +
            "Arena/costra: " + (100f * bandas[1] / total).ToString("F1") + "%\n" +
            "Pasto/basalto: " + (100f * bandas[2] / total).ToString("F1") + "%\n" +
            "Roca: " + (100f * bandas[3] / total).ToString("F1") + "%\n" +
            "Nieve/ceniza: " + (100f * bandas[4] / total).ToString("F1") + "%\n" +
            "Cuevas: " + (100f * celdasCueva / total).ToString("F1") + "%\n" +
            "Árboles: " + LSystemGenerator.arbolesColocados + "\n" +
            "Tiempo: " + ms.ToString("F1") + " ms");
    }

    // Dice en qué banda de altura cae un valor: 0 agua, 1 arena, 2 pasto, 3 roca, 4 nieve
    int IndiceBanda(float h)
    {
        if (h < nivelAgua) return 0;
        if (h < nivelArena) return 1;
        if (h < nivelPasto) return 2;
        if (h < nivelRoca) return 3;
        return 4;
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