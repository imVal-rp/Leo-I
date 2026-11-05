using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reproduce los audios de Assets/Resources/Audio.
/// Para cambiar un sonido, reemplaza el archivo y conserva su nombre.
/// </summary>
public class Sonido : MonoBehaviour
{
    private static Sonido instancia;

    private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    private AudioSource efectos;
    private AudioSource musica;

    private void Awake()
    {
        instancia = this;
        efectos = gameObject.AddComponent<AudioSource>();
        musica = gameObject.AddComponent<AudioSource>();
        musica.loop = true;
        musica.volume = 0.35f;
    }

    public static void Reproducir(string nombre, float volumen = 0.8f)
    {
        if (instancia == null)
            return;

        AudioClip clip = instancia.Buscar(nombre);

        if (clip != null)
            instancia.efectos.PlayOneShot(clip, volumen);
    }

    public static void MusicaDeEscena(string escena)
    {
        switch (escena)
        {
            case "Menu":
            case "pueblo":
            case "Victoria":
                Musica("musica_pueblo");
                break;
            case "GameOver":
                Musica(null);
                break;
            default:
                Musica("musica_tension");
                break;
        }
    }

    private static void Musica(string nombre)
    {
        if (instancia == null)
            return;

        AudioClip clip = nombre != null ? instancia.Buscar(nombre) : null;
        AudioSource fuente = instancia.musica;

        if (clip == null)
        {
            fuente.Stop();
            fuente.clip = null;
            return;
        }

        if (fuente.clip == clip && fuente.isPlaying)
            return;

        fuente.clip = clip;
        fuente.Play();
    }

    private AudioClip Buscar(string nombre)
    {
        if (!clips.TryGetValue(nombre, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>("Audio/" + nombre);
            clips[nombre] = clip;
        }

        return clip;
    }
}
