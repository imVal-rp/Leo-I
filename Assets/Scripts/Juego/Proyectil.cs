using UnityEngine;

/// <summary>Ataque a distancia. Leo puede saltarlo o bloquearlo de frente.</summary>
public class Proyectil : MonoBehaviour
{
    private const float RadioImpacto = 0.75f;

    private float direccion = 1f;
    private float velocidad = 6f;
    private int danio = 1;

    public void Lanzar(float nuevaDireccion, float nuevaVelocidad, int nuevoDanio)
    {
        direccion = nuevaDireccion;
        velocidad = nuevaVelocidad;
        danio = nuevoDanio;
    }

    private void Update()
    {
        Juego juego = Juego.I;

        transform.position += Vector3.right * (direccion * velocidad * Time.deltaTime);

        float x = transform.position.x;

        if (x < juego.LimiteIzquierdo - 1f || x > juego.LimiteDerecho + 1f)
        {
            Destroy(gameObject);
            return;
        }

        if (!juego.LeoVivo)
            return;

        float distancia = Vector2.Distance(transform.position, juego.LeoTransform.position);

        if (distancia <= RadioImpacto)
        {
            juego.Leo.RecibirDanio(danio, transform.position);
            Destroy(gameObject);
        }
    }
}
