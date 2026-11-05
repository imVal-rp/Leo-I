using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla todo el recorrido del juego. Se crea solo al dar Play,
/// por lo que no hay que agregarlo a ninguna escena.
/// </summary>
public class Juego : MonoBehaviour
{
    public const string EscenaMenu = "Menu";
    public const string EscenaGameOver = "GameOver";
    public const string EscenaVictoria = "Victoria";

    private static readonly string[] Recorrido =
    {
        "Menu", "pueblo", "Bosque", "Hacienda_patio", "Hacienda_pasillo", "Hacienda_final", "Victoria"
    };

    public static Juego I { get; private set; }

    public Hud Hud { get; private set; }
    public LeoCombate Leo { get; private set; }
    public LeoController LeoMovimiento { get; private set; }
    public Transform LeoTransform { get; private set; }

    public float Suelo { get; private set; }
    public float LimiteIzquierdo { get; private set; }
    public float LimiteDerecho { get; private set; }

    public bool LeoVivo => Leo != null && !LeoMovimiento.Muerto;

    public float AlturaDeLeo =>
        LeoTransform != null ? LeoTransform.position.y - LeoFactory.DistanciaPies - Suelo : 0f;

    private Coroutine rutinaEscena;
    private bool cambiando;
    private string ultimaEscena = "pueblo";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Crear()
    {
        GameObject objeto = new GameObject(nameof(Juego));
        DontDestroyOnLoad(objeto);
        objeto.AddComponent<Juego>();
    }

    private void Awake()
    {
        I = this;
        Hud = gameObject.AddComponent<Hud>();
        gameObject.AddComponent<Sonido>();
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

#if UNITY_EDITOR
    // Atajo solo para probar en el editor: F2 salta a la siguiente escena.
    private void Update()
    {
        Keyboard teclado = Keyboard.current;

        if (teclado != null && teclado.f2Key.wasPressedThisFrame)
            SiguienteEscena();
    }
#endif

    public void IrA(string escena)
    {
        if (cambiando)
            return;

        if (!Application.CanStreamedLevelBeLoaded(escena))
        {
            Debug.LogError($"La escena '{escena}' no está en la lista de escenas del proyecto.");
            return;
        }

        StartCoroutine(CambiarEscena(escena));
    }

    public void SiguienteEscena()
    {
        int indice = System.Array.IndexOf(Recorrido, SceneManager.GetActiveScene().name);
        IrA(Recorrido[(indice + 1) % Recorrido.Length]);
    }

    public void Reintentar()
    {
        IrA(ultimaEscena);
    }

    /// <summary>Prepara el suelo, las paredes, la cámara y a Leo en la escena actual.</summary>
    public void PrepararNivel(float suelo, float izquierda, float derecha, Bounds camara, float inicioX, bool conMachete)
    {
        Suelo = suelo;
        LimiteIzquierdo = izquierda;
        LimiteDerecho = derecha;
        ultimaEscena = SceneManager.GetActiveScene().name;

        CrearLimites();

        GameObject anterior = GameObject.Find("Leo");

        if (anterior != null)
            DestroyImmediate(anterior);

        Leo = LeoFactory.Crear(new Vector2(inicioX, suelo));
        LeoMovimiento = Leo.GetComponent<LeoController>();
        LeoTransform = Leo.transform;

        Leo.EstablecerMachete(conMachete);
        Leo.AlCambiarVida += Hud.MostrarVida;
        Leo.AlMorir += AlMorirLeo;

        Camera principal = Camera.main;

        if (principal != null)
        {
            CamaraSeguir seguir = principal.GetComponent<CamaraSeguir>();

            if (seguir == null)
                seguir = principal.gameObject.AddComponent<CamaraSeguir>();

            seguir.Configurar(LeoTransform, camara);
        }

        Hud.MostrarInterfazDeJuego(true);
    }

    private void CrearLimites()
    {
        GameObject limites = new GameObject("LimitesDelNivel");

        float centro = (LimiteIzquierdo + LimiteDerecho) / 2f;
        float ancho = LimiteDerecho - LimiteIzquierdo + 12f;

        CrearCaja(limites, new Vector2(centro, Suelo - 1f), new Vector2(ancho, 2f));
        CrearCaja(limites, new Vector2(LimiteIzquierdo - 1f, Suelo + 10f), new Vector2(2f, 24f));
        CrearCaja(limites, new Vector2(LimiteDerecho + 1f, Suelo + 10f), new Vector2(2f, 24f));
    }

    public static BoxCollider2D CrearCaja(GameObject padre, Vector2 centro, Vector2 tamano)
    {
        BoxCollider2D caja = padre.AddComponent<BoxCollider2D>();
        caja.offset = centro;
        caja.size = tamano;
        return caja;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Leo = null;
        LeoMovimiento = null;
        LeoTransform = null;

        Hud.Reiniciar();
        Fabrica.PrepararMaterial();
        Sonido.MusicaDeEscena(escena.name);

        rutinaEscena = StartCoroutine(Guion.ParaEscena(escena.name));
    }

    private void AlMorirLeo()
    {
        if (!cambiando)
            StartCoroutine(TrasMorirLeo());
    }

    private IEnumerator TrasMorirLeo()
    {
        DetenerGuion();
        yield return new WaitForSeconds(1.6f);
        IrA(EscenaGameOver);
    }

    private void DetenerGuion()
    {
        if (rutinaEscena != null)
        {
            StopCoroutine(rutinaEscena);
            rutinaEscena = null;
        }
    }

    private IEnumerator CambiarEscena(string escena)
    {
        cambiando = true;
        DetenerGuion();

        yield return Hud.Fundir(1f, 0.5f);

        SceneManager.LoadScene(escena);
        yield return null;

        cambiando = false;
        yield return Hud.Fundir(0f, 0.5f);
    }
}
