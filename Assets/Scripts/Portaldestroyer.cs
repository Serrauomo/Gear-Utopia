using UnityEngine;

public class DestroyTarget : MonoBehaviour
{
    public GameObject targetToDestroy; // Oggetto da distruggere
    public float delay = 0f; // Ritardo opzionale in secondi

    public void DestroyNow()
    {
        if (targetToDestroy != null)
        {
            Destroy(targetToDestroy, delay);
        }
        else
        {
            Debug.LogWarning("Nessun oggetto assegnato a 'targetToDestroy'.");
        }
    }
}
