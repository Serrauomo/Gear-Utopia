using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    [Tooltip("The camera that will follow the player")]
    public Transform cameraTransform;

    [Tooltip("How slowly the background moves compared to the camera (0 = stationary, 1 = same as camera)")]
    [Range(0f, 1f)]
    public float parallaxFactor = 0.5f;

    [Tooltip("Constant horizontal speed for wind effect (positive for right, negative for left)")]
    public float windSpeed = 0.1f; // Aggiungi questa nuova variabile

    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        // Calcolo del movimento di parallasse
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        Vector3 parallaxDisplacement = new Vector3(deltaMovement.x * parallaxFactor, deltaMovement.y * parallaxFactor, 0f);

        // Calcolo del movimento costante del vento
        // Moltiplichiamo per Time.deltaTime per rendere il movimento indipendente dal frame rate
        Vector3 windDisplacement = Vector3.right * windSpeed * Time.deltaTime;

        // Applica entrambi i movimenti
        transform.position += parallaxDisplacement + windDisplacement;

        lastCameraPosition = cameraTransform.position;
    }
}