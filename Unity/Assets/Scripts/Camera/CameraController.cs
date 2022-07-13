using UnityEngine;
using Unity.Mathematics;

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Bounds currentBounds;

    [SerializeField] private Transform followTarget = default;
    [SerializeField] private bool3 freezeFollow = default;

    [SerializeField] private Vector3 targetOffset, zoomMaxClamp, zoomMinClamp;
    [SerializeField, Range(0.1f, 10f)] private float cameraSmoothFactor = 2.5f;

    private Camera cameraComponent;
    private Shakeable shakeableComponent;

    private Vector3 nextPosition;

    private void Start()
    {
        cameraComponent = GetComponent<Camera>();
        shakeableComponent = GetComponent<Shakeable>();
    }

    private void Update()
    {
        LerpToTarget();

        if ((transform.position - nextPosition).sqrMagnitude < 0.0001f)
        {
            return;
        }

        //Debug.Log("Camera Next Position: (" + nextPosition.x + ", " + nextPosition.y + ", " + nextPosition.z + ")");

        transform.position = Vector3.Lerp(transform.position, nextPosition, cameraSmoothFactor * Time.deltaTime);
        //if (shakeableComponent != null)
        //{
        //    shakeableComponent.UpdatePosition(transform.position);
        //}
    }

    public void LerpToTarget()
    {
        Vector2 min = cameraComponent.ViewportToWorldPoint(new Vector3(0f, 0f, -cameraComponent.transform.position.z - currentBounds.center.z));
        Vector2 max = cameraComponent.ViewportToWorldPoint(new Vector3(1f, 1f, -cameraComponent.transform.position.z - currentBounds.center.z));

        nextPosition.x = freezeFollow.x ? transform.position.x : Mathf.Clamp(
            followTarget.position.x + targetOffset.x,
            (Mathf.Round(max.x) + currentBounds.min.x) / 2f,
            (currentBounds.max.x + Mathf.Round(min.x)) / 2f
        );

        nextPosition.y = freezeFollow.y ? transform.position.y : Mathf.Clamp(
            followTarget.position.y + targetOffset.y,
            (Mathf.Round(max.y) + currentBounds.min.y) / 2f,
            (currentBounds.max.y + Mathf.Round(min.y)) / 2f
        );

        nextPosition.z = freezeFollow.z ? transform.position.z : Mathf.Clamp(
            followTarget.position.z + targetOffset.z,
            currentBounds.min.z,
            currentBounds.max.z
        );
    }

    private void OnLevelChange(GameLevel gameLevel)
    {
        //Debug.Log("New Level Bounds: (" + gameLevel.Bounds.min.x + ", " + gameLevel.Bounds.max.x + ")");

        currentBounds = gameLevel.Bounds;
    }

    private void OnEnable()
    {
        GameEvents.LevelChangeEvent += OnLevelChange;
    }

    private void OnDisable()
    {
        GameEvents.LevelChangeEvent -= OnLevelChange;
    }
}
