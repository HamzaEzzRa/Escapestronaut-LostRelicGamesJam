using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class JetNozzle : MonoBehaviour
{
    public enum NozzleDirection
    {
        NORTH,
        SOUTH,
        EAST,
        WEST,
    }

    public enum NozzleType
    {
        BACKPACK_NOZZLE,
        ROPE_NOZZLE,
    }

    public NozzleDirection Direction => direction;
    public NozzleType Type => type;

    [SerializeField] NozzleDirection direction = default;
    [SerializeField] NozzleType type = default;

    private VisualEffect effect;
    private bool isPlaying;

    private void Start()
    {
        effect = GetComponent<VisualEffect>();
    }

    public void PlayEffect()
    {
        if (!isPlaying)
        {
            effect.Play();
            isPlaying = true;
        }
    }

    public void StopEffect()
    {
        if (isPlaying)
        {
            effect.Stop();
            isPlaying = false;
        }
    }

    private void OnEnable()
    {
        if (type == NozzleType.BACKPACK_NOZZLE)
        {
            JetNozzleManager.backpackNozzles.Add(this);
        }
        else if (type == NozzleType.ROPE_NOZZLE)
        {
            JetNozzleManager.ropeNozzles.Add(this);
        }
    }

    private void OnDisable()
    {
        if (type == NozzleType.BACKPACK_NOZZLE)
        {
            JetNozzleManager.backpackNozzles.Remove(this);
        }
        else if (type == NozzleType.ROPE_NOZZLE)
        {
            JetNozzleManager.ropeNozzles.Remove(this);
        }
    }
}
