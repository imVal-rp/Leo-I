using UnityEngine;

// Cualquier personaje que pueda recibir daño implementa esta interfaz.
public interface IDaniable
{
    void RecibirDanio(int cantidad, Vector2 origen);
}
