using System.Collections;
using UnityEngine;

/// <summary>Avisa, embiste en línea recta y queda expuesto al terminar.</summary>
public class Chupacabras : Enemigo
{
    private const float VelocidadCaminar = 2f;
    private const float VelocidadEmbestida = 11f;

    private ClipSprite clipIdle;
    private ClipSprite clipCamina;
    private ClipSprite clipEmbestida;
    private ClipSprite clipDanio;
    private ClipSprite clipMuerte;
    private bool expuesto;

    protected override ClipSprite ClipMuerte => clipMuerte;

    public void Clips(ClipSprite idle, ClipSprite camina, ClipSprite embestida, ClipSprite danio, ClipSprite muerte)
    {
        clipIdle = idle;
        clipCamina = camina;
        clipEmbestida = embestida;
        clipDanio = danio;
        clipMuerte = muerte;
        FijarVida(8);
    }

    private void Start()
    {
        StartCoroutine(Comportamiento());
    }

    private IEnumerator Comportamiento()
    {
        while (Vivo)
        {
            if (!J.LeoVivo)
            {
                animador.Reproducir(clipIdle);
                yield return null;
                continue;
            }

            yield return Caminar(Azar(0.8f, 1.5f));

            if (J.LeoVivo)
                yield return Embestir();

            expuesto = true;
            animador.Reproducir(clipIdle);
            yield return new WaitForSeconds(1.6f);
            expuesto = false;
        }
    }

    private IEnumerator Caminar(float duracion)
    {
        float terminaEn = Time.time + duracion;
        animador.Reproducir(clipCamina);

        while (Time.time < terminaEn && J.LeoVivo)
        {
            MirarALeo();
            MoverHorizontal(LadoDeLeo * VelocidadCaminar * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator Embestir()
    {
        MirarALeo();
        animador.Reproducir(clipIdle);
        animador.Render.color = new Color(1f, 0.45f, 0.35f, 1f);
        Sonido.Reproducir("rugido");

        yield return new WaitForSeconds(0.7f);

        animador.Render.color = Color.white;
        animador.Reproducir(clipEmbestida);

        float direccion = LadoDeLeo;
        float destino = J.LeoTransform.position.x + direccion * 3.5f;
        float limite = direccion > 0f ? J.LimiteDerecho - 1.5f : J.LimiteIzquierdo + 1.5f;

        destino = direccion > 0f ? Mathf.Min(destino, limite) : Mathf.Max(destino, limite);

        bool golpeo = false;

        while ((destino - transform.position.x) * direccion > 0f)
        {
            MoverHorizontal(direccion * VelocidadEmbestida * Time.deltaTime);

            if (!golpeo)
                golpeo = IntentarGolpear(1.4f, 2, 2.6f);

            yield return null;
        }
    }

    protected override void AlRecibirGolpe(Vector2 origen)
    {
        if (expuesto)
            StartCoroutine(Quejarse());
    }

    private IEnumerator Quejarse()
    {
        animador.Reproducir(clipDanio, true);
        yield return new WaitForSeconds(0.2f);

        if (Vivo && expuesto)
            animador.Reproducir(clipIdle);
    }
}
