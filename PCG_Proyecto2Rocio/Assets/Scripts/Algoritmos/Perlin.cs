using UnityEngine;

public static class PerlinPropio
{
    // tabla de permutación: son los números del 0 al 255 mezclados, copiados 2 veces (512)
    static int[] perm = new int[512];

    // prepara la tabla con la seed 
    public static void Inicializar(int seed)
    {
        // primero se llena una tabla con los números del 0 al 255
        int[] p = new int[256];
        for (int i = 0; i < 256; i++) p[i] = i;

        // mezcla de los números usando la seed
        System.Random rng = new System.Random(seed);
        for (int i = 255; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int t = p[i]; 
            p[i] = p[j]; 
            p[j] = t;
        }

        // copia la tabla 2 veces para poder usar índices que pasan el 255
        for (int i = 0; i < 512; i++) perm[i] = p[i & 255];
    }

    // polinomio de suavizado de Perlin: 6t^5 - 15t^4 + 10t^3
    // hace que la transición entre una celda y otra sea suave y no se vean cuadrados
    static float Fade(float t) { return t * t * t * (t * (t * 6f - 15f) + 10f); }

    // elige una dirección (gradiente) según el hash y la multiplica con la posición del punto
    // aquí se usan los últimos 3 bits del hash, o sea 8 direcciones posibles:
    // las 4 diagonales y las 4 de los ejes 
    static float Grad(int hash, float x, float y)
    {
        switch (hash & 7)
        {
            case 0: return x + y;   // diagonal (1, 1)
            case 1: return -x + y;  // diagonal (-1, 1)
            case 2: return x - y;   // diagonal (1, -1)
            case 3: return -x - y;  // diagonal (-1, -1)
            case 4: return x;       // eje (1, 0)
            case 5: return -x;      // eje (-1, 0)
            case 6: return y;       // eje (0, 1)
            default: return -y;     // eje (0, -1)
        }
    }

    // Perlin Noise en un punto (x, y)
    // devuelve aprox. -1 a 1
    public static float Ruido(float x, float y)
    {
        // esquina de la celda donde está el punto
        int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);

        // parte decimal: qué tan adentro de la celda está el punto (entre 0 y 1)
        float xf = x - xi, yf = y - yi;

        // & 255 para no salirse de la tabla (también funciona con números negativos)
        int X = xi & 255, Y = yi & 255;

        // suaviza las coordenadas
        float u = Fade(xf), v = Fade(yf);

        // hash de las cuatro esquinas de la celda
        int aa = perm[perm[X] + Y], ab = perm[perm[X] + Y + 1];
        int ba = perm[perm[X + 1] + Y], bb = perm[perm[X + 1] + Y + 1];

        // gradiente de cada esquina (xf - 1 e yf - 1 son la distancia a la esquina de la derecha / de arriba)
        // se interpola en horizontal (x1 abajo, x2 arriba)
        float x1 = Mathf.Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
        float x2 = Mathf.Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

        //y después en vertical
        return Mathf.Lerp(x1, x2, v);
    }
}