/*
 * StatUpgradeUI.cs
 * * ISTRUZIONI:
 * 1. Crea un oggetto vuoto per rappresentare una singola riga di statistica 
 * (es. un oggetto "SpeedStatRow").
 * 2. Assegna questo script a quell'oggetto.
 * 3. Trascina all'interno dello script, nell'Inspector, i seguenti elementi:
 * - Le 5 immagini dei quadretti (nell'ordine corretto!) nella lista "Stat Images".
 * - Il pulsante "+" nello slot "Upgrade Button".
 * - L'immagine del simbolo "+" nello slot "Plus Sign Image" per l'effetto glow.
 * 4. Imposta i colori per lo stato di default, potenziato e per il glow.
 * 5. Trasforma questo oggetto in un Prefab per riutilizzarlo facilmente.
 *
 * NOVITÀ:
 * - Aggiunto riferimento a "plusSignImage" e un "glowColor".
 * - Quando si preme il pulsante, l'immagine del "+" avrà un breve effetto luminoso.
*/
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class StatUpgradeUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [Tooltip("Lista delle immagini che rappresentano i livelli della statistica. Devono essere in ordine.")]
    [SerializeField] private List<Image> statImages;
    
    [Tooltip("Il pulsante per eseguire l'upgrade.")]
    [SerializeField] private Button upgradeButton;
    
    [Tooltip("L'immagine del segno '+' da animare quando si preme il pulsante.")]
    [SerializeField] private Image plusSignImage;

    [Header("Impostazioni Upgrade")]
    [Tooltip("Il colore del quadretto quando la statistica è potenziata.")]
    [SerializeField] private Color upgradedColor = Color.white;
    
    [Tooltip("Il colore di default dei quadretti.")]
    [SerializeField] private Color defaultColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Grigio
    
    [Tooltip("Il colore del 'glow' per il pulsante quando premuto.")]
    [SerializeField] private Color glowColor = new Color(1f, 1f, 0.5f, 1f); // Giallo chiaro
    
    [Tooltip("Durata dell'animazione di upgrade.")]
    [SerializeField] private float animationDuration = 0.4f;

    private int currentLevel = 0;
    private Color plusSignOriginalColor;

    void Start()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(UpgradeStat);
        }

        // Salviamo il colore originale del segno + per l'animazione glow
        if (plusSignImage != null)
        {
            plusSignOriginalColor = plusSignImage.color;
        }

        ResetVisuals();
    }

    public void ResetVisuals()
    {
        foreach (var image in statImages)
        {
            image.color = defaultColor;
        }
        currentLevel = 0;
        upgradeButton.interactable = true;
    }

    private void UpgradeStat()
    {
        // ---- Animazione GLOW sul pulsante ----
        if (plusSignImage != null)
        {
            // Crea una sequenza flash: diventa giallo e torna al colore originale
            DOTween.Sequence()
                .Append(plusSignImage.DOColor(glowColor, 0.1f))
                .Append(plusSignImage.DOColor(plusSignOriginalColor, 0.2f));
        }
        // ---- Fine Animazione GLOW ----

        if (currentLevel >= statImages.Count)
        {
            return;
        }

        Image imageToUpgrade = statImages[currentLevel];
        AnimateUpgrade(imageToUpgrade);
        currentLevel++;

        if (currentLevel >= statImages.Count)
        {
            upgradeButton.interactable = false;
        }
    }

    private void AnimateUpgrade(Image targetImage)
    {
        Sequence sequence = DOTween.Sequence();
        Vector3 initialScale = targetImage.transform.localScale;
        sequence.Append(targetImage.transform.DOPunchScale(initialScale * 0.4f, animationDuration, 10, 1));
        sequence.Join(targetImage.DOColor(upgradedColor, animationDuration * 0.5f));
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(UpgradeStat);
        }
    }
}
