using System.Collections.Generic;
using UnityEngine;

/// <summary>Conjunto de cuadros de una animación.</summary>
public class ClipSprite
{
    public Sprite[] Cuadros;
    public float Fps;
    public bool Repetir;
    public float Escala = 1f;
    public bool AnclarCuerpo;

    public float Duracion => Fps > 0f && Cuadros != null ? Cuadros.Length / Fps : 0f;
}

/// <summary>
/// Carga las hojas de Assets/Resources/Sprites y arma clips con los mismos
/// cuadros que usan sus animaciones.
/// </summary>
public static class Hojas
{
    private static readonly Dictionary<string, Dictionary<string, Sprite>> Cargadas =
        new Dictionary<string, Dictionary<string, Sprite>>();

    public static ClipSprite Clip(string hoja, string cuadros, float fps, bool repetir, float escala = 1f, bool anclarCuerpo = false)
    {
        List<Sprite> lista = new List<Sprite>();

        foreach (string numero in cuadros.Split(' '))
        {
            Sprite sprite = Cuadro(hoja, numero);

            if (sprite != null)
                lista.Add(sprite);
        }

        return new ClipSprite
        {
            Cuadros = lista.ToArray(),
            Fps = fps,
            Repetir = repetir,
            Escala = escala,
            AnclarCuerpo = anclarCuerpo
        };
    }

    public static Sprite Cuadro(string hoja, string numero)
    {
        if (!Cargadas.TryGetValue(hoja, out Dictionary<string, Sprite> sprites))
        {
            sprites = new Dictionary<string, Sprite>();

            foreach (Sprite sprite in Resources.LoadAll<Sprite>("Sprites/" + hoja))
                sprites[sprite.name] = sprite;

            Cargadas[hoja] = sprites;
        }

        if (sprites.TryGetValue(hoja + "_" + numero, out Sprite encontrado))
            return encontrado;

        Debug.LogWarning($"No se encontró el cuadro {hoja}_{numero}.");
        return null;
    }
}

/// <summary>
/// Reproduce clips en un SpriteRenderer hijo y mantiene los pies en el suelo
/// aunque cada cuadro tenga un tamaño diferente.
/// </summary>
public class AnimadorSprite : MonoBehaviour
{
    private SpriteRenderer sprite;
    private Transform cuerpo;
    private ClipSprite clip;
    private float tiempo;
    private float escalaBase = 1f;
    private float anchoBase = 1f;
    private bool mirandoDerecha = true;

    public SpriteRenderer Render => sprite;
    public bool Termino => clip == null || (!clip.Repetir && tiempo >= clip.Duracion);

    public void Configurar(SpriteRenderer renderer, Sprite referencia, float altura)
    {
        sprite = renderer;
        cuerpo = renderer.transform;

        if (referencia != null)
        {
            escalaBase = altura / Mathf.Max(referencia.bounds.size.y, 0.01f);
            anchoBase = referencia.bounds.size.x * escalaBase;
        }
    }

    public void Reproducir(ClipSprite nuevo, bool reiniciar = false)
    {
        if (clip == nuevo && !reiniciar)
            return;

        clip = nuevo;
        tiempo = 0f;
        Aplicar();
    }

    public void Mirar(bool derecha)
    {
        mirandoDerecha = derecha;
        sprite.flipX = !derecha;
    }

    private void Update()
    {
        if (clip == null)
            return;

        tiempo += Time.deltaTime;
        Aplicar();
    }

    private void Aplicar()
    {
        if (clip == null || clip.Cuadros.Length == 0)
            return;

        int indice = Mathf.FloorToInt(tiempo * clip.Fps);

        indice = clip.Repetir
            ? indice % clip.Cuadros.Length
            : Mathf.Min(indice, clip.Cuadros.Length - 1);

        Sprite actual = clip.Cuadros[indice];
        float escala = escalaBase * clip.Escala;

        sprite.sprite = actual;
        cuerpo.localScale = new Vector3(escala, escala, 1f);

        Bounds limites = actual.bounds;
        float x = 0f;

        // En los ataques el personaje está a un lado del efecto: así no parece brincar.
        if (clip.AnclarCuerpo)
        {
            float sobrante = limites.extents.x * escala - anchoBase / 2f;
            x = Mathf.Max(sobrante, 0f) * (mirandoDerecha ? 1f : -1f);
        }

        cuerpo.localPosition = new Vector3(x, -limites.min.y * escala, 0f);
    }
}
