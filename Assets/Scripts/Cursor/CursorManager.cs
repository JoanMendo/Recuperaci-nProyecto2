using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;
    public Vector3 currentObjectPosition;
    public Vector2 cursorPosition;
    public bool canDropIngrdient = true;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject ingredientPrefab;
    [SerializeField] private LayerMask groundLayerMask;  // Para el segundo raycast
    [SerializeField] private float spawnHeight = 20f;    // Altura Y fija para instanciar, que será el tope de la caja

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void FixedUpdate()
    {
        MoveCursor(cursorPosition);
    }

    public void MoveCursor(Vector2 cursorPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(cursorPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 150f, groundLayerMask))
        {
            canDropIngrdient = LayerMask.LayerToName(hit.collider.gameObject.layer) == "Interactuable";

            currentObjectPosition = hit.point;
            currentObjectPosition.y = spawnHeight;

            if (GameManager.instance.currentIngredient != null)
            {
                Rigidbody rb = GameManager.instance.currentIngredient.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.MovePosition(currentObjectPosition);
                }
                else
                {
                    Debug.LogWarning("El ingrediente actual no tiene Rigidbody.");
                }
            }
        }
    }
}
