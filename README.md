# PCG Tarea 2 - Generador procedural de mundos 2D

Proyecto de la Tarea 2 del curso **IA y PCG para videojuegos**.
Autora: Rocio Rojas Orellana.

## ¿Qué es?

Es un generador de mapas 2D hecho en Unity. Combina tres técnicas de generación procedural,
y cada una genera una parte del mapa:

| Técnica | Qué genera |
|---|---|
| Ruido fractal (Perlin hecho a mano, con octavas) | El terreno (agua, arena, pasto, roca y nieve) |
| Autómata celular | Cuevas, lagunas o magma |
| L-System estocástico | Árboles (palmeras o árboles secos) |

Tiene dos contextos que usan el mismo código y solo cambian los parámetros:
**isla tropical** y **mundo volcánico**.

## Versión jugable (sin instalar nada)

Se puede probar en el navegador:
https://itsrocibro.itch.io/tarea2-rociorojas

## Requisitos

- Unity Hub
- Unity **2022.3.62f2** (sirve cualquier 2022.3)
- Git (opcional, solo si se clona con comandos)

## Instalación

1. Descargar el proyecto con git:

```
git clone https://github.com/rocibro/PCG_Proyecto2Rocio.git
```

   También se puede descargar con el botón verde **Code → Download ZIP** y descomprimir.

2. Abrir **Unity Hub**, ir a **Projects → Add → Add project from disk**.
3. Elegir la carpeta `PCG_Proyecto2Rocio` (la que tiene dentro `Assets` y `ProjectSettings`).
4. Abrir el proyecto con Unity 2022.3 (la primera vez tarda un rato).
5. En la ventana Project, abrir la escena `Assets/Scenes/PCG_Proyecto.unity`.
6. Presionar el botón **Play**.

## Cómo usarlo

El menú está a la derecha del mapa:

- **Desplegable:** cambia entre isla tropical y mundo volcánico.
- **Campo de seed:** con el botón **Generar** se obtiene siempre el mismo mundo para esa seed.
- **Seed aleatoria:** crea un mundo nuevo.
- **Sliders:** modifican los parámetros del ruido, las alturas, el autómata y el L-System.
  El mapa se regenera al mover cualquiera.
- **Interruptores:** activan o desactivan el autómata y los árboles.

## Archivos principales

```
PCG_Proyecto2Rocio/Assets/
  Scenes/PCG_Proyecto.unity       escena del proyecto
  Scripts/MapRenderer.cs          junta los 3 algoritmos, define los contextos y dibuja el mapa
  Scripts/Algoritmos/             Perlin.cs, NoiseGenerator.cs, CellularAutomata.cs, L-System.cs
  Scripts/UI/                     MenuController.cs y PanelParametros.cs (menú y sliders)
```

## Notas

- Cada vez que se genera un mapa, las métricas (porcentaje de terreno, cuevas, árboles y
  tiempo) se escriben en la Console de Unity.
- Para exportar a la web: File → Build Settings → WebGL, con Compression Format en Disabled.
