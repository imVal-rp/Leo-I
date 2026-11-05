using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LeoController))]
public class LeoCombate : MonoBehaviour, IDaniable
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 6;
    [SerializeField] private float tiempoInvulnerable = 0.8f;
    [SerializeField] private float velocidadRetroceso = 5f;
    [SerializeField] private float duracionRetroceso = 0.15f;

    [Header("Ataque")]
    [SerializeField] private int danioMachete = 1;
    [SerializeField] private float tiempoEntreAtaques = 0.4f;
    [SerializeField] private float bloqueoAlAtacar = 0.2f;
    [SerializeField] private float alcance = 1.2f;
    [SerializeField] private Vector2 tamanoGolpe = new Vector2(1.6f, 2.2f);
    [SerializeField] private LayerMask capaEnemigos;

    [Header("Pruebas")]
    [Tooltip("Actívalo en el bosque para probar. En el pueblo debe estar apagado.")]
    [SerializeField] private bool empezarConMachete = true;

    [Tooltip("Permite recibir daño con H. Apágalo en la versión final.")]
    [SerializeField] private bool pruebaDanioConH = false;

    // Eventos para la interfaz y el Game Over.
    public event Action<int, int> AlCambiarVida;
    public event Action AlMorir;

    public int VidaActual { get; private set; }
    public int VidaMaxima => vidaMaxima;
    public bool TieneMachete { get; private set; }
    public bool Protegiendo { get; private set; }

    private LeoController leo;
    private Animator animator;
    private SpriteRenderer sprite;
    private float siguienteAtaque;
    private float bloqueoHasta;
    private float invulnerableHasta;

    private void Awake()
    {
        leo = GetComponent<LeoController>();
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        if (capaEnemigos.value == 0)
            capaEnemigos = LayerMask.GetMask("Enemigo");

        VidaActual = vidaMaxima;
        TieneMachete = empezarConMachete;
    }

    private void Start()
    {
        AlCambiarVida?.Invoke(VidaActual, vidaMaxima);
    }

    private void Update()
    {
        Keyboard teclado = Keyboard.current;

        if (leo.Muerto || teclado == null)
            return;

        bool puedeActuar = leo.PuedeMoverse && leo.EnSuelo;

        Protegiendo = puedeActuar && teclado.kKey.isPressed;

        if (puedeActuar && !Protegiendo && teclado.jKey.wasPressedThisFrame)
            Atacar();

        if (pruebaDanioConH && teclado.hKey.wasPressedThisFrame)
            RecibirDanio(1, (Vector2)transform.position - Vector2.right * leo.Direccion);

        leo.BloqueadoPorCombate = Protegiendo || Time.time < bloqueoHasta;

        if (TieneControlador())
        {
            animator.SetBool("Protegiendo", Protegiendo);
            animator.SetBool("TieneMachete", TieneMachete);
        }
    }

    public void DarMachete()
    {
        TieneMachete = true;
    }

    public void EstablecerMachete(bool tieneMachete)
    {
        TieneMachete = tieneMachete;
    }

    public void RecibirDanio(int cantidad, Vector2 origen)
    {
        if (leo.Muerto || Time.time < invulnerableHasta)
            return;

        float lado = origen.x - transform.position.x;

        // Protegerse solo detiene ataques que vienen de frente.
        if (Protegiendo && lado * leo.Direccion > 0f)
        {
            Sonido.Reproducir("bloqueo");
            return;
        }

        VidaActual = Mathf.Max(VidaActual - cantidad, 0);
        invulnerableHasta = Time.time + tiempoInvulnerable;
        AlCambiarVida?.Invoke(VidaActual, vidaMaxima);
        Sonido.Reproducir("dano");

        StopAllCoroutines();
        StartCoroutine(Destello());

        if (VidaActual == 0)
        {
            Morir();
            return;
        }

        float direccionRetroceso = lado >= 0f ? -1f : 1f;
        leo.AplicarRetroceso(direccionRetroceso * velocidadRetroceso, duracionRetroceso);
    }

    private void Atacar()
    {
        if (!TieneMachete || Time.time < siguienteAtaque)
            return;

        siguienteAtaque = Time.time + tiempoEntreAtaques;
        bloqueoHasta = Time.time + bloqueoAlAtacar;
        Sonido.Reproducir("ataque", 0.6f);

        if (TieneControlador())
            animator.SetTrigger("Atacar");

        Collider2D[] colisiones = Physics2D.OverlapBoxAll(PuntoGolpe(), tamanoGolpe, 0f, capaEnemigos);
        HashSet<IDaniable> golpeados = new HashSet<IDaniable>();

        foreach (Collider2D colision in colisiones)
        {
            IDaniable objetivo = colision.GetComponentInParent<IDaniable>();

            if (objetivo != null && golpeados.Add(objetivo))
                objetivo.RecibirDanio(danioMachete, transform.position);
        }
    }

    private void Morir()
    {
        leo.Muerto = true;
        leo.BloqueadoPorCombate = true;
        Protegiendo = false;

        if (TieneControlador())
        {
            animator.SetBool("Protegiendo", false);
            animator.SetTrigger("Muerto");
        }

        AlMorir?.Invoke();
    }

    private IEnumerator Destello()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = Color.white;
    }

    private Vector2 PuntoGolpe()
    {
        int direccion = leo != null ? leo.Direccion : 1;
        return (Vector2)transform.position + Vector2.right * direccion * alcance;
    }

    private bool TieneControlador()
    {
        return animator != null && animator.runtimeAnimatorController != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(PuntoGolpe(), tamanoGolpe);
    }
}
