using UnityEngine;

[RequireComponent(typeof(AnimationManager))]
public class ShipAI : MonoBehaviour, IPersistentObject
{
    public static int currentStoryDialogID = 1;

    [SerializeField, Range(0f, 2f)] private float animationSpeed = 1f;

    [SerializeField] private Transform target;
    [SerializeField] private Transform eye;

    [SerializeField] private float rotationLimit = 50f;
    [SerializeField, Range(0.1f, 10f)] private float smoothFactor = 2.5f;

    private readonly int
        AI_DEPLOY = Animator.StringToHash("AI_Deploy");

    private AnimationManager animationManager;

    private bool isDistanceSet;
    private Vector2 distanceFromBound;

    private float originalAngle;
    private Vector3 targetRotation;

    private void Awake()
    {
        animationManager = GetComponent<AnimationManager>();

        if (eye != null)
        {
            originalAngle = eye.transform.rotation.eulerAngles.z;
        }
    }

    private void Update()
    {
        LerpToTarget();
    }

    private void LateUpdate()
    {
        if (eye == null || target == null)
        {
            return;
        }

        Quaternion rotation = eye.transform.rotation;
        eye.transform.rotation = Quaternion.Slerp(rotation, Quaternion.Euler(targetRotation), smoothFactor * Time.deltaTime);
    }

    private void LerpToTarget()
    {
        if (eye == null || target == null)
        {
            return;
        }

        Vector2 vec = target.transform.position - eye.transform.position;
        float angle = Mathf.Clamp(Mathf.Atan2(vec.y, -vec.x) * Mathf.Rad2Deg, -rotationLimit, rotationLimit);

        Vector3 currentRotation = eye.transform.rotation.eulerAngles;
        targetRotation = new Vector3(currentRotation.x, currentRotation.y, originalAngle + angle);
    }

    private void OnLevelChange(GameLevel currentLevel)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllCoroutines();
            AudioManager.Instance.StopCurrentVoice();
        }

        if (GameLevel.IsOutside)
        {
            return;
        }

        Bounds bounds = currentLevel.Bounds;
        //Debug.Log(currentLevel.Order + ": " + currentLevel.Bounds.max);

        if (!isDistanceSet)
        {
            distanceFromBound.x = bounds.max.x - transform.position.x;
            distanceFromBound.y = bounds.min.y - transform.position.y;
            isDistanceSet = true;
        }
        else
        {
            transform.position = new Vector3(bounds.max.x - distanceFromBound.x, bounds.min.y - distanceFromBound.y, transform.position.z);
            animationManager.ControlAnimation(AI_DEPLOY, 0f);
        }

        animationManager.ResumeAnimation();
        animationManager.ChangeAnimation(AI_DEPLOY, animationSpeed);
    }


    private void OnCinematicAIDialog(int dialogId)
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        if (dialogId == 1)
        {
            AudioManager.Instance.Play("Story_1");
            AudioManager.Instance.WaitForAudio(new string[] { "Story_2", "Story_3", "Story_4" }, 0.25f);
            currentStoryDialogID += 4;
        }

        if (dialogId == 5)
        {
            AudioManager.Instance.Play("Story_5");
            currentStoryDialogID += 1;
        }

        if (dialogId == 6)
        {
            AudioManager.Instance.Play("Story_6");
            currentStoryDialogID += 1;
        }

        if (dialogId == 7)
        {
            AudioManager.Instance.Play("Story_7");
            currentStoryDialogID += 1;
        }

        if (dialogId == 8)
        {
            AudioManager.Instance.Play("Story_8");
            AudioManager.Instance.WaitForAudio(new string[] { "Story_9", "Story_10" }, 0.25f, () => GameManager.Instance.EndGame(5f));
            currentStoryDialogID += 3;
        }
    }

    private void OnEnable()
    {
        SerializationManager.AddPersistentObject(this);

        GameEvents.LevelChangeEvent += OnLevelChange;
        GameEvents.CinematicAIDialogEvent += OnCinematicAIDialog;
    }

    private void OnDisable()
    {
        SerializationManager.RemovePersistentObject(this);

        GameEvents.LevelChangeEvent -= OnLevelChange;
        GameEvents.CinematicAIDialogEvent -= OnCinematicAIDialog;
    }

    public void SaveData(GameData data)
    {
        data.isAIDistanceSet = isDistanceSet;
        data.AIDistanceFromBound = distanceFromBound;
    }

    public void LoadData(GameData data)
    {
        distanceFromBound = data.AIDistanceFromBound;
        isDistanceSet = data.isAIDistanceSet;
    }
}
