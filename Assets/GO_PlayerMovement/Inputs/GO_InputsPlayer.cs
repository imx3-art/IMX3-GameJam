using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class GO_InputsPlayer : StarterAssetsInputs
    {
        [Header("Custom Input Values")]
        public bool stealth;
        public static bool IsPause = false;

#if ENABLE_INPUT_SYSTEM
        public new void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }
        public void OnStealth(InputValue value)
        {
            StealthInput(value.isPressed);
        }
#endif
        public void StealthInput(bool newStealthState)
        {
            stealth = newStealthState;
        }

        public new void MoveInput(Vector2 newMoveDirection)
        {
            move.x = IsPause ? Vector2.zero.x : newMoveDirection.x;
            move.y = IsPause ? Vector2.zero.y : newMoveDirection.y;
        }
    }
}