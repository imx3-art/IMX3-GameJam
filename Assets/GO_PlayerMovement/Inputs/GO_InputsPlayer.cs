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

#if ENABLE_INPUT_SYSTEM
        public void OnStealth(InputValue value)
        {
            StealthInput(value.isPressed);
        }
#endif

        public void StealthInput(bool newStealthState)
        {
            stealth = newStealthState;
        }
    }
}