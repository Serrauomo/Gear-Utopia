using UnityEngine;

public class ShowObjectOnAnimationStart : MonoBehaviour
{
    public GameObject objectToShow; // Il GameObject da mostrare

    // Questo metodo va chiamato tramite Animation Event
    public void ShowObject()
    {
        if (objectToShow != null)
            objectToShow.SetActive(true);
        else
            Debug.LogWarning("objectToShow non assegnato!");
    }
}
