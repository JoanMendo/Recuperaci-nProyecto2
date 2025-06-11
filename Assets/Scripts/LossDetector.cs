using UnityEngine;
using UnityEngine.SceneManagement;

public class LossDetector : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {
            SceneManager.LoadScene(2);
        }
    }
}
