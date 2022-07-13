using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Portal : MonoBehaviour
{
    [SerializeField] private LayerMask triggerLayer = 1 << 10;

    [SerializeField] private Color offColor, onColor;
    [SerializeField] private bool isPowered = true;

    [SerializeField] private GameDoor targetDoor = default;

    private MeshRenderer meshRenderer;
    private GameLevel parentLevel;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        parentLevel = GetComponentInParent<GameLevel>();

        UpdateColor();
    }

    private void UpdateColor()
    {
        if (isPowered)
        {
            meshRenderer.materials[1].SetColor("_EmissionColor", onColor);
        }
        else
        {
            meshRenderer.materials[1].SetColor("_EmissionColor", offColor);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (targetDoor == null || ((1 << other.gameObject.layer) & triggerLayer) == 0)
        {
            if (targetDoor == null)
            {
                Debug.LogWarning("No GameDoor specified as portal target!");
            }

            return;
        }

        isPowered = true;

        UpdateColor();

        if (AudioManager.Instance != null)
        {
            if (parentLevel != null && parentLevel.IsActive)
            {
                AudioManager.Instance.StopAllCoroutines();
                AudioManager.Instance.StopCurrentVoice();

                AudioManager.Instance.Play("Portal On");
                AudioManager.Instance.Play("CommentGood_" + ((int)Random.Range(1f, 2.99f)).ToString());
            }
        }

        GameEvents.PortalTriggerEnterInvoke(targetDoor.Id);
    }

    private void OnTriggerExit(Collider other)
    {
        if (targetDoor == null || ((1 << other.gameObject.layer) & triggerLayer) == 0)
        {
            if (targetDoor == null)
            {
                Debug.LogWarning("No GameDoor specified as portal target!");
            }

            return;
        }

        isPowered = false;

        UpdateColor();

        if (AudioManager.Instance != null)
        {
            if (parentLevel != null && parentLevel.IsActive)
            {
                AudioManager.Instance.StopAllCoroutines();
                AudioManager.Instance.StopCurrentVoice();

                AudioManager.Instance.Play("Portal Off");
                AudioManager.Instance.Play("CommentBad_1");
            }
        }
        
        GameEvents.PortalTriggerExitInvoke(targetDoor.Id);
    }
}
