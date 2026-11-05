using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Jefe final. Invoca criaturas y es inmune mientras sigan vivas.
/// Al derrotarlas pierde poder y queda aturdida.
/// </summary>
public class Nahuala : Enemigo
{
    private const int GolpesPorAturdimiento = 4;

    private ClipSprite clipIdle;
    private ClipSprite clipInvocar;
    private ClipSprite clipAtaque;
    private ClipSprite clipAturdida;
    private ClipSprite clipMuerte;

    private readonly List<Enemigo> criaturas = new List<Enemigo>();

    private bool vulnerable;
    private int golpesRecibidos;
    private bool avisoMostrado;

    protected override ClipSprite ClipMuerte => clipMuerte;

    public void Clips(ClipSprite idle, ClipSprite invocar, ClipSprite ataque, ClipSprite aturdida, ClipSprite muerte)
    {
        clipIdle = idle;
        clipInvocar = invocar;
        clipAtaque = ataque;
        clipAturdida = aturdida;
        clipMuerte = muerte;
        FijarVida(8);
        animador.Reproducir(clipIdle);
    }

    public void Empezar()
    {
        StartCoroutine(Comportamiento());
    }

    protected override bool PuedeRecibirDanio()
    {
        return vulnerable;
    }

    protected override void AlBloquearGolpe()
    {
        Sonido.Reproducir("bloqueo");

        if (!avisoMostrado)
        {
            avisoMostrado = true;
            J.Hud.Avisar("Su poder la protege. ¡Derrota a sus criaturas!");
        }
    }

    protected override void AlRecibirGolpe(Vector2 origen)
    {
        golpesRecibidos++;
    }

    public void EliminarCriaturas()
    {
        foreach (Enemigo criatura in criaturas)
        {
            if (criatura != null)
                Destroy(criatura.gameObject);
        }

        criaturas.Clear();
    }

    private IEnumerator Comportamiento()
    {
        while (Vivo)
        {
            yield return Invocar();

            while (Vivo && QuedanCriaturas() && J.LeoVivo)
            {
                yield return Esperar(2.4f);

                if (QuedanCriaturas() && J.LeoVivo)
                    yield return Disparar();
            }

            if (!J.LeoVivo)
                yield break;

            yield return Aturdida();
            yield return Esperar(0.8f);
        }
    }

    private bool QuedanCriaturas()
    {
        criaturas.RemoveAll(criatura => criatura == null || !criatura.Vivo);
        return criaturas.Count > 0;
    }

    private IEnumerator Esperar(float segundos)
    {
        animador.Reproducir(clipIdle);
        float terminaEn = Time.time + segundos;

        while (Time.time < terminaEn)
        {
            MirarALeo();
            yield return null;
        }
    }

    private IEnumerator Invocar()
    {
        MirarALeo();
        animador.Reproducir(clipInvocar, true);
        Sonido.Reproducir("invocar");

        yield return new WaitForSeconds(clipInvocar.Duracion);

        int cantidad = PorcentajeVida <= 0.5f ? 3 : 2;

        for (int i = 0; i < cantidad; i++)
            criaturas.Add(CrearCriatura(i));

        animador.Reproducir(clipIdle);
    }

    private Enemigo CrearCriatura(int indice)
    {
        float lado = indice % 2 == 0 ? 1f : -1f;
        float x = J.LeoTransform.position.x + lado * (3.5f + indice * 1.2f);
        x = Mathf.Clamp(x, J.LimiteIzquierdo + 1f, J.LimiteDerecho - 1f);

        Vector2 pies = new Vector2(x, J.Suelo);

        return indice % 2 == 0
            ? (Enemigo)Fabrica.CrearMomia(pies)
            : Fabrica.CrearCalavera(pies);
    }

    private IEnumerator Disparar()
    {
        MirarALeo();
        animador.Reproducir(clipAtaque, true);

        yield return new WaitForSeconds(clipAtaque.Duracion * 0.7f);

        float direccion = LadoDeLeo;
        Vector2 salida = new Vector2(transform.position.x + direccion * 1.6f, J.Suelo + 1.2f);
        Fabrica.CrearProyectil(salida, direccion, 6.5f, 1, new Color(0.65f, 0.25f, 0.9f, 0.95f));
        Sonido.Reproducir("sombra");

        yield return new WaitForSeconds(clipAtaque.Duracion * 0.3f);
    }

    private IEnumerator Aturdida()
    {
        vulnerable = true;
        golpesRecibidos = 0;
        animador.Reproducir(clipAturdida);
        J.Hud.Avisar("¡Perdió poder! ¡Golpéala ahora!");

        float terminaEn = Time.time + 5f;

        while (Vivo && Time.time < terminaEn && golpesRecibidos < GolpesPorAturdimiento)
            yield return null;

        vulnerable = false;
    }
}
