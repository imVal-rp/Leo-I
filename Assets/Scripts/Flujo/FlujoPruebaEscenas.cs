using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// HERRAMIENTA TEMPORAL para probar el recorrido del juego.
/// Se crea sola al dar Play, por lo que no hay que agregarla a ninguna escena.
/// F2: pasa a la siguiente escena.
/// F3: abre Game Over.
/// La eliminaremos cuando existan las transiciones reales.
/// </summary>
public class FlujoPruebaEscenas : MonoBehaviour
{
    // Los nombres deben coincidir exactamente con los de las escenas.
    private static readonly string[] OrdenEscenas =
    {
        "Menu",
        "pueblo",
        "Bosque",
        "Hacienda_patio",
        "Hacienda_pasillo",
        "Hacienda_final",
        "Victoria"
    };

    private const string EscenaGameOver = "GameOver";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Crear()
    {
        GameObject objeto = new GameObject(nameof(FlujoPruebaEscenas));
        objeto.AddComponent<FlujoPruebaEscenas>();
        DontDestroyOnLoad(objeto);
    }

    private void Update()
    {
        Keyboard teclado = Keyboard.current;

        if (teclado == null)
        {
            return;
        }

        if (teclado.f2Key.wasPressedThisFrame)
        {
            CargarSiguienteEscena();
        }
        else if (teclado.f3Key.wasPressedThisFrame)
        {
            CargarEscena(EscenaGameOver);
        }
    }

    private void CargarSiguienteEscena()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        int indiceActual = Array.IndexOf(OrdenEscenas, escenaActual);

        // Si estamos en Game Over, en otra escena fuera de la lista o en Victoria,
        // el recorrido vuelve al inicio.
        int siguienteIndice = indiceActual < 0
            ? 0
            : (indiceActual + 1) % OrdenEscenas.Length;

        CargarEscena(OrdenEscenas[siguienteIndice]);
    }

    private void CargarEscena(string nombreEscena)
    {
        if (!Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            Debug.LogError(
                $"No se pudo cargar '{nombreEscena}'. Revisa que el nombre sea exacto " +
                "y que la escena esté en File > Build Profiles > Scene List.");
            return;
        }

        Debug.Log($"[Flujo de prueba] Cargando: {nombreEscena}");
        SceneManager.LoadScene(nombreEscena);
    }

    private void OnGUI()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        GUI.Label(
            new Rect(10, 10, 600, 25),
            $"Escena: {escenaActual} | F2: siguiente | F3: Game Over");
    }
}