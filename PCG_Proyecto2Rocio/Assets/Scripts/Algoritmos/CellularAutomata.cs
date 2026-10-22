using UnityEngine;

//Parámetros del autómata, editables en el Inspector y en el panel de la UI.
[System.Serializable]
public class CellularSettings
{
    public bool activar = true;// checkbox para prender/apagar el algoritmo

    //estado inicial: cada celda nace "viva" con esta probabilidad.
    //cerca de 0.45 el autómata produce las cuevas más interesantes: con valores bajos todo muere y con valores altos todo se llena.
    [Range(0f, 1f)] public float relleno = 0.42f;

    //cuántas veces se aplica la regla. Más iteraciones = formas más suaves y conectadas.
    [Range(0, 10)] public int iteraciones = 5;

    //Regla B/S (Birth/Survival): estos dos números definen todo el comportamiento.
    //- Una celda MUERTA se vuelve viva si tiene al menos "limiteNacimiento" vecinas vivas.
    //- Una celda VIVA sigue viva si tiene al menos "limiteSupervivencia" vecinas vivas.
    [Range(0, 8)] public int limiteNacimiento = 5;//vecinas vivas para que una celda muerta nazca
    [Range(0, 8)] public int limiteSupervivencia = 4;//vecinas vivas para que una celda viva sobreviva

    //Estos dos los usa MapRenderer para mostrar las cuevas solo en una franja del terreno (así dependen del ruido).
    [Header("Zona donde actúa (altura 0 a 1)")]
    [Range(0f, 1f)] public float alturaMinima = 0.70f;
    [Range(0f, 1f)] public float alturaMaxima = 1f;
}

//herramienta que recibe datos y devuelve una grilla.
public static class CellularAutomata
{
    // Devuelve una grilla de true/false. true = hueco (cueva o laguna).
    // La misma seed y los mismos parámetros siempre dan la misma grilla.
    public static bool[,] Generar(int ancho, int alto, int seed, CellularSettings s)
    {
        // System.Random con seed fija hace que el resultado sea reproducible
        System.Random rng = new System.Random(seed);
        bool[,] grilla = new bool[ancho, alto];

        //1) Llenado inicial al azar: es el "ruido" del que el autómata saca las formas
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                grilla[x, y] = rng.NextDouble() < s.relleno;
            }
        }

        //2) Repetimos la regla varias veces; cada pasada va agrupando las celdas en manchas
        for (int i = 0; i < s.iteraciones; i++)
        {
            //creamos una grilla NUEVA para guardar el resultado.
            //si cambiáramos la misma grilla que estamos leyendo, las celdas
            //ya modificadas afectarían a las siguientes y saldría un resultado raro.
            bool[,] nueva = new bool[ancho, alto];

            for (int x = 0; x < ancho; x++)
            {
                for (int y = 0; y < alto; y++)
                {
                    int vecinas = ContarVecinasVivas(grilla, x, y, ancho, alto);

                    //aplicamos la regla B/S según el estado actual de la celda
                    if (grilla[x, y])
                        nueva[x, y] = vecinas >= s.limiteSupervivencia; //estaba viva
                    else
                        nueva[x, y] = vecinas >= s.limiteNacimiento;    //estaba muerta
                }
            }

            grilla = nueva; //la nueva grilla pasa a ser la actual para la siguiente pasada
        }

        return grilla;
    }

    //cuenta cuántas de las 8 vecinas están vivas
    static int ContarVecinasVivas(bool[,] grilla, int x, int y, int ancho, int alto)
    {
        int cuenta = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // no cuenta a la celda misma

                int vx = x + dx;
                int vy = y + dy;

                // Si la vecina está fuera del mapa, la ignoramos (cuenta como muerta).
                // Por eso las celdas del borde tienen menos vecinas y tienden a morir.
                if (vx < 0 || vx >= ancho || vy < 0 || vy >= alto) continue;

                if (grilla[vx, vy]) cuenta++;
            }
        }
        return cuenta;
    }
}