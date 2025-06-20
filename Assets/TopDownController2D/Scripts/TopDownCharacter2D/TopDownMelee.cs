using TopDownCharacter2D.Attacks;
using TopDownCharacter2D.Attacks.Melee;
using TopDownCharacter2D.Controllers;
using UnityEngine;

namespace TopDownCharacter2D
{
    /// <summary>
    ///     Handles the logic behind a close combat attack
    /// </summary>
    [RequireComponent(typeof(TopDownCharacterController))]
    public class TopDownMelee : MonoBehaviour
    {
        [SerializeField] private GameObject attackObject;

        [SerializeField] [Tooltip("The pivot point of the attack")]
        private Transform attackPivot;

        private Vector2 _attackDirection;


        private TopDownCharacterController _controller;

        private void Awake()
        {
            _controller = GetComponent<TopDownCharacterController>();
        }

        private void Start()
        {
            _controller.OnAttackEvent.AddListener(Attack);
            _controller.LookEvent.AddListener(Rotate);
        }

        private void Attack(AttackConfig config)
        {
            if (!(config is MeleeAttackConfig))
            {
                return;
            }

            InstantiateAttack((MeleeAttackConfig) config);
        }

        private void Rotate(Vector2 rotation)
        {
            _attackDirection = rotation;
        }

        /// <summary>
        ///     Creates an attack object
        /// </summary>
        /// <param name="attackConfig"> The configuration on the melee attack</param>
        private void InstantiateAttack(MeleeAttackConfig attackConfig)
        {
            attackPivot.localRotation = Quaternion.identity;
            GameObject obj = Instantiate(attackObject, attackPivot.position,
                Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, _attackDirection)), attackPivot);
            MeleeAttackController attackController = obj.GetComponent<MeleeAttackController>();
            attackController.InitializeAttack(attackConfig);
        }
    }
}