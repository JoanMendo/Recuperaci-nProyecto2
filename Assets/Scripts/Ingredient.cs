using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Modifier
{
    public ModificatorsType modType;
    public bool isActive;
    public float chance;
}

[System.Serializable]
public class IngredientsModifiers
{
    public List<Modifier> modifiers = new List<Modifier>
    {
        new Modifier {modType = ModificatorsType.Small, isActive = false, chance = 10f },
        new Modifier {modType = ModificatorsType.Large, isActive = false, chance = 10f },
        new Modifier { modType = ModificatorsType.Bouncy, isActive = false, chance = 50f },
        new Modifier { modType = ModificatorsType.Heavy, isActive = false,chance = 50f },
    };
}

public class Ingredient : MonoBehaviour
{
    private IngredientsModifiers modifiers = new IngredientsModifiers();
    [SerializeField] public IngredientType ingredientType = IngredientType.None;
    [SerializeField] private GameObject combinationPrefab;
    [SerializeField] private PhysicsMaterial bouncyMaterial;
    [SerializeField] private Material metalMaterial;
    [SerializeField] private Material slimyMaterial;
    [SerializeField] private Material metalAndSlimyMaterial;
    private bool canCombine = true;

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Ingredient>() != null)
        {
            Ingredient other = collision.gameObject.GetComponent<Ingredient>();
            if (other.ingredientType == ingredientType && (other.canCombine == true && canCombine))
            {
                canCombine = false;
                other.canCombine = false;
               CombineIngredients(this, other);

            }
        }
    }

    public void SetModificators()
    {
        for (int i = 0; i < modifiers.modifiers.Count; i++)
        {
            float randomValue = UnityEngine.Random.Range(0f, 100f);
            Modifier modifier = modifiers.modifiers[i];
            modifier.isActive = randomValue <= modifier.chance;
            modifiers.modifiers[i] = modifier; // Esto asi que si lo intentas con un foreach no deja
        }
        ApplyModificators();
    }

    public void CombineIngredients(Ingredient ingredient1, Ingredient ingredient2)
    {
        GameObject combinedObject = Instantiate(combinationPrefab, (ingredient1.transform.position + ingredient2.transform.position) / 2, Quaternion.identity);
        AudioManager.Instance.PlayIngredientMerge();
        Ingredient combinedIngredient = combinedObject.GetComponent<Ingredient>();
        foreach (Modifier modifier in ingredient1.modifiers.modifiers)
        {
            if (modifier.isActive)
            {
                combinedIngredient.modifiers.modifiers.Add(modifier);
            }
        }
        foreach (Modifier modifier in ingredient2.modifiers.modifiers)
        {
            if (modifier.isActive)
            {
                combinedIngredient.modifiers.modifiers.Add(modifier);
            }
        }
        bool isSmall = false;
        bool isBig = false;
        

        foreach (Modifier modifier in combinedIngredient.modifiers.modifiers)
        {
            if (modifier.modType == ModificatorsType.Small && modifier.isActive)
            {
                isSmall = true;
            }
            if (modifier.modType == ModificatorsType.Large && modifier.isActive)
            {
                isBig = true;
            }
            
        }
        if (isSmall && isBig)
        {
            combinedIngredient.modifiers.modifiers.RemoveAll(mod => mod.modType == ModificatorsType.Small || mod.modType == ModificatorsType.Large);
        }
        

        Destroy(ingredient1.gameObject);
        Destroy(ingredient2.gameObject);
        combinedObject.GetComponent<Ingredient>().ApplyModificators();
    }

    public void ApplyModificators()
    {

        bool isBouncy = false;
        bool isHeavy = false;

        foreach (Modifier modifier in modifiers.modifiers)
        {
            if (!modifier.isActive) continue;

            switch (modifier.modType)
            {
                case ModificatorsType.Small:
                   gameObject.transform.localScale *= 0.5f;
                    Debug.Log("Aplicando modificador: Small");
                    break;

                case ModificatorsType.Large:
                    gameObject.transform.localScale *= 2f;
                    Debug.Log("Aplicando modificador: Large");
                    break;

                case ModificatorsType.Bouncy:

                    Collider bouncyCollider = gameObject.GetComponent<Collider>();
                    bouncyCollider.material = bouncyMaterial;
                    isBouncy = true; // Marca que el ingrediente es rebotador
                    Debug.Log("Aplicando modificador: Bouncy");
                    break;

                case ModificatorsType.Heavy:

                    Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                    rb.mass *= 10f; // Aumenta la masa del Rigidbody

                    isHeavy = true; // Marca que el ingrediente es pesado
                    Debug.Log("Aplicando modificador: Heavy");

                    break;


                default:
                    Debug.Log("Modificador no reconocido.");
                    break;
            }
        }
        if (isBouncy && isHeavy)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = metalAndSlimyMaterial;
                }
                renderer.materials = materials;
            }
        }
        else if (isBouncy)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = slimyMaterial;
                }
                renderer.materials = materials;
            }
        }
        else if (isHeavy)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = metalMaterial;
                }
                renderer.materials = materials;
            }
        }
    }




}
