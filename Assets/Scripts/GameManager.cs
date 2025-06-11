using NUnit.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System;
using UnityEngine.SceneManagement;

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
    public GameObject remainingIngredientsPrefab;

    public GameObject currentIngredient;
    public Coroutine ingredientCoroutine;

    [SerializeField] private GameObject boxArea;

    [SerializeField] private float minIngredientValue = 0;
    [SerializeField] private float maxIngredientValue = 3;
    [SerializeField] private List<TipoCantidad> winCondition = new List<TipoCantidad>();
    [SerializeField] private List<TipoCantidad> ingredientesEnCaja = new List<TipoCantidad>();
    [SerializeField] private List<TipoCantidad> remainingIngredientsList = new List<TipoCantidad>();


      private List<GameObject> displayedIngredients = new List<GameObject>();

    private Vector3 originalRemainingScale;


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

        ClearDisplayedIngredients();


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


            if (remainingIngredientsContainerInstance != null)
            {
                Destroy(remainingIngredientsContainerInstance);
                remainingIngredientsContainerInstance = null;
            }

            if (totalIngredientesFaltantes > 0)
            {
                remainingIngredientsContainerInstance = Instantiate(remainingIngredientsPrefab);

                 float newScaleY = originalRemainingScale.y * (totalIngredientesFaltantes / 4f);
                remainingIngredientsContainerInstance.transform.localScale = new Vector3(
                    originalRemainingScale.x,
                    newScaleY/4f,
                    originalRemainingScale.z
                );

                // Obtener el collider de la instancia recién creada para calcular bounds
                Collider listCollider = remainingIngredientsContainerInstance.GetComponent<Collider>();
                if (listCollider != null && remainingIngredientsList.Count > 0)
                {
  
                    Bounds bounds = listCollider.bounds;
                    float totalHeight = bounds.size.y;
                    float spacing = totalHeight / (remainingIngredientsList.Count + 1);

   
                    float textX = bounds.center.x + textOffsetFromContainer.x;
                    float textZ = bounds.center.z + textOffsetFromContainer.y; 

                    float currentY = bounds.min.y + spacing;

                    foreach (TipoCantidad tipoCantidad in remainingIngredientsList)
                    {
                        // Buscar prefab de display para este tipo
                        GameObject ingredientPrefab = allDisplayIngredientsList.FirstOrDefault(x =>
                            x.GetComponent<DisplayIngredient>().ingredientType == tipoCantidad.tipo);

                        if (ingredientPrefab != null)
                        {

                              GameObject ingredientInstance = Instantiate(ingredientPrefab);

    
                            Vector3 ingredientPos = new Vector3(
                                bounds.center.x - 7.5f,
                                currentY,
                                bounds.center.z - 1.5f
                            );
                            ingredientInstance.transform.position = ingredientPos;
                            displayedIngredients.Add(ingredientInstance);

                            int faltan = tipoCantidad.cantidad;
                            if (faltan > 0)
                            {
                                int adicionales = faltan; 
                                GameObject textGO = new GameObject("RemainingCountText");

                             
                                Vector3 textPosition = new Vector3(textX, currentY, textZ);
                                textGO.transform.position = textPosition + new Vector3(3, -4, -3);

                                // Rotación fija de (0, 45, 0)
                                textGO.transform.rotation = Quaternion.Euler(new Vector3(0f,45f,0f));


                                TextMesh textMesh = textGO.AddComponent<TextMesh>();
                                textMesh.text = "x" + adicionales;
                                textMesh.fontSize = countTextFontSize;
                                textMesh.color = countTextColor;
                                textMesh.fontStyle = FontStyle.Bold;
                                textMesh.alignment = TextAlignment.Center;
                                textMesh.anchor = TextAnchor.MiddleCenter;

                                if (countTextMaterial != null)
                                {
                                    MeshRenderer mr = textGO.GetComponent<MeshRenderer>();
                                    if (mr != null)
                                    {
                                        mr.material = countTextMaterial;
                                    }
                                }


                                displayedIngredients.Add(textGO);
                            }
                          
                            currentY += spacing;
                        }
                       
                    }
                }
             
            }
            else if (remainingIngredientsList.Count == 0)
            {
                SceneManager.LoadScene(3);
            }
        }

    }

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
