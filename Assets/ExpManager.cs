using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;

public class ExpManager : MonoBehaviour
{
    public static ExpManager Instance { get; private set; }

    [Header("Experience Settings")]
    public int level = 1;
    public int currentExp = 0;
    public int expToLevel = 10;
    public float expGrowthMultiplier = 1.2f;
    
    [Header("UI References")]
    public Slider expSlider;
    public TMP_Text currentLevelText;
    public TMP_Text additionalText;
    public Image expBarFill;
    public Image expBarBackground;
    
    [Header("Level Up Effect")]
    public GameObject levelUpEffectPrefab;
    public AudioClip levelUpSound;
    
    [Header("Animation Settings")]
    public float levelUpAnimationDuration = 2f;
    public float expBarFillDuration = 0.5f;
    public float expBarFillToMaxDuration = 0.3f; // Faster when reaching max
    public float glowIntensity = 10f;
    public float scaleStrength = 0.3f;
    public Color normalFillColor = Color.blue;
    public Color normalBackgroundColor = Color.gray;
    public Color hdrGlowColor = Color.white;
    
    // Private fields
    private Color originalTextColor;
    private Color originalAdditionalTextColor;
    private AudioSource audioSource;
    private Material expBarMaterial;
    private bool isLevelingUp = false;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializeComponents();
        UpdateUI();
    }

    private void Update()
    {
        HandleInput();
    }

    #region Initialization
    private void InitializeComponents()
    {
        SetupAudio();
        SetupColors();
        SetupMaterial();
    }

    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void SetupColors()
    {
        if (expBarFill != null) expBarFill.color = normalFillColor;
        if (expBarBackground != null) expBarBackground.color = normalBackgroundColor;
        
        originalTextColor = currentLevelText.color;
        if (additionalText != null) originalAdditionalTextColor = additionalText.color;
    }

    private void SetupMaterial()
    {
        // Get the XP bar material to control emission
        if (expBarFill != null)
        {
            // Create a copy of the material to avoid modifying the original asset
            expBarMaterial = new Material(expBarFill.material);
            expBarFill.material = expBarMaterial;
            SetEmissionGlow(false); // No glow initially
        }
    }

    private void SetEmissionGlow(bool enabled)
    {
        if (expBarMaterial == null) return;
        
        if (enabled)
        {
            expBarMaterial.SetColor("_EmissionColor", hdrGlowColor * glowIntensity);
            expBarMaterial.EnableKeyword("_EMISSION");
        }
        else
        {
            expBarMaterial.SetColor("_EmissionColor", Color.black);
            expBarMaterial.DisableKeyword("_EMISSION");
            // Make sure emission is completely off
            expBarMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
    }
    #endregion

    #region Input Handling
    private void HandleInput()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
            GainExperience(expToLevel - currentExp);
        
        if (Keyboard.current.kKey.wasPressedThisFrame)
            GainExperience(2);
    }
    #endregion

    #region Experience System
    public void GainExperience(int amount)
    {
        int targetExp = currentExp + amount;
        
        if (targetExp >= expToLevel && !isLevelingUp)
        {
            // Animate quickly to max, then level up
            currentExp = targetExp; // Update immediately for calculations
            AnimateExpBar(expToLevel, () => StartCoroutine(LevelUpSequence()), expBarFillToMaxDuration);
        }
        else
        {
            // Animate normally (even during level up)
            currentExp = targetExp;
            
            if (!isLevelingUp)
            {
                AnimateExpBar(currentExp);
            }
            else
            {
                // During level up, stop the current animation and animate to the new value
                AnimateExpBarDuringLevelUp();
            }
        }
    }

    private IEnumerator LevelUpSequence()
    {
        isLevelingUp = true;
        
        // 1. Calculate new level (currentExp is already updated)
        int oldLevel = level;
        level++;
        currentExp -= expToLevel;
        expToLevel = Mathf.RoundToInt(expToLevel * expGrowthMultiplier);
        
        // 2. Reset the bar to 0 and set new max
        if (expSlider != null)
        {
            expSlider.maxValue = expToLevel;
            expSlider.value = 0;
        }
        
        // 3. Play audio and spawn particles
        PlayLevelUpSound();
        SpawnLevelUpParticles();
        
        // 4. Start simultaneous animations
        StartGlowAnimation();
        
        // 5. Start scale animation and animate the bar in parallel
        StartCoroutine(ScaleAndTextAnimation(oldLevel));
        
        // 6. Animate the bar towards the current value (which may change during the animation)
        AnimateExpBarDuringLevelUp();
        
        // 7. Wait for the animations to finish
        yield return new WaitForSeconds(levelUpAnimationDuration);
        
        isLevelingUp = false;
        
        // 8. Check if in the meantime we have gained enough exp for another level up
        if (currentExp >= expToLevel)
        {
            yield return StartCoroutine(LevelUpSequence());
        }
    }

    private void AnimateExpBarDuringLevelUp()
    {
        if (expSlider == null) return;
        
        // Stop any ongoing animation and animate to the new currentExp value
        DOTween.Kill("expBar");
        
        // Animate from the current position to the new currentExp value
        expSlider.DOValue(currentExp, expBarFillDuration * 0.7f) // Slightly faster during level up
            .SetEase(Ease.OutQuart)
            .SetId("expBar");
    }
    #endregion

    #region Level Up Effects
    private void PlayLevelUpSound()
    {
        if (levelUpSound != null && audioSource != null)
            audioSource.PlayOneShot(levelUpSound);
    }

    private void SpawnLevelUpParticles()
    {
        if (levelUpEffectPrefab != null)
            Instantiate(levelUpEffectPrefab, transform.position, Quaternion.identity);
    }

    private void StartGlowAnimation()
    {
        float glowDuration = levelUpAnimationDuration * 0.6f; // 60% of the total duration
        
        // Enable emission for bloom
        SetEmissionGlow(true);
        
        // Animate UI colors
        AnimateColorGlow(expBarFill, normalFillColor, glowDuration);
        AnimateColorGlow(expBarBackground, normalBackgroundColor, glowDuration);
        AnimateTextGlow(currentLevelText, originalTextColor, glowDuration);
        
        if (additionalText != null)
            AnimateTextGlow(additionalText, originalAdditionalTextColor, glowDuration);
    }

    private void AnimateColorGlow(Image image, Color normalColor, float duration)
    {
        if (image == null) return;
        
        image.DOColor(hdrGlowColor, duration * 0.3f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => 
            {
                image.DOColor(normalColor, duration * 0.7f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => 
                    {
                        SetEmissionGlow(false); // Disable emission at the end
                        // Force material refresh
                        if (expBarFill != null) expBarFill.SetMaterialDirty();
                    });
            });
    }

    private void AnimateTextGlow(TMP_Text text, Color originalColor, float duration)
    {
        if (text == null) return;
        
        text.DOColor(hdrGlowColor, duration * 0.3f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => 
            {
                text.DOColor(originalColor, duration * 0.7f)
                    .SetEase(Ease.InQuad);
            });
    }

    private IEnumerator ScaleAndTextAnimation(int oldLevel)
    {
        Transform barTransform = expSlider?.transform ?? transform;
        
        // Phase 1: Scale up with bounce (40% of the time)
        float phase1Duration = levelUpAnimationDuration * 0.4f;
        barTransform.DOScale(Vector3.one * (1f + scaleStrength), phase1Duration)
            .SetEase(Ease.OutBack);
        
        yield return new WaitForSeconds(phase1Duration * 0.7f); // Wait a bit before changing the text
        
        // Change the level text during the animation
        UpdateLevelText();
        
        yield return new WaitForSeconds(phase1Duration * 0.3f); // Complete phase 1
        
        // Phase 2: Scale down smooth (60% of the time)
        float phase2Duration = levelUpAnimationDuration * 0.6f;
        barTransform.DOScale(Vector3.one, phase2Duration)
            .SetEase(Ease.OutElastic);
        
        yield return new WaitForSeconds(phase2Duration);
    }
    #endregion

    #region UI Updates
    private void UpdateUI()
    {
        UpdateExpSlider();
        UpdateLevelText();
    }

    private void UpdateExpSlider()
    {
        if (expSlider == null) return;
        
        expSlider.maxValue = expToLevel;
        expSlider.value = currentExp;
    }

    private void AnimateExpBar(float targetValue, System.Action onComplete = null, float? customDuration = null)
    {
        if (expSlider == null) return;
        
        // Kill any existing bar animations
        DOTween.Kill("expBar");
        
        // Use custom duration or default one
        float duration = customDuration ?? expBarFillDuration;
        
        // Animate the bar towards the target value
        expSlider.DOValue(targetValue, duration)
            .SetEase(Ease.OutQuart)
            .SetId("expBar")
            .OnComplete(() => onComplete?.Invoke());
    }

    private void UpdateLevelText()
    {
        if (currentLevelText != null)
            currentLevelText.text = $"Level: {level}";
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        DOTween.Kill(transform);
        DOTween.Kill("expBar");
        if (expSlider != null) DOTween.Kill(expSlider.transform);
    }

    private void OnValidate()
    {
        if (Application.isPlaying) SetupColors();
    }
    #endregion
}