using NUnit.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System;

[System.Serializable]
public class TipoCantidad
{
    public IngredientType tipo;
    public int cantidad;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<GameObject> allIngredientsList = new List<GameObject>();
    public List<GameObject> allDisplayIngredientsList = new List<GameObject>();

    // Este prefab servirá como “contenedor” de la lista de ingredientes restantes.
    // No modificaremos directamente su escala en el Asset, sino sobre instancias.
    public GameObject remainingIngredientsPrefab;

    public GameObject currentIngredient;
    public Coroutine ingredientCoroutine;

    [SerializeField] private GameObject boxArea;

    [SerializeField] private float minIngredientValue = 0;
    [SerializeField] private float maxIngredientValue = 3;
    [SerializeField] private List<TipoCantidad> winCondition = new List<TipoCantidad>();
    [SerializeField] private List<TipoCantidad> ingredientesEnCaja = new List<TipoCantidad>();
    [SerializeField] private List<TipoCantidad> remainingIngredientsList = new List<TipoCantidad>();

    // Lista para mantener referencia a los ingredientes mostrados y sus textos
    private List<GameObject> displayedIngredients = new List<GameObject>();

    // ---- Novedades para mostrar texto 3D de cantidad faltante ----
    // Para almacenar la escala original del prefab (base para cálculos):
    private Vector3 originalRemainingScale;
    // Para referenciar la instancia actual del contenedor de ingredientes restantes:
    private GameObject remainingIngredientsContainerInstance;

    [Header("Configuración del texto 3D para cantidad faltante")]
    [Tooltip("Offset en X y Z desde el centro del contenedor de ingredientes restantes hacia donde se coloca el texto 3D.")]
    // Usaremos un Vector2 (X,Z) para posicionar los textos siempre en la misma columna.
    [SerializeField] private Vector2 textOffsetFromContainer = new Vector2(2f, 2f);
    [Tooltip("Tamaño de fuente para el TextMesh")]
    [SerializeField] private int countTextFontSize = 32;
    [Tooltip("Color del texto 3D")]
    [SerializeField] private Color countTextColor = Color.black;
    [Tooltip("Material opcional para el TextMesh. Si no se asigna, se usará el material por defecto.")]
    [SerializeField] private Material countTextMaterial = null;
    // ---------------------------------------------------------------

    // Rotación fija de todos los textos: (0,45,0)
    private readonly Vector3 textFixedEuler = new Vector3(0f, 45f, 0f);

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Guardamos la escala original del prefab para usarla como base.
            if (remainingIngredientsPrefab != null)
            {
                originalRemainingScale = remainingIngredientsPrefab.transform.localScale;
            }
            else
            {
                Debug.LogWarning("remainingIngredientsPrefab no está asignado en GameManager.");
                originalRemainingScale = Vector3.one;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        if (currentIngredient == null)
        {
            ingredientCoroutine = StartCoroutine(SetCurrentIngredient());
            CheckWinConditions();
        }
    }

    public void DropIngredient()
    {
        if (ingredientCoroutine == null && CursorManager.instance.canDropIngrdient)
        {
            currentIngredient.GetComponent<Rigidbody>().useGravity = true;
            currentIngredient.GetComponent<Ingredient>().enabled = true;
            currentIngredient = null;
            ingredientCoroutine = StartCoroutine(SetCurrentIngredient());
            CheckWinConditions();
        }
    }

    private GameObject SelectRandomIngredient()
    {
        if (allIngredientsList.Count > 0)
        {
            List<GameObject> filteredIngredients = allIngredientsList.Where(ingredient =>
                (int)ingredient.GetComponent<Ingredient>().ingredientType >= minIngredientValue &&
                (int)ingredient.GetComponent<Ingredient>().ingredientType <= maxIngredientValue).ToList();

            if (filteredIngredients.Count == 0)
            {
                Debug.LogWarning("No hay ingredientes en el rango especificado.");
                return null;
            }
            int randomIndex = UnityEngine.Random.Range(0, filteredIngredients.Count);
            return filteredIngredients[randomIndex];
        }
        return null;
    }

    public IEnumerator SetCurrentIngredient()
    {
        yield return new WaitForSeconds(2f);
        GameObject ingredientPrefab = SelectRandomIngredient();
        if (ingredientPrefab != null)
        {
            currentIngredient = Instantiate(ingredientPrefab, CursorManager.instance.currentObjectPosition, Quaternion.identity);
            currentIngredient.GetComponent<Rigidbody>().useGravity = false;
            currentIngredient.GetComponent<Ingredient>().SetModificators();
        }
        ingredientCoroutine = null;
    }

    public void CheckWinConditions()
    {
        // Limpiar modelos y textos previos de ingredientes mostrados
        ClearDisplayedIngredients();

        // Contar ingredientes en caja
        ingredientesEnCaja.Clear();
        foreach (IngredientType tipo in Enum.GetValues(typeof(IngredientType)))
        {
            ingredientesEnCaja.Add(new TipoCantidad { tipo = tipo, cantidad = 0 });
        }

        BoxCollider colider = boxArea.GetComponentInChildren<BoxCollider>();
        if (colider != null)
        {
            Vector3 worldCenter = colider.transform.TransformPoint(colider.center);
            Vector3 halfExtents = Vector3.Scale(colider.size, colider.transform.lossyScale) / 2f;
            Collider[] colliders = Physics.OverlapBox(worldCenter, halfExtents, colider.transform.rotation);

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.TryGetComponent<Ingredient>(out Ingredient ingredient))
                {
                    TipoCantidad tipoCantidad = ingredientesEnCaja.FirstOrDefault(x => x.tipo == ingredient.ingredientType);
                    if (tipoCantidad != null)
                        tipoCantidad.cantidad++;
                }
            }

            // Calcular lista de ingredientes que faltan según winCondition
            remainingIngredientsList.Clear();
            foreach (TipoCantidad winIngredient in winCondition)
            {
                TipoCantidad enCaja = ingredientesEnCaja.FirstOrDefault(x => x.tipo == winIngredient.tipo);
                int faltan = winIngredient.cantidad - (enCaja != null ? enCaja.cantidad : 0);

                if (faltan > 0)
                {
                    remainingIngredientsList.Add(new TipoCantidad { tipo = winIngredient.tipo, cantidad = faltan });
                }
            }

            int totalIngredientesFaltantes = remainingIngredientsList.Count;

            // Destruir la instancia previa del contenedor de ingredientes restantes, si existe
            if (remainingIngredientsContainerInstance != null)
            {
                Destroy(remainingIngredientsContainerInstance);
                remainingIngredientsContainerInstance = null;
            }

            if (totalIngredientesFaltantes > 0)
            {
                // Instanciar un nuevo contenedor a partir del prefab
                remainingIngredientsContainerInstance = Instantiate(remainingIngredientsPrefab);

                // Ajustar la escala de la instancia según la cantidad de ingredientes faltantes,
                // usando la escala original como base, para evitar acumulaciones de escala.
                float newScaleY = originalRemainingScale.y * (totalIngredientesFaltantes / 4f);
                remainingIngredientsContainerInstance.transform.localScale = new Vector3(
                    originalRemainingScale.x,
                    newScaleY,
                    originalRemainingScale.z
                );

                // Obtener el collider de la instancia recién creada para calcular bounds
                Collider listCollider = remainingIngredientsContainerInstance.GetComponent<Collider>();
                if (listCollider != null && remainingIngredientsList.Count > 0)
                {
                    // Reobtén bounds después de ajustar la escala, para usar el tamaño actualizado.
                    Bounds bounds = listCollider.bounds;
                    float totalHeight = bounds.size.y;
                    float spacing = totalHeight / (remainingIngredientsList.Count + 1);

                    // Pre-calcular la X y Z fijas para los textos, usando el centro del bounds + offset:
                    float textX = bounds.center.x + textOffsetFromContainer.x;
                    float textZ = bounds.center.z + textOffsetFromContainer.y; // usar Vector2 y por convención Y como Z offset

                    float currentY = bounds.min.y + spacing;

                    foreach (TipoCantidad tipoCantidad in remainingIngredientsList)
                    {
                        // Buscar prefab de display para este tipo
                        GameObject ingredientPrefab = allDisplayIngredientsList.FirstOrDefault(x =>
                            x.GetComponent<DisplayIngredient>().ingredientType == tipoCantidad.tipo);

                        if (ingredientPrefab != null)
                        {
                            // Instanciar el prefab de display
                            GameObject ingredientInstance = Instantiate(ingredientPrefab);

                            // Posicionar el ingrediente dentro del collider con ajuste en X y Z:
                            Vector3 ingredientPos = new Vector3(
                                bounds.center.x - 2.5f,
                                currentY,
                                bounds.center.z - 2.5f
                            );
                            ingredientInstance.transform.position = ingredientPos;

                            // Guardar referencia para luego destruirlo en la siguiente limpieza
                            displayedIngredients.Add(ingredientInstance);

                            // --------- Instanciar texto 3D con la cantidad faltante, alineado en X/Z comunes ----------
                            int faltan = tipoCantidad.cantidad;
                            if (faltan > 1)
                            {
                                int adicionales = faltan; // según lo que desees mostrar
                                GameObject textGO = new GameObject("RemainingCountText");

                                // Posición world: aquí usamos las coordenadas fijas textX, currentY, textZ
                                Vector3 textPosition = new Vector3(textX, currentY, textZ);
                                textGO.transform.position = textPosition + new Vector3(5, -4, -5);

                                // Rotación fija de (0, 45, 0)
                                textGO.transform.rotation = Quaternion.Euler(textFixedEuler);

                                // Añadir componente TextMesh
                                TextMesh textMesh = textGO.AddComponent<TextMesh>();
                                textMesh.text = "x" + adicionales;
                                textMesh.fontSize = countTextFontSize;
                                textMesh.color = countTextColor;
                                textMesh.fontStyle = FontStyle.Bold;
                                textMesh.alignment = TextAlignment.Center;
                                textMesh.anchor = TextAnchor.MiddleCenter;
                                // Material si se asignó
                                if (countTextMaterial != null)
                                {
                                    MeshRenderer mr = textGO.GetComponent<MeshRenderer>();
                                    if (mr != null)
                                    {
                                        mr.material = countTextMaterial;
                                    }
                                }

                                // Guardar referencia para luego destruirlo
                                displayedIngredients.Add(textGO);
                            }
                            // ------------------------------------------------------------------------------

                            // Aumentar Y para el siguiente ingrediente
                            currentY += spacing;
                        }
                        else
                        {
                            Debug.LogWarning($"No se encontró prefab en allDisplayIngredientsList para el tipo {tipoCantidad.tipo}");
                        }
                    }
                }
                else
                {
                    if (listCollider == null)
                        Debug.LogWarning("El prefab de remainingIngredientsPrefab no tiene Collider para calcular bounds.");
                }
            }
        }
        else
        {
            Debug.LogWarning("No se encontró BoxCollider en boxArea para CheckWinConditions.");
        }
    }

    // Método para limpiar los ingredientes mostrados anteriormente y sus textos
    private void ClearDisplayedIngredients()
    {
        foreach (GameObject go in displayedIngredients)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }
        displayedIngredients.Clear();
    }
}
