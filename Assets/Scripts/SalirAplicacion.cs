using UnityEngine;

public class SalirAplicacion : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clip;
    public void Salir()
    {
        audioSource.PlayOneShot(clip);
        Application.Quit();
        Debug.Log("Salir() llamado - no funciona en el editor de Unity.");
    }
}