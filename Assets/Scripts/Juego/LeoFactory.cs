using UnityEngine;

/// <summary>Crea a Leo en cualquier escena con la misma configuración que tenía en el bosque.</summary>
public static class LeoFactory
{
    // Distancia entre el centro de Leo y la planta de sus pies.
    public const float DistanciaPies = 1.33f;

    public static LeoCombate Crear(Vector2 piesEn)
    {
        GameObject leo = new GameObject("Leo");
        leo.transform.position = new Vector3(piesEn.x, piesEn.y + DistanciaPies + 0.02f, 0f);

        SpriteRenderer sprite = leo.AddComponent<SpriteRenderer>();
        sprite.sortingOrder = 20;
        Fabrica.AplicarMaterial(sprite);

        Animator animator = leo.AddComponent<Animator>();
        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Leo");

        Rigidbody2D cuerpo = leo.AddComponent<Rigidbody2D>();
        cuerpo.gravityScale = 4f;
        cuerpo.interpolation = RigidbodyInterpolation2D.Interpolate;
        cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        cuerpo.freezeRotation = true;

        BoxCollider2D caja = leo.AddComponent<BoxCollider2D>();
        caja.offset = new Vector2(0.09f, -1.186f);
        caja.size = new Vector2(0.96f, 0.284f);

        // Sin fricción para que Leo no se quede pegado a las paredes al saltar.
        PhysicsMaterial2D sinFriccion = new PhysicsMaterial2D("LeoSinFriccion");
        sinFriccion.friction = 0f;
        sinFriccion.bounciness = 0f;
        caja.sharedMaterial = sinFriccion;

        leo.AddComponent<LeoController>();
        return leo.AddComponent<LeoCombate>();
    }
}
