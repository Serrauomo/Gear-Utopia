using TopDownCharacter2D.FX;
using UnityEngine;

namespace TopDownCharacter2D.Health
{
    /// <summary>
    ///     Handles the removal of an entity when it dies
    /// </summary>
    public class DisappearOnDeath : MonoBehaviour
    {
        public void OnDeath()
        {
            foreach (Behaviour component in transform.GetComponentsInChildren<Behaviour>())
            {
                if (!(component is AudioSource) && !(component is TopDownFx))
                {
                    component.enabled = false;
                }
            }

            foreach (Renderer component in transform.GetComponentsInChildren<Renderer>())
            {
                if (!(component is ParticleSystemRenderer))
                {
                    component.enabled = false;
                }
            }
            
            // We wait before destroying the object in order to properly end all the related effects
            Destroy(gameObject, 20f);
        }
    }
}