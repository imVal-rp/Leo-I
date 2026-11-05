using System.Collections;
using UnityEngine;

/// <summary>Momias y calaveras: aparecen, persiguen a Leo y atacan de cerca.</summary>
public class Esbirro : Enemigo
{
    private ClipSprite clipAparece;
    private ClipSprite clipCamina;
    private ClipSprite clipAtaque;
    private ClipSprite clipMuerte;

    private float velocidad = 1.5f;
    private float alcance = 1.3f;
    private float distanciaDeseada = 1.1f;
    private float aturdidoHasta;

    protected override ClipSprite ClipMuerte => clipMuerte;

    public void Clips(ClipSprite aparece, ClipSprite camina, ClipSprite ataque, ClipSprite muerte)
    {
        clipAparece = aparece;
        clipCamina = camina;
        clipAtaque = ataque;
        clipMuerte = muerte;
    }

    public void Ajustar(int vidaInicial, float velocidadInicial, float alcanceInicial)
    {
        FijarVida(vidaInicial);
        velocidad = velocidadInicial * Azar(0.9f, 1.15f);
        alcance = alcanceInicial;
        distanciaDeseada = alcanceInicial * Azar(0.7f, 0.95f);
    }

    private void Start()
    {
        StartCoroutine(Comportamiento());
    }

    protected override void AlRecibirGolpe(Vector2 origen)
    {
        aturdidoHasta = Time.time + 0.3f;
        MoverHorizontal(transform.position.x >= origen.x ? 0.5f : -0.5f);
    }

    private IEnumerator Comportamiento()
    {
        Sonido.Reproducir("aparece", 0.5f);
        MirarALeo();
        animador.Reproducir(clipAparece, true);

        while (!animador.Termino)
            yield return null;

        while (Vivo)
        {
            if (!J.LeoVivo)
            {
                animador.Reproducir(clipCamina);
                yield return null;
                continue;
            }

            if (Time.time < aturdidoHasta)
            {
                yield return null;
                continue;
            }

            MirarALeo();

            if (DistanciaALeo <= distanciaDeseada)
            {
                yield return Atacar();
                continue;
            }

            animador.Reproducir(clipCamina);
            MoverHorizontal(LadoDeLeo * velocidad * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator Atacar()
    {
        animador.Reproducir(clipAtaque, true);

        yield return new WaitForSeconds(clipAtaque.Duracion * 0.55f);
        IntentarGolpear(alcance + 0.25f, 1);

        yield return new WaitForSeconds(clipAtaque.Duracion * 0.45f + 0.5f);
    }
}
