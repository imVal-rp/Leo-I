using System.Collections;
using UnityEngine;

/// <summary>Flota a distancia y lanza su grito. Leo debe bloquearlo o saltarlo.</summary>
public class LloronaEnemiga : Enemigo
{
    private const float DistanciaDeseada = 4.5f;
    private const float VelocidadFlotar = 1.7f;

    private ClipSprite clipIdle;
    private ClipSprite clipFlota;
    private ClipSprite clipGrito;
    private ClipSprite clipDanio;
    private ClipSprite clipMuerte;

    private float recibioGolpe;

    protected override ClipSprite ClipMuerte => clipMuerte;

    public void Clips(ClipSprite idle, ClipSprite flota, ClipSprite grito, ClipSprite danio, ClipSprite muerte)
    {
        clipIdle = idle;
        clipFlota = flota;
        clipGrito = grito;
        clipDanio = danio;
        clipMuerte = muerte;
        FijarVida(6);
    }

    private void Start()
    {
        StartCoroutine(Comportamiento());
    }

    private void LateUpdate()
    {
        if (!Vivo)
            return;

        Vector3 posicion = transform.position;
        posicion.y = J.Suelo + 0.35f + Mathf.Sin(Time.time * 2.2f) * 0.18f;
        transform.position = posicion;
    }

    protected override void AlRecibirGolpe(Vector2 origen)
    {
        recibioGolpe = Time.time;
        animador.Reproducir(clipDanio, true);
    }

    private IEnumerator Comportamiento()
    {
        while (Vivo)
        {
            float terminaEn = Time.time + Azar(2f, 3.2f);

            while (Time.time < terminaEn)
            {
                if (J.LeoVivo)
                {
                    MirarALeo();

                    if (Time.time - recibioGolpe > 0.3f)
                        animador.Reproducir(DistanciaALeo > DistanciaDeseada + 0.4f ? clipFlota : clipIdle);

                    // Mantiene su distancia: si Leo se acerca, retrocede más lento de lo que él camina.
                    if (DistanciaALeo > DistanciaDeseada + 0.4f)
                        MoverHorizontal(LadoDeLeo * VelocidadFlotar * Time.deltaTime);
                    else if (DistanciaALeo < DistanciaDeseada - 0.8f)
                        MoverHorizontal(-LadoDeLeo * VelocidadFlotar * 0.55f * Time.deltaTime);
                }

                yield return null;
            }

            if (!J.LeoVivo)
                continue;

            yield return Gritar();
            yield return new WaitForSeconds(1.2f);
        }
    }

    private IEnumerator Gritar()
    {
        MirarALeo();

        // Aviso antes del ataque.
        animador.Reproducir(clipIdle);
        animador.Render.color = new Color(0.6f, 0.75f, 1f, 1f);
        yield return new WaitForSeconds(0.6f);
        animador.Render.color = Color.white;

        float direccion = LadoDeLeo;
        animador.Reproducir(clipGrito, true);
        Sonido.Reproducir("grito");

        Vector2 salida = new Vector2(transform.position.x + direccion * 1.2f, J.Suelo + 1.3f);
        Fabrica.CrearProyectil(salida, direccion, 7f, 1, new Color(0.7f, 0.9f, 1f, 0.9f));

        yield return new WaitForSeconds(clipGrito.Duracion);
    }
}
