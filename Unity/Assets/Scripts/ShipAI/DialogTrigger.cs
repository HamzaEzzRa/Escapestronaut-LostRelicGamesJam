using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask triggerLayer = 1 << 8;
    [SerializeField] private int dialogId;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || ((1 << other.gameObject.layer) & triggerLayer) == 0)
        {
            return;
        }

        hasTriggered = true;
        GameEvents.CinematicAIDialogInvoke(dialogId);
    }
}
