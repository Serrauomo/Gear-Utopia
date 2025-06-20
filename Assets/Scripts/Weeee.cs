using UnityEngine;
using TopDownCharacter2D.Health;

/// <summary>
/// Script temporaneo per debuggare il sistema di danno e flash rosso
/// </summary>
public class DamageDebugScript : MonoBehaviour
{
    private HealthSystem _healthSystem;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
        _animator = GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    
    private void Start()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnDamage.AddListener(OnDamageReceived);
            _healthSystem.OnInvincibilityEnd.AddListener(OnInvincibilityEnd);
        }
    }
    
    private void OnDamageReceived()
    {
        Debug.Log("=== DAMAGE DEBUG ===");
        Debug.Log($"Health System: {(_healthSystem != null ? "OK" : "MISSING")}");
        Debug.Log($"Animator: {(_animator != null ? "OK" : "MISSING")}");
        Debug.Log($"Sprite Renderer: {(_spriteRenderer != null ? "OK" : "MISSING")}");
        
        if (_animator != null)
        {
            Debug.Log($"Animator Controller: {(_animator.runtimeAnimatorController != null ? _animator.runtimeAnimatorController.name : "MISSING")}");
            
            // Verifica parametri
            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                Debug.Log($"Animator Parameter: {param.name} ({param.type})");
            }
            
            // Forza il parametro IsHurt a true
            if (HasParameter(_animator, "IsHurt"))
            {
                _animator.SetBool("IsHurt", true);
                Debug.Log("Set IsHurt to TRUE");
            }
            else
            {
                Debug.Log("IsHurt parameter NOT FOUND!");
            }
        }
        
        if (_spriteRenderer != null)
        {
            Debug.Log($"Current Color: {_spriteRenderer.color}");
            Debug.Log($"Material: {_spriteRenderer.material.name}");
            Debug.Log($"Shader: {_spriteRenderer.material.shader.name}");
        }
    }
    
    private void OnInvincibilityEnd()
    {
        Debug.Log("=== INVINCIBILITY END ===");
        
        if (_animator != null && HasParameter(_animator, "IsHurt"))
        {
            _animator.SetBool("IsHurt", false);
            Debug.Log("Set IsHurt to FALSE");
        }
    }
    
    private bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}