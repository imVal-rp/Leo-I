using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Toda la interfaz del juego. Se construye por código y dura toda la partida.</summary>
public class Hud : MonoBehaviour
{
    private static readonly Color ColorPanel = new Color(0.06f, 0.05f, 0.08f, 0.93f);
    private static readonly Color ColorAcento = new Color(0.82f, 0.36f, 0.14f, 1f);
    private static readonly Color ColorCrema = new Color(0.95f, 0.87f, 0.73f, 1f);

    private Font fuente;
    private RectTransform lienzo;

    private GameObject interfazJuego;
    private RectTransform barraVida;
    private Text textoObjetivo;

    private GameObject panelJefe;
    private RectTransform barraJefe;
    private Text nombreJefe;

    private GameObject panelDialogo;
    private Text nombreDialogo;
    private Text textoDialogo;

    private GameObject panelLetrero;
    private Text textoLetrero;

    private GameObject panelPantalla;
    private Text tituloPantalla;
    private Text cuerpoPantalla;
    private Image retratoPantalla;
    private Image retratoPantalla2;

    private Text aviso;
    private Text indicador;
    private Image noche;
    private Image negro;

    private void Awake()
    {
        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject objeto = new GameObject("Interfaz");
        objeto.transform.SetParent(transform, false);

        Canvas canvas = objeto.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler escala = objeto.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 0.5f;

        lienzo = objeto.GetComponent<RectTransform>();

        noche = CrearImagen(lienzo, "Noche", new Color(0.03f, 0.05f, 0.2f, 0.62f));
        Estirar(noche.rectTransform);
        noche.gameObject.SetActive(false);

        CrearInterfazJuego();
        CrearJefe();
        CrearDialogo();
        CrearLetrero();
        CrearPantalla();

        aviso = CrearTexto(lienzo, "Aviso", 40, TextAnchor.MiddleCenter, ColorCrema);
        Colocar(aviso.rectTransform, new Vector2(0.5f, 0.72f), new Vector2(1500f, 80f));
        aviso.gameObject.SetActive(false);

        indicador = CrearTexto(lienzo, "Indicador", 34, TextAnchor.MiddleCenter, Color.white);
        indicador.rectTransform.sizeDelta = new Vector2(420f, 60f);
        indicador.gameObject.AddComponent<Outline>().effectColor = Color.black;
        indicador.gameObject.SetActive(false);

        negro = CrearImagen(lienzo, "Fundido", new Color(0f, 0f, 0f, 0f));
        Estirar(negro.rectTransform);

        // El letrero de "Esa noche..." se lee sobre la pantalla negra.
        panelLetrero.transform.SetAsLastSibling();
    }

    public void Reiniciar()
    {
        StopAllCoroutines();
        interfazJuego.SetActive(false);
        panelJefe.SetActive(false);
        panelDialogo.SetActive(false);
        panelLetrero.SetActive(false);
        panelPantalla.SetActive(false);
        aviso.gameObject.SetActive(false);
        indicador.gameObject.SetActive(false);
        noche.gameObject.SetActive(false);
        textoObjetivo.text = string.Empty;
    }

    public void MostrarInterfazDeJuego(bool mostrar)
    {
        interfazJuego.SetActive(mostrar);
    }

    public void MostrarVida(int vida, int maxima)
    {
        CambiarRelleno(barraVida, maxima > 0 ? (float)vida / maxima : 0f);
    }

    public void Objetivo(string texto)
    {
        textoObjetivo.text = texto;
    }

    public void MostrarJefe(string nombre)
    {
        nombreJefe.text = nombre;
        CambiarRelleno(barraJefe, 1f);
        panelJefe.SetActive(true);
    }

    public void ActualizarJefe(float porcentaje)
    {
        CambiarRelleno(barraJefe, porcentaje);
    }

    public void OcultarJefe()
    {
        panelJefe.SetActive(false);
    }

    public void Noche(bool activa)
    {
        noche.gameObject.SetActive(activa);
    }

    public void Avisar(string texto, float segundos = 2.5f)
    {
        StartCoroutine(MostrarAviso(texto, segundos));
    }

    private IEnumerator MostrarAviso(string texto, float segundos)
    {
        aviso.text = texto;
        aviso.gameObject.SetActive(true);
        yield return new WaitForSeconds(segundos);

        if (aviso.text == texto)
            aviso.gameObject.SetActive(false);
    }

    public void MostrarIndicador(Vector3 posicionMundo, string texto)
    {
        Camera camara = Camera.main;

        if (camara == null)
            return;

        indicador.text = texto;
        indicador.rectTransform.position = camara.WorldToScreenPoint(posicionMundo);
        indicador.gameObject.SetActive(true);
    }

    public void OcultarIndicador()
    {
        indicador.gameObject.SetActive(false);
    }

    /// <summary>Muestra una línea de diálogo y espera a que el jugador continúe.</summary>
    public IEnumerator Dialogo(string nombre, string texto)
    {
        nombreDialogo.text = nombre;
        textoDialogo.text = string.Empty;
        panelDialogo.SetActive(true);

        yield return null;

        float tiempo = 0f;
        bool completo = false;

        while (!completo)
        {
            tiempo += Time.deltaTime;
            int letras = Mathf.Min(Mathf.FloorToInt(tiempo * 55f), texto.Length);
            textoDialogo.text = texto.Substring(0, letras);
            completo = letras >= texto.Length;

            if (PresionoContinuar())
            {
                textoDialogo.text = texto;
                completo = true;
            }

            yield return null;
        }

        while (!PresionoContinuar())
            yield return null;

        panelDialogo.SetActive(false);
        yield return null;
    }

    public IEnumerator Letrero(string texto)
    {
        textoLetrero.text = texto;
        panelLetrero.SetActive(true);

        yield return null;

        while (!PresionoContinuar())
            yield return null;

        panelLetrero.SetActive(false);
        yield return null;
    }

    public void MostrarPantalla(string titulo, string cuerpo, Color fondo, Sprite retrato = null, Sprite retrato2 = null)
    {
        tituloPantalla.text = titulo;
        cuerpoPantalla.text = cuerpo;
        panelPantalla.GetComponent<Image>().color = fondo;
        ColocarRetrato(retratoPantalla, retrato);
        ColocarRetrato(retratoPantalla2, retrato2);
        panelPantalla.SetActive(true);
    }

    private static void ColocarRetrato(Image imagen, Sprite sprite)
    {
        imagen.sprite = sprite;
        imagen.gameObject.SetActive(sprite != null);

        if (sprite != null)
            imagen.SetNativeSize();

        if (sprite != null)
        {
            RectTransform rect = imagen.rectTransform;
            float escala = 460f / Mathf.Max(rect.sizeDelta.y, 1f);
            rect.sizeDelta *= escala;
        }
    }

    public IEnumerator Fundir(float destino, float duracion)
    {
        Color color = negro.color;
        float inicio = color.a;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(inicio, destino, tiempo / duracion);
            negro.color = color;
            yield return null;
        }

        color.a = destino;
        negro.color = color;
    }

    public static bool PresionoContinuar()
    {
        Keyboard teclado = Keyboard.current;
        Mouse raton = Mouse.current;

        bool tecla = teclado != null && (teclado.spaceKey.wasPressedThisFrame ||
                                         teclado.enterKey.wasPressedThisFrame ||
                                         teclado.numpadEnterKey.wasPressedThisFrame ||
                                         teclado.eKey.wasPressedThisFrame);

        return tecla || (raton != null && raton.leftButton.wasPressedThisFrame);
    }

    private void CrearInterfazJuego()
    {
        interfazJuego = new GameObject("InterfazDeJuego", typeof(RectTransform));
        interfazJuego.transform.SetParent(lienzo, false);
        Estirar((RectTransform)interfazJuego.transform);

        Text etiqueta = CrearTexto((RectTransform)interfazJuego.transform, "EtiquetaLeo", 30, TextAnchor.MiddleLeft, ColorCrema);
        etiqueta.text = "LEO";
        etiqueta.fontStyle = FontStyle.Bold;
        EsquinaSuperiorIzquierda(etiqueta.rectTransform, new Vector2(40f, -28f), new Vector2(100f, 40f));

        barraVida = CrearBarra((RectTransform)interfazJuego.transform, new Vector2(120f, -32f), new Vector2(400f, 30f),
            new Color(0.8f, 0.14f, 0.12f, 1f));

        textoObjetivo = CrearTexto((RectTransform)interfazJuego.transform, "Objetivo", 30, TextAnchor.UpperRight, ColorCrema);
        textoObjetivo.gameObject.AddComponent<Outline>().effectColor = Color.black;
        RectTransform rectObjetivo = textoObjetivo.rectTransform;
        rectObjetivo.anchorMin = new Vector2(1f, 1f);
        rectObjetivo.anchorMax = new Vector2(1f, 1f);
        rectObjetivo.pivot = new Vector2(1f, 1f);
        rectObjetivo.anchoredPosition = new Vector2(-40f, -28f);
        rectObjetivo.sizeDelta = new Vector2(900f, 110f);

        interfazJuego.SetActive(false);
    }

    private void CrearJefe()
    {
        panelJefe = new GameObject("Jefe", typeof(RectTransform));
        panelJefe.transform.SetParent(lienzo, false);
        Estirar((RectTransform)panelJefe.transform);

        nombreJefe = CrearTexto((RectTransform)panelJefe.transform, "NombreJefe", 28, TextAnchor.MiddleCenter, ColorCrema);
        nombreJefe.fontStyle = FontStyle.Bold;
        nombreJefe.gameObject.AddComponent<Outline>().effectColor = Color.black;
        Colocar(nombreJefe.rectTransform, new Vector2(0.5f, 0.9f), new Vector2(800f, 44f));

        barraJefe = CrearBarra((RectTransform)panelJefe.transform, new Vector2(560f, -150f), new Vector2(800f, 26f),
            new Color(0.5f, 0.16f, 0.62f, 1f));

        panelJefe.SetActive(false);
    }

    private void CrearDialogo()
    {
        Image panel = CrearImagen(lienzo, "Dialogo", ColorPanel);
        panelDialogo = panel.gameObject;
        RectTransform rect = panel.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 40f);
        rect.sizeDelta = new Vector2(1500f, 240f);

        nombreDialogo = CrearTexto(rect, "Nombre", 36, TextAnchor.MiddleLeft, ColorAcento);
        nombreDialogo.fontStyle = FontStyle.Bold;
        EsquinaSuperiorIzquierda(nombreDialogo.rectTransform, new Vector2(40f, -18f), new Vector2(900f, 50f));

        textoDialogo = CrearTexto(rect, "Texto", 34, TextAnchor.UpperLeft, ColorCrema);
        EsquinaSuperiorIzquierda(textoDialogo.rectTransform, new Vector2(40f, -74f), new Vector2(1420f, 120f));

        Text continuar = CrearTexto(rect, "Continuar", 24, TextAnchor.MiddleRight, ColorCrema);
        continuar.text = "Espacio para continuar";
        EsquinaSuperiorIzquierda(continuar.rectTransform, new Vector2(960f, -196f), new Vector2(500f, 34f));

        panelDialogo.SetActive(false);
    }

    private void CrearLetrero()
    {
        Image panel = CrearImagen(lienzo, "Letrero", ColorPanel);
        panelLetrero = panel.gameObject;
        Colocar(panel.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(1240f, 340f));

        textoLetrero = CrearTexto(panel.rectTransform, "Texto", 42, TextAnchor.MiddleCenter, ColorCrema);
        Estirar(textoLetrero.rectTransform);
        textoLetrero.rectTransform.offsetMin = new Vector2(50f, 70f);
        textoLetrero.rectTransform.offsetMax = new Vector2(-50f, -30f);

        Text continuar = CrearTexto(panel.rectTransform, "Continuar", 24, TextAnchor.MiddleCenter, ColorAcento);
        continuar.text = "Espacio para continuar";
        Colocar(continuar.rectTransform, new Vector2(0.5f, 0.12f), new Vector2(700f, 34f));

        panelLetrero.SetActive(false);
    }

    private void CrearPantalla()
    {
        Image panel = CrearImagen(lienzo, "Pantalla", Color.black);
        panelPantalla = panel.gameObject;
        Estirar(panel.rectTransform);

        retratoPantalla = CrearImagen(panel.rectTransform, "Retrato", Color.white);
        Colocar(retratoPantalla.rectTransform, new Vector2(0.14f, 0.45f), new Vector2(300f, 460f));
        retratoPantalla.preserveAspect = true;

        retratoPantalla2 = CrearImagen(panel.rectTransform, "Retrato2", Color.white);
        Colocar(retratoPantalla2.rectTransform, new Vector2(0.86f, 0.45f), new Vector2(300f, 460f));
        retratoPantalla2.preserveAspect = true;

        tituloPantalla = CrearTexto(panel.rectTransform, "Titulo", 130, TextAnchor.MiddleCenter, ColorAcento);
        tituloPantalla.fontStyle = FontStyle.Bold;
        Colocar(tituloPantalla.rectTransform, new Vector2(0.5f, 0.75f), new Vector2(1300f, 220f));

        cuerpoPantalla = CrearTexto(panel.rectTransform, "Cuerpo", 36, TextAnchor.UpperCenter, ColorCrema);
        Colocar(cuerpoPantalla.rectTransform, new Vector2(0.5f, 0.32f), new Vector2(1000f, 520f));

        panelPantalla.SetActive(false);
    }

    private RectTransform CrearBarra(RectTransform padre, Vector2 posicion, Vector2 tamano, Color color)
    {
        Image fondo = CrearImagen(padre, "FondoBarra", new Color(0f, 0f, 0f, 0.75f));
        EsquinaSuperiorIzquierda(fondo.rectTransform, posicion, tamano);

        Image relleno = CrearImagen(fondo.rectTransform, "Relleno", color);
        RectTransform rect = relleno.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(3f, 3f);
        rect.offsetMax = new Vector2(-3f, -3f);
        return rect;
    }

    private static void CambiarRelleno(RectTransform barra, float porcentaje)
    {
        barra.anchorMax = new Vector2(Mathf.Clamp01(porcentaje), 1f);
    }

    private Image CrearImagen(RectTransform padre, string nombre, Color color)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.transform.SetParent(padre, false);
        Image imagen = objeto.AddComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;
        return imagen;
    }

    private Text CrearTexto(RectTransform padre, string nombre, int tamano, TextAnchor alineacion, Color color)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.transform.SetParent(padre, false);
        Text texto = objeto.AddComponent<Text>();
        texto.font = fuente;
        texto.fontSize = tamano;
        texto.alignment = alineacion;
        texto.color = color;
        texto.raycastTarget = false;
        return texto;
    }

    private static void Estirar(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Colocar(RectTransform rect, Vector2 ancla, Vector2 tamano)
    {
        rect.anchorMin = ancla;
        rect.anchorMax = ancla;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = tamano;
    }

    private static void EsquinaSuperiorIzquierda(RectTransform rect, Vector2 posicion, Vector2 tamano)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
    }
}
