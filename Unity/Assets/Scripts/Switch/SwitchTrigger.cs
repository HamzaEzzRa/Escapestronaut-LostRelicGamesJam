using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SwitchTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask triggerLayer = 1 << 9;

    private GameLevel parentLevel;
    private GravitySwitch gravitySwitch;

    private void Awake()
    {
        parentLevel = GetComponentInParent<GameLevel>();
        gravitySwitch = GetComponentInParent<GravitySwitch>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & triggerLayer) == 0 || parentLevel == null || gravitySwitch == null || gravitySwitch.IsActivating)
        {
            return;
        }

        GameEvents.GravitySwitchInvoke();
    }
}
