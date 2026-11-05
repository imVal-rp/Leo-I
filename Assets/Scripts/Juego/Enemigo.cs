using System;
using System.Collections;
using UnityEngine;

/// <summary>Vida, daño y muerte comunes a todos los enemigos.</summary>
public abstract class Enemigo : MonoBehaviour, IDaniable
{
    protected AnimadorSprite animador;
    protected int vida = 2;
    protected int vidaMaxima = 2;

    private Coroutine destello;

    public bool Vivo { get; private set; } = true;

    public float PorcentajeVida => vidaMaxima > 0 ? Mathf.Clamp01((float)vida / vidaMaxima) : 0f;

    public event Action AlCambiarVida;

    protected static Juego J => Juego.I;

    protected float LadoDeLeo =>
        J.LeoTransform != null && J.LeoTransform.position.x >= transform.position.x ? 1f : -1f;

    protected float DistanciaALeo =>
        J.LeoTransform != null ? Mathf.Abs(J.LeoTransform.position.x - transform.position.x) : 999f;

    public void Inicializar(AnimadorSprite animadorSprite)
    {
        animador = animadorSprite;
    }

    protected void FijarVida(int cantidad)
    {
        vida = cantidad;
        vidaMaxima = cantidad;
    }

    protected virtual bool PuedeRecibirDanio()
    {
        return true;
    }

    public virtual void RecibirDanio(int cantidad, Vector2 origen)
    {
        if (!Vivo)
            return;

        if (!PuedeRecibirDanio())
        {
            AlBloquearGolpe();
            return;
        }

        vida = Mathf.Max(vida - cantidad, 0);
        AlCambiarVida?.Invoke();
        Sonido.Reproducir("golpe");

        if (destello != null)
            StopCoroutine(destello);

        destello = StartCoroutine(Destello(new Color(1f, 0.35f, 0.35f, 1f), 0.12f));

        if (vida == 0)
        {
            Vivo = false;

            Collider2D caja = GetComponent<Collider2D>();

            if (caja != null)
                caja.enabled = false;

            StopAllCoroutines();
            animador.Render.color = Color.white;
            StartCoroutine(Morir());
            return;
        }

        AlRecibirGolpe(origen);
    }

    protected virtual void AlBloquearGolpe()
    {
    }

    protected virtual void AlRecibirGolpe(Vector2 origen)
    {
    }

    protected abstract ClipSprite ClipMuerte { get; }

    private IEnumerator Morir()
    {
        animador.Reproducir(ClipMuerte, true);

        while (!animador.Termino)
            yield return null;

        yield return new WaitForSeconds(0.35f);

        float tiempo = 0f;
        Color color = animador.Render.color;

        while (tiempo < 0.5f)
        {
            tiempo += Time.deltaTime;
            color.a = 1f - tiempo / 0.5f;
            animador.Render.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator Destello(Color color, float duracion)
    {
        animador.Render.color = color;
        yield return new WaitForSeconds(duracion);
        animador.Render.color = Color.white;
    }

    protected void MirarALeo()
    {
        animador.Mirar(LadoDeLeo > 0f);
    }

    protected void MoverHorizontal(float desplazamiento)
    {
        Vector3 posicion = transform.position;
        posicion.x = Mathf.Clamp(posicion.x + desplazamiento, J.LimiteIzquierdo + 0.5f, J.LimiteDerecho - 0.5f);
        transform.position = posicion;
    }

    /// <summary>Hace daño si Leo está dentro del alcance y no va por encima del ataque.</summary>
    protected bool IntentarGolpear(float alcance, int danio, float alturaMaxima = 1.3f)
    {
        if (!J.LeoVivo || DistanciaALeo > alcance || J.AlturaDeLeo > alturaMaxima)
            return false;

        J.Leo.RecibirDanio(danio, transform.position);
        return true;
    }

    protected static float Azar(float minimo, float maximo)
    {
        return UnityEngine.Random.Range(minimo, maximo);
    }
}
