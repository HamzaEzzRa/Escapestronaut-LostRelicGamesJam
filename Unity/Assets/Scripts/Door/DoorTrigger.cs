using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private GameDoor door;
    [SerializeField] private LayerMask triggerLayer = 1 << 8;

    public bool IsOpened
    {
        get => isOpened;
        set
        {
            isOpened = value;
        }
    }
    
    private bool isOpened;

    private void OnTriggerEnter(Collider other)
    {
        if (IsOpened || ((1 << other.gameObject.layer) & triggerLayer) == 0)
        {
            return;
        }

        IsOpened = true;
        GameEvents.DoorTriggerOpenInvoke(door.Id);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOpened || ((1 << other.gameObject.layer) & triggerLayer) == 0)
        {
            return;
        }

        IsOpened = false;
        GameEvents.DoorTriggerCloseInvoke(door.Id);
    }
}
