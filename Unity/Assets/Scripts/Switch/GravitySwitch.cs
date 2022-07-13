using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AnimationManager))]
public class GravitySwitch : MonoBehaviour
{
    public bool IsActivating => isActivating;

    private readonly int LEVER_ACTION = Animator.StringToHash("Lever_Action");

    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private bool isLeverUp = true;

    private AnimationManager animationManager;

    private bool isActivating;

    private void Awake()
    {
        animationManager = GetComponent<AnimationManager>();
    }

    private void OnGravitySwitch(bool changeGravity)
    {
        float target = isLeverUp ? 1f : 0f;
        isLeverUp = !isLeverUp;

        isActivating = true;
        StartCoroutine(SwitchAnimationCoroutine(target));
    }

    private IEnumerator SwitchAnimationCoroutine(float target)
    {
        float progress = 0f;
        while (progress < 1f)
        {
            progress = Mathf.Min(1f, progress + animationSpeed * Time.deltaTime);
            animationManager.ControlAnimation(LEVER_ACTION, Mathf.Abs(1f - (progress + target)));
            yield return null;
        }

        animationManager.ControlAnimation(LEVER_ACTION, target);
        isActivating = false;
    }

    private void OnEnable()
    {
        GameEvents.GravitySwitchEvent += OnGravitySwitch;
    }

    private void OnDisable()
    {
        GameEvents.GravitySwitchEvent -= OnGravitySwitch;
    }
}
