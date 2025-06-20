using TopDownCharacter2D.Attacks;
using UnityEngine;
using UnityEngine.Events;

namespace TopDownCharacter2D.Controllers
{
    /// <summary>
    ///     An event representing a movement input in the direction of its parameter
    /// </summary>
    public class MoveEvent : UnityEvent<Vector2> { }

    /// <summary>
    ///     An event representing an attack input with the given configuration
    /// </summary>
    public class AttackEvent : UnityEvent<AttackConfig> { }

    /// <summary>
    ///     An event representing a look input (where the character must look) in the direction of its parameter
    /// </summary>
    public class LookEvent : UnityEvent<Vector2> { }
}