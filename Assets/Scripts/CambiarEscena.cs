using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarEscena : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clip;
    private int numeroEscena=1;

    public void CargarEscena()
    {
        audioSource.PlayOneShot(clip);
        SceneManager.LoadScene(numeroEscena);

    }
}
