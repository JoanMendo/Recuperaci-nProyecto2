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

    public GameObject currentIngredient;
    public Coroutine ingredientCoroutine;

    [SerializeField] private GameObject boxArea;


   [SerializeField] private float minIngredientValue = 0;
    [SerializeField] private float maxIngredientValue = 3; //0 es el ingrediente inicial y por cada combinacion ves sumando 1 (lechuga 0, tomate 1, cebolla 2, etc)
    [SerializeField] private List<TipoCantidad> winCondition = new List<TipoCantidad>();
    [SerializeField] private List<TipoCantidad> ingredientesEnCaja = new List<TipoCantidad>();


    public void Awake() //Esto para hacerlo singleton
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

    public void Start() //Esto pal ingrediente inicial
    {
        if (currentIngredient == null)
        {
            ingredientCoroutine = StartCoroutine(SetCurrentIngredient());
        }
    }

    public void DropIngredient() //Esto para soltar el ingrediente actual y generar otro nuevo
    {
        if (ingredientCoroutine == null && CursorManager.instance.canDropIngrdient)
        {
            currentIngredient.GetComponent<Rigidbody>().useGravity = true;
            currentIngredient = null;
            ingredientCoroutine = StartCoroutine(SetCurrentIngredient());
            CheckWinConditions();
        }

    }

    private GameObject SelectRandomIngredient() //Esto para la pool de ingredientes que se generan aleatoriamente
    {

        if (allIngredientsList.Count > 0)
        {
            List<GameObject> filteredIngredients = allIngredientsList.Where(ingredient =>
            (int)ingredient.GetComponent<Ingredient>().ingredientType >= minIngredientValue &&
            (int)ingredient.GetComponent<Ingredient>().ingredientType <= maxIngredientValue).ToList();

            int randomIndex = UnityEngine.Random.Range(0, filteredIngredients.Count);
            return(filteredIngredients[randomIndex]);

        }
        return null;

    }

    public IEnumerator SetCurrentIngredient()
    {
        yield return new WaitForSeconds(2f); // Cooldown entre ingredientes
        GameObject ingredientPrefab = SelectRandomIngredient();
        currentIngredient = Instantiate(ingredientPrefab, CursorManager.instance.currentObjectPosition, Quaternion.identity);
        currentIngredient.GetComponent<Rigidbody>().useGravity = false;
        currentIngredient.GetComponent<Ingredient>().SetModificators();
        ingredientCoroutine = null; // Reinicia la coroutine para que no se repita ni nada raro

    }


    public void CheckWinConditions()
    {
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
            Collider[] colliders = Physics.OverlapBox(worldCenter, halfExtents, colider.transform.rotation); //Aqui no puedes usar colider.bounds que eso no te da datos de posicion global sino local

            foreach (Collider collider in colliders)
            {
               
                if (collider.gameObject.TryGetComponent<Ingredient>(out Ingredient ingredient))
                {

                    TipoCantidad tipoCantidad = ingredientesEnCaja.FirstOrDefault(x => x.tipo == ingredient.ingredientType);
                        tipoCantidad.cantidad++;


                }
            }

            //Comprobar que todos los ingredientes del winCondition esten en la caja (pueden haber sobrantes)
           foreach (TipoCantidad winIngredient in winCondition)
            {
      
                if (!ingredientesEnCaja.Any(x => x.tipo == winIngredient.tipo && x.cantidad >= winIngredient.cantidad))
                {
                    Debug.Log($"No se cumple la condición de victoria para {winIngredient.tipo}. Se necesitan al menos {winIngredient.cantidad}.");
                    return; // Si no se cumple una condición, salimos del método
                }
            }

            Debug.Log("¡Se cumplen las condiciones de victoria!"); // Si llega hasta aqui, se han cumplido todas las condiciones
            // Aquí puedes añadir la lógica para finalizar el juego o mostrar un mensaje de victoria.
        }
    }

}
