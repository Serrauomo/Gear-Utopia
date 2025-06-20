using UnityEngine;

public class FloatingIslandParallax : MonoBehaviour
{
    [Tooltip("The camera that will follow the player")]
    public Transform cameraTransform;
    
    [Tooltip("How slowly the background moves compared to the camera (0 = stationary, 1 = same as camera)")]
    [Range(0f, 1f)]
    public float parallaxFactor = 0.3f;
    
    [Tooltip("Constant horizontal speed for wind effect (positive for right, negative for left)")]
    public float windSpeed = 0f; // Disabilitato di default per le isole
    
    [Header("Floating Animation")]
    [Tooltip("How high and low the island floats")]
    [Range(0.1f, 3f)]
    public float floatAmplitude = 0.5f;
    
    [Tooltip("How fast the island oscillates up and down")]
    [Range(0.1f, 5f)]
    public float floatSpeed = 1f;
    
    [Tooltip("Random offset for the floating animation")]
    [Range(0f, 6.28f)]
    public float floatOffset = 0f;
    
    private Vector3 lastCameraPosition;
    private Vector3 startPosition;
    private float timeAccumulator;
    
    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        lastCameraPosition = cameraTransform.position;
        startPosition = transform.position;
        
        // Se floatOffset è 0, genera un offset casuale
        if (floatOffset == 0f)
        {
            floatOffset = Random.Range(0f, Mathf.PI * 2f);
        }
        
        timeAccumulator = floatOffset;
    }
    
    void LateUpdate()
    {
        // Calcolo del movimento di parallasse (identico al tuo script originale)
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        Vector3 parallaxDisplacement = new Vector3(deltaMovement.x * parallaxFactor, deltaMovement.y * parallaxFactor, 0f);
        
        // Calcolo del movimento costante del vento (identico al tuo script originale)
        Vector3 windDisplacement = Vector3.right * windSpeed * Time.deltaTime;
        
        // NUOVO: Calcolo del movimento oscillante verticale
        timeAccumulator += Time.deltaTime * floatSpeed;
        float verticalFloat = Mathf.Sin(timeAccumulator) * floatAmplitude;
        
        // Applica tutti i movimenti
        transform.position += parallaxDisplacement + windDisplacement;
        
        // NUOVO: Aggiungi il movimento verticale oscillante
        transform.position = new Vector3(transform.position.x, startPosition.y + verticalFloat, transform.position.z);
        
        // Aggiorna startPosition per seguire il movimento orizzontale
        startPosition += parallaxDisplacement + windDisplacement;
        
        lastCameraPosition = cameraTransform.position;
    }
}