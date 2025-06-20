/*
 * MenuPowerUpController.cs
 * * Questo script gestisce l'animazione di un pannello UI e di uno
 * sfondo oscurato ("dimmer").
 *
 * NOVITÀ:
 * - Aggiunto un riferimento a "backgroundDimmer", un'immagine UI che
 * verrà usata per scurire lo schermo.
 * - L'animazione di entrata ora include un fade-in del dimmer.
 * - L'animazione di uscita include un fade-out del dimmer, che viene
 * poi disattivato per non bloccare l'input.
*/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // <-- Aggiunto per poter usare il tipo 'Image'
using DG.Tweening;

public class MenuPowerUpController : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [Tooltip("Trascina qui il pannello del menu che vuoi animare.")]
    [SerializeField] private RectTransform menuPanel;

    [Tooltip("Immagine usata per scurire lo sfondo quando il menu è aperto.")]
    [SerializeField] private Image backgroundDimmer;

    [Header("Input Action")]
    [Tooltip("Riferimento all'azione di input per attivare il menu.")]
    [SerializeField] private InputActionReference toggleActionReference;

    [Header("Impostazioni Animazione")]
    [Tooltip("La durata in secondi dell'animazione di entrata e uscita.")]
    [SerializeField] private float animationDuration = 0.8f;
    [Tooltip("Quante rotazioni complete (spin) deve fare il menu.")]
    [SerializeField] private int numberOfSpins = 2;
    [Tooltip("La rotazione iniziale sull'asse Y. 90 lo rende invisibile di taglio.")]
    [SerializeField] private float initialYRotation = 90f;
    [Tooltip("L'effetto 'easing' da applicare.")]
    [SerializeField] private Ease easeType = Ease.OutExpo;
    
    [Tooltip("Quanto scuro deve diventare lo sfondo (0 = trasparente, 1 = nero pieno).")]
    [Range(0, 1)]
    [SerializeField] private float dimmerOpacity = 0.7f;

    [Header("Posizioni del Menu")]
    [Tooltip("La posizione del menu quando è visibile sullo schermo.")]
    [SerializeField] private Vector2 positionOnScreen = new Vector2(0, 250);
    [Tooltip("La posizione del menu quando è nascosto fuori dallo schermo.")]
    [SerializeField] private Vector2 positionOffScreen = new Vector2(0, -250);

    private bool isMenuOpen = false;
    private Sequence animationSequence;

    private void Awake()
    {
        if (menuPanel != null)
        {
            menuPanel.anchoredPosition = positionOffScreen;
            menuPanel.localRotation = Quaternion.Euler(0, initialYRotation, 0);
        }

        // Assicurati che il dimmer sia invisibile e disattivato all'inizio
        if (backgroundDimmer != null)
        {
            backgroundDimmer.color = new Color(0, 0, 0, 0); // Nero con alpha 0
            backgroundDimmer.gameObject.SetActive(false);
        }
    }
    
    // ... i metodi OnEnable, OnDisable e OnTogglePerformed rimangono identici ...

    private void OnEnable() { if (toggleActionReference != null && toggleActionReference.action != null) { toggleActionReference.action.Enable(); toggleActionReference.action.performed += OnTogglePerformed; } }
    private void OnDisable() { if (toggleActionReference != null && toggleActionReference.action != null) { toggleActionReference.action.performed -= OnTogglePerformed; toggleActionReference.action.Disable(); } }
    private void OnTogglePerformed(InputAction.CallbackContext context) { ToggleMenu(); }
    
    public void ToggleMenu()
    {
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            ApriMenu();
        }
        else
        {
            ChiudiMenu();
        }
    }

    private void ApriMenu()
    {
        // Attiva e anima il dimmer per scurire lo sfondo
        if (backgroundDimmer != null)
        {
            backgroundDimmer.gameObject.SetActive(true);
            backgroundDimmer.DOFade(dimmerOpacity, animationDuration).SetEase(easeType);
        }
        
        // Parte l'animazione del pannello
        animationSequence = DOTween.Sequence();
        animationSequence.Append(menuPanel.DOAnchorPos(positionOnScreen, animationDuration));
        float targetYRotation = 360 * numberOfSpins;
        animationSequence.Join(menuPanel.DORotate(new Vector3(0, targetYRotation, 0), animationDuration, RotateMode.FastBeyond360));
        animationSequence.SetEase(easeType);
    }

    private void ChiudiMenu()
    {
        // Fa tornare trasparente il dimmer e lo disattiva a fine animazione
        if (backgroundDimmer != null)
        {
            backgroundDimmer.DOFade(0f, animationDuration).SetEase(easeType)
                .OnComplete(() => backgroundDimmer.gameObject.SetActive(false));
        }
        
        // Parte l'animazione del pannello
        animationSequence = DOTween.Sequence();
        animationSequence.Append(menuPanel.DOAnchorPos(positionOffScreen, animationDuration));
        float targetYRotation = 360 * numberOfSpins + initialYRotation;
        animationSequence.Join(menuPanel.DORotate(new Vector3(0, targetYRotation, 0), animationDuration, RotateMode.FastBeyond360));
        animationSequence.SetEase(easeType);
    }
}
