using UnityEngine;

/// <summary>Crea personajes, enemigos y objetos durante el juego.</summary>
public static class Fabrica
{
    private const string HojaMomia = "momia_limpio";
    private const string HojaCalavera = "sprite_sheet_calavera_limpio";
    private const string HojaLlorona = "llorona_sheet_pixel_v3";
    private const string HojaChupacabras = "chupacabras_limpio";
    private const string HojaNahuala = "nahuala_limpio";
    private const string HojaNahualaAtaques = "nahuala_ataques";
    private const string HojaTeodora = "teodora_limpio";

    private static Material materialSprites;

    /// <summary>Usa el mismo material de la escena para que todo reciba la misma luz.</summary>
    public static void PrepararMaterial()
    {
        SpriteRenderer deEscena = Object.FindAnyObjectByType<SpriteRenderer>();
        materialSprites = deEscena != null ? deEscena.sharedMaterial : null;
    }

    public static void AplicarMaterial(SpriteRenderer sprite)
    {
        if (materialSprites != null)
            sprite.sharedMaterial = materialSprites;
    }

    public static Esbirro CrearMomia(Vector2 pies)
    {
        ClipSprite camina = Hojas.Clip(HojaMomia, "6 7 8 5", 6f, true);
        Esbirro enemigo = Crear<Esbirro>("Momia", pies, camina, 2.4f, 1.1f);
        enemigo.Clips(
            Hojas.Clip(HojaMomia, "1 3 4", 4f, false),
            camina,
            Hojas.Clip(HojaMomia, "9 10 11", 6f, false),
            Hojas.Clip(HojaMomia, "13 14 15", 6f, false));
        enemigo.Ajustar(2, 1.5f, 1.3f);
        return enemigo;
    }

    public static Esbirro CrearCalavera(Vector2 pies)
    {
        ClipSprite camina = Hojas.Clip(HojaCalavera, "10 12 11 13", 8f, true);
        Esbirro enemigo = Crear<Esbirro>("Calavera", pies, camina, 2.5f, 1.1f);
        enemigo.Clips(
            Hojas.Clip(HojaCalavera, "0 2 3", 4f, false),
            camina,
            Hojas.Clip(HojaCalavera, "14 15 16", 7f, false),
            Hojas.Clip(HojaCalavera, "17 18 20 22", 6f, false));
        enemigo.Ajustar(2, 2.2f, 1.4f);
        return enemigo;
    }

    public static LloronaEnemiga CrearLlorona(Vector2 pies)
    {
        ClipSprite flota = Hojas.Clip(HojaLlorona, "7 6 4 5", 5f, true);
        LloronaEnemiga enemigo = Crear<LloronaEnemiga>("Llorona", pies, flota, 3f, 1.3f);
        enemigo.Clips(
            Hojas.Clip(HojaLlorona, "0 1", 3f, true),
            flota,
            Hojas.Clip(HojaLlorona, "8 9 10 13", 6f, false),
            Hojas.Clip(HojaLlorona, "2 20 21", 10f, false),
            Hojas.Clip(HojaLlorona, "56 39 22 11 12", 5f, false));
        return enemigo;
    }

    public static Chupacabras CrearChupacabras(Vector2 pies)
    {
        ClipSprite idle = Hojas.Clip(HojaChupacabras, "0 1", 3f, true);
        Chupacabras enemigo = Crear<Chupacabras>("Chupacabras", pies, idle, 3.4f, 1.9f);
        enemigo.Clips(
            idle,
            Hojas.Clip(HojaChupacabras, "2 3 4 5", 7f, true),
            Hojas.Clip(HojaChupacabras, "7 8 10", 10f, true),
            Hojas.Clip(HojaChupacabras, "9", 6f, false),
            Hojas.Clip(HojaChupacabras, "6 10 17 26 28", 6f, false));
        return enemigo;
    }

    public static Nahuala CrearNahuala(Vector2 pies)
    {
        ClipSprite idle = Hojas.Clip(HojaNahuala, "0 1", 2.5f, true);
        Nahuala enemigo = Crear<Nahuala>("Nahuala", pies, idle, 3.4f, 1.5f);
        enemigo.Clips(
            idle,
            Hojas.Clip(HojaNahualaAtaques, "47 50 44 52", 4f, false, 1.3f, true),
            Hojas.Clip(HojaNahualaAtaques, "0 4 2", 5f, false, 1.15f, true),
            Hojas.Clip(HojaNahuala, "15 14", 3f, true),
            Hojas.Clip(HojaNahuala, "3 14 20 21 22 24 27", 5f, false));
        return enemigo;
    }

    /// <summary>Crea a Teodora o a un personaje de escena, sin colisiones.</summary>
    public static AnimadorSprite CrearTeodora(Vector2 pies)
    {
        ClipSprite idle = Hojas.Clip(HojaTeodora, "0 1", 2f, true);
        AnimadorSprite animador = CrearVisual("Teodora", pies, idle, 2.3f, 15);
        animador.Reproducir(idle);
        return animador;
    }

    public static AnimadorSprite CrearNahualaDeEscena(Vector2 pies)
    {
        ClipSprite idle = Hojas.Clip(HojaNahuala, "0 1", 2.5f, true);
        AnimadorSprite animador = CrearVisual("NahualaEscena", pies, idle, 3.4f, 16);
        animador.Reproducir(idle);
        return animador;
    }

    public static SpriteRenderer CrearMachete(Vector2 posicion)
    {
        string[] dibujo =
        {
            "...............##.",
            ".............##OO#",
            "...........##OOO#.",
            ".........##OOOO#..",
            ".......##OOOOO#...",
            ".....##OOOOOO#....",
            "...##OOOOOO##.....",
            "..#MMOOOO##.......",
            ".#BB#MM##.........",
            "#BB#..............",
            ".##...............",
        };

        int alto = dibujo.Length;
        int ancho = dibujo[0].Length;
        Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Point;

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
                textura.SetPixel(x, alto - 1 - y, ColorDe(dibujo[y][x]));
        }

        textura.Apply();

        GameObject objeto = new GameObject("Machete");
        objeto.transform.position = posicion;

        SpriteRenderer sprite = objeto.AddComponent<SpriteRenderer>();
        sprite.sprite = Sprite.Create(textura, new Rect(0f, 0f, ancho, alto), new Vector2(0.5f, 0.5f), 12f);
        sprite.sortingOrder = 15;
        AplicarMaterial(sprite);
        return sprite;
    }

    private static Color ColorDe(char letra)
    {
        switch (letra)
        {
            case '#': return new Color(0.1f, 0.08f, 0.08f, 1f);
            case 'O': return new Color(0.68f, 0.68f, 0.7f, 1f);
            case 'M': return new Color(0.42f, 0.3f, 0.2f, 1f);
            case 'B': return new Color(0.36f, 0.2f, 0.11f, 1f);
            default: return Color.clear;
        }
    }

    public static Proyectil CrearProyectil(Vector2 posicion, float direccion, float velocidad, int danio, Color color)
    {
        const int tamano = 32;
        Texture2D textura = new Texture2D(tamano, tamano, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Point;

        for (int y = 0; y < tamano; y++)
        {
            for (int x = 0; x < tamano; x++)
            {
                float distancia = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                bool relleno = distancia < 15f;
                bool frente = distancia > 11f || distancia < 5f;
                textura.SetPixel(x, y, relleno && frente ? Color.white : Color.clear);
            }
        }

        textura.Apply();

        GameObject objeto = new GameObject("Proyectil");
        objeto.transform.position = posicion;

        SpriteRenderer sprite = objeto.AddComponent<SpriteRenderer>();
        sprite.sprite = Sprite.Create(textura, new Rect(0f, 0f, tamano, tamano), new Vector2(0.5f, 0.5f), 32f);
        sprite.color = color;
        sprite.sortingOrder = 25;
        AplicarMaterial(sprite);

        Proyectil proyectil = objeto.AddComponent<Proyectil>();
        proyectil.Lanzar(direccion, velocidad, danio);
        return proyectil;
    }

    private static T Crear<T>(string nombre, Vector2 pies, ClipSprite referencia, float altura, float ancho) where T : Enemigo
    {
        AnimadorSprite animador = CrearVisual(nombre, pies, referencia, altura, 19);
        GameObject objeto = animador.gameObject;

        int capa = LayerMask.NameToLayer("Enemigo");

        if (capa >= 0)
            objeto.layer = capa;

        BoxCollider2D caja = objeto.AddComponent<BoxCollider2D>();
        caja.isTrigger = true;
        caja.offset = new Vector2(0f, altura / 2f);
        caja.size = new Vector2(ancho, altura);

        T enemigo = objeto.AddComponent<T>();
        enemigo.Inicializar(animador);
        return enemigo;
    }

    private static AnimadorSprite CrearVisual(string nombre, Vector2 pies, ClipSprite referencia, float altura, int orden)
    {
        GameObject raiz = new GameObject(nombre);
        raiz.transform.position = pies;

        GameObject cuerpo = new GameObject("Cuerpo");
        cuerpo.transform.SetParent(raiz.transform, false);

        SpriteRenderer sprite = cuerpo.AddComponent<SpriteRenderer>();
        sprite.sortingOrder = orden;
        AplicarMaterial(sprite);

        AnimadorSprite animador = raiz.AddComponent<AnimadorSprite>();
        Sprite primero = referencia.Cuadros.Length > 0 ? referencia.Cuadros[0] : null;
        animador.Configurar(sprite, primero, altura);
        return animador;
    }
}
