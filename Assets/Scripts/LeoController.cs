using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class LeoController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 4f;
    [SerializeField] private float fuerzaSalto = 11f;

    [Tooltip("Actívalo si el dibujo original de Leo mira hacia la derecha.")]
    [SerializeField] private bool spriteMiraDerecha = true;

    [Header("Referencias")]
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator animator;

    // Tu compañera podrá apagarlo durante los diálogos.
    public bool PuedeMoverse { get; set; } = true;

    // LeoCombate lo usa mientras Leo ataca o se protege.
    public bool BloqueadoPorCombate { get; set; }

    public bool Muerto { get; set; }

    // 1 = derecha, -1 = izquierda.
    public int Direccion { get; private set; } = 1;

    public bool EnSuelo { get; private set; }

    private Rigidbody2D rb;
    private float entradaX;
    private bool saltoPendiente;
    private bool tocandoSuelo;
    private float retrocesoX;
    private float retrocesoHasta;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Mantiene activa la detección del suelo cuando Leo está quieto.
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        if (sprite == null)
            sprite = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        LeerEntrada();

        if (entradaX > 0f)
            Direccion = 1;
        else if (entradaX < 0f)
            Direccion = -1;

        bool mirarDerecha = Direccion == 1;
        sprite.flipX = spriteMiraDerecha ? !mirarDerecha : mirarDerecha;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool("Moviendo", entradaX != 0f);
            animator.SetBool("EnSuelo", EnSuelo || Muerto);
        }
    }

    private void FixedUpdate()
    {
        EnSuelo = tocandoSuelo;
        tocandoSuelo = false;

        float velocidadX = entradaX * velocidad;
        float velocidadY = rb.linearVelocity.y;

        if (Time.time < retrocesoHasta)
            velocidadX = retrocesoX;

        if (saltoPendiente && EnSuelo)
        {
            velocidadY = fuerzaSalto;
            EnSuelo = false;
        }

        saltoPendiente = false;
        rb.linearVelocity = new Vector2(velocidadX, velocidadY);
    }

    public void AplicarRetroceso(float velocidadRetroceso, float duracion)
    {
        retrocesoX = velocidadRetroceso;
        retrocesoHasta = Time.time + duracion;
    }

    private void LeerEntrada()
    {
        entradaX = 0f;
        Keyboard teclado = Keyboard.current;

        if (!PuedeMoverse || BloqueadoPorCombate || Muerto || teclado == null)
            return;

        if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) entradaX -= 1f;
        if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) entradaX += 1f;

        if (teclado.wKey.wasPressedThisFrame || teclado.upArrowKey.wasPressedThisFrame)
            saltoPendiente = true;
    }

    private void OnCollisionStay2D(Collision2D colision)
    {
        foreach (ContactPoint2D contacto in colision.contacts)
        {
            if (contacto.normal.y > 0.5f)
            {
                tocandoSuelo = true;
                return;
            }
        }
    }
}