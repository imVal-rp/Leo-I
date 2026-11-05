using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamaraSeguir : MonoBehaviour
{
    [SerializeField] private Transform objetivo;

    [Tooltip("BoxCollider2D marcado como Is Trigger que cubre toda el área visible del escenario.")]
    [SerializeField] private BoxCollider2D limites;

    private Camera camara;
    private Bounds area;
    private float posicionZ;
    private bool configurada;

    private void Awake()
    {
        camara = GetComponent<Camera>();
        camara.orthographic = true;
        posicionZ = transform.position.z;
    }

    private void Start()
    {
        if (!configurada && limites != null)
            Configurar(objetivo, limites.bounds);
    }

    /// <summary>Asigna a quién seguir y el área visible del escenario.</summary>
    public void Configurar(Transform nuevoObjetivo, Bounds nuevaArea)
    {
        objetivo = nuevoObjetivo;
        area = nuevaArea;
        configurada = true;

        // Ajusta la cámara para mostrar toda la altura del escenario.
        camara.orthographicSize = area.extents.y;
    }

    private void LateUpdate()
    {
        if (objetivo == null || !configurada)
            return;

        float mitadAncho = camara.orthographicSize * camara.aspect;
        float minimoX = area.min.x + mitadAncho;
        float maximoX = area.max.x - mitadAncho;

        // Si el escenario es más angosto que la pantalla, la cámara queda centrada.
        float x = minimoX <= maximoX
            ? Mathf.Clamp(objetivo.position.x, minimoX, maximoX)
            : area.center.x;

        transform.position = new Vector3(x, area.center.y, posicionZ);
    }
}
