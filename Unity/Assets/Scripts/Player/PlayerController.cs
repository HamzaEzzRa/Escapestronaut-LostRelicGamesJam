using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour, IPersistentObject
{
    public float BackpackBoost => backpackBoost * backpackBoostMultiplier;
    public bool IsControllerActive => isControllerActive;

    [SerializeField] private Rigidbody root = default;
    [SerializeField] private Rigidbody body = default;
    [SerializeField] private Transform head = default;
    [SerializeField] private Transform leftRope = default;
    [SerializeField] private Transform rightRope = default;
    [SerializeField, Range(0f, 100f)] private float backpackBoost = 50f;
    [SerializeField, Range(0f, 10f)] private float ropeBoost = 5f;
    [SerializeField] private Vector2 headRotationLimit = new Vector2(90f, 90f);

    [SerializeField, Range(0f, 20f)] private float maxSpeed = 5f;

    private PlayerInputActions playerInputActions;

    private InputAction boost, look, leftHook, rightHook;
    private bool lmbDown, rmbDown;

    private Rigidbody leftRopeRb, rightRopeRb;
    private Camera mainCamera;

    private RopeRoot leftRopeRoot, rightRopeRoot;
    private ConfigurableJoint[] leftRopeJoints, rightRopeJoints;
    private Vector2[] leftRopeOriginalPositions, rightRopeOriginalPositions;
    private SoftJointLimit[] leftRopeOriginalLinearLimits, rightRopeOriginalLinearLimits;
    private int[] leftRopeDescrIdList, rightRopeDescrIdList;

    private float backpackBoostMultiplier = 1f;

    private bool isControllerActive;

    private void Awake()
    {
        mainCamera = Camera.main;
        isControllerActive = true;
        playerInputActions = new PlayerInputActions();

        SetupRopes();
    }

    private void Update()
    {
        if (body.velocity.sqrMagnitude >= maxSpeed * maxSpeed)
        {
            body.velocity = Vector3.ClampMagnitude(body.velocity, maxSpeed);
        }
    }

    private void FixedUpdate()
    {
        Boost();
        LeftHook();
        RightHook();
    }

    private void SetupRopes()
    {
        if (leftRope != null)
        {
            leftRopeRb = leftRope.GetComponent<Rigidbody>();
        }

        if (rightRope != null)
        {
            rightRopeRb = rightRope.GetComponent<Rigidbody>();
        }

        RopeRoot[] ropeRoots = GetComponentsInChildren<RopeRoot>();
        foreach (RopeRoot root in ropeRoots)
        {
            if (root.Direction == RopeRoot.RopeDirection.LEFT)
            {
                leftRopeRoot = root;
                RopeSetupHelper(root, ref leftRopeJoints, ref leftRopeOriginalPositions, ref leftRopeOriginalLinearLimits, ref leftRopeDescrIdList);
            }
            else if (root.Direction == RopeRoot.RopeDirection.RIGHT)
            {
                rightRopeRoot = root;
                RopeSetupHelper(root, ref rightRopeJoints, ref rightRopeOriginalPositions, ref rightRopeOriginalLinearLimits, ref rightRopeDescrIdList);
            }
        }
    }

    private void RopeSetupHelper(RopeRoot root, ref ConfigurableJoint[] ropeJoints, ref Vector2[] ropeOriginalPositions, ref SoftJointLimit[] ropeOriginalLinearLimits, ref int[] ropeDescrIdList)
    {
        ropeJoints = root.transform.GetComponentsInChildren<ConfigurableJoint>();
        ropeOriginalPositions = new Vector2[ropeJoints.Length];
        ropeOriginalLinearLimits = new SoftJointLimit[ropeJoints.Length];
        ropeDescrIdList = new int[ropeJoints.Length];

        for (int i = 0; i < ropeJoints.Length; i++)
        {
            ropeOriginalPositions[i] = ropeJoints[i].transform.position;
            ropeOriginalLinearLimits[i] = ropeJoints[i].linearLimit;
            ropeJoints[i].linearLimit = new SoftJointLimit()
            {
                limit = 0f,
            };
        }
    }

    private void Boost()
    {
        if (!IsControllerActive || body == null)
        {
            return;
        }

        Vector2 input = boost.ReadValue<Vector2>();
        JetNozzleManager.BoostEffect(input);

        if (input.sqrMagnitude != 0)
        {
            body.AddForce(input.normalized * BackpackBoost, ForceMode.Force);
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.NextSequence(1);
            }
        }
    }

    private void Look(InputAction.CallbackContext context)
    {
        if (!IsControllerActive || head == null)
        {
            return;
        }

        Vector2 target = context.ReadValue<Vector2>();
        if (target.sqrMagnitude > 2f)
        {
            // Mouse Position
            target = mainCamera.ScreenToViewportPoint(target);
            target -= (Vector2)mainCamera.WorldToViewportPoint(head.position);
            target.x = Mathf.Clamp(target.x, -0.5f, 0.5f);
            target.y = Mathf.Clamp(target.y, -0.5f, 0.5f);
        }
        else
        {
            // Mouse or Controller
            target /= 2f;
        }
        //Debug.Log("(" + target.x + ", " + target.y + ")");

        float xAngle = -Mathf.Clamp(Mathf.Atan2(target.y, Mathf.Max(0.175f, Mathf.Abs(target.x))) * Mathf.Rad2Deg, -headRotationLimit.x, headRotationLimit.x);
        float yAngle = Mathf.Clamp(2 * target.x * -headRotationLimit.y, -headRotationLimit.y, headRotationLimit.y);

        head.localRotation = Quaternion.Euler(xAngle, yAngle, 0f);
    }

    private void LeftHook()
    {
        if (!IsControllerActive || !lmbDown)
        {
            return;
        }

        HookHelper(leftRopeRb, JetNozzle.NozzleDirection.EAST, leftRopeRoot.IsAttached);
    }

    private void RightHook()
    {
        if (!IsControllerActive || !rmbDown)
        {
            return;
        }

        HookHelper(rightRopeRb, JetNozzle.NozzleDirection.WEST, rightRopeRoot.IsAttached);
    }

    private void HookHelper(Rigidbody ropeRb, JetNozzle.NozzleDirection direction, bool isAttached)
    {
        //if (!isAttached)
        //{
            Vector3 target = look.ReadValue<Vector2>();
            //target.z = -mainCamera.transform.position.z - 2f;
            if (target.sqrMagnitude > 2f)
            {
                target = (mainCamera.ScreenToViewportPoint(target) - mainCamera.WorldToViewportPoint(body.transform.position));
                target.z = 0f;
            }
            target /= 2f;
            //Debug.Log("(" + target.x + ", " + target.y + ")");

            
            if (ropeRb != null)
            {
                ropeRb.AddForce(target.normalized * ropeBoost, ForceMode.Force);
                JetNozzleManager.HookEffect(direction);
            }

            if (target.sqrMagnitude > 0f && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.NextSequence(2);
            }
        //}
    }

    private void LMBStart(InputAction.CallbackContext context)
    {
        //if (!leftRopeRoot.IsAttached)
        //{
            leftRopeRoot.IsDeployed = true;
            ResetLinearLimit(leftRopeJoints, ref leftRopeDescrIdList, leftRopeOriginalLinearLimits);
            lmbDown = true;
        //}
        //else
        //{
        //    GameEvents.JointDetachInvoke(leftRopeRoot.GetHashCode());
        //    ResetHookHelper(leftRopeRoot, leftRopeJoints, leftRopeOriginalPositions, ref leftRopeDescrList);
        //    lmbDown = false;
        //}
    }

    private void LMBRelease(InputAction.CallbackContext context)
    {
        if (!lmbDown)
        {
            return;
        }

        if (leftRopeRoot.IsAttached)
        {
            leftRopeRoot.IsAttached = false;
            GameEvents.JointDetachInvoke(leftRopeRoot.GetHashCode());
        }

        ResetHookHelper(leftRopeRoot, leftRopeJoints, leftRopeOriginalPositions, ref leftRopeDescrIdList, JetNozzle.NozzleDirection.EAST);
        lmbDown = false;
    }

    private void RMBStart(InputAction.CallbackContext context)
    {
        //if (!rightRopeRoot.IsAttached)
        //{
            rightRopeRoot.IsDeployed = true;
            ResetLinearLimit(rightRopeJoints, ref rightRopeDescrIdList, rightRopeOriginalLinearLimits);
            rmbDown = true;
        //}
        //else
        //{
        //    GameEvents.JointDetachInvoke(rightRopeRoot.GetHashCode());
        //    ResetHookHelper(rightRopeRoot, rightRopeJoints, rightRopeOriginalPositions, ref rightRopeDescrList);
        //    rmbDown = false;
        //}
    }

    private void RMBRelease(InputAction.CallbackContext context)
    {
        if (!rmbDown)
        {
            return;
        }

        if (rightRopeRoot.IsAttached)
        {
            rightRopeRoot.IsAttached = false;
            GameEvents.JointDetachInvoke(rightRopeRoot.GetHashCode());
        }

        ResetHookHelper(rightRopeRoot, rightRopeJoints, rightRopeOriginalPositions, ref rightRopeDescrIdList, JetNozzle.NozzleDirection.WEST);
        rmbDown = false;
    }

    private void ResetHookHelper(RopeRoot ropeRoot, ConfigurableJoint[] ropeJoints, Vector2[] ropeOriginalPositions, ref int[] ropeDescrList, JetNozzle.NozzleDirection direction)
    {
        ropeRoot.IsDeployed = false;
        if (!ropeRoot.IsAttached)
        {
            for (int i = 0; i < ropeJoints.Length; i++)
            {
                ConfigurableJoint joint = ropeJoints[i];

                float currentLimit = joint.linearLimit.limit;
                float distanceToOrigin = Vector2.Distance(joint.transform.position, ropeOriginalPositions[i]);

                ropeDescrList[i] = LeanTween.value(1f, 0f, Mathf.Clamp(3f * distanceToOrigin, 3f, 5f))
                    .setOnUpdate((float value) =>
                    {
                        if (joint != null)
                        {
                            joint.linearLimit = new SoftJointLimit()
                            {
                                limit = currentLimit * value,
                            };
                        }
                    }).uniqueId;
            }

            JetNozzleManager.ReleaseHookEffect(direction);
        }
    }

    private void ResetLinearLimit(ConfigurableJoint[] ropeJoints, ref int[] ropeDescrList, SoftJointLimit[] ropeOriginalLinearLimits)
    {
        for (int i = 0; i < ropeJoints.Length; i++)
        {
            if (ropeDescrList[i] != -1)
            {
                LeanTween.cancel(ropeDescrList[i]);
                ropeDescrList[i] = -1;
            }
            ropeJoints[i].linearLimit = ropeOriginalLinearLimits[i];
        }
    }

    private void OnRopeAttached(RopeRoot.RopeDirection direction)
    {
        if (direction == leftRopeRoot.Direction)
        {
            ResetLinearLimit(leftRopeJoints, ref leftRopeDescrIdList, leftRopeOriginalLinearLimits);
            leftRopeRoot.IsAttached = true;
        }
        else if (direction == rightRopeRoot.Direction)
        {
            ResetLinearLimit(rightRopeJoints, ref rightRopeDescrIdList, rightRopeOriginalLinearLimits);
            rightRopeRoot.IsAttached = true;
        }
    }

    private void OnLevelChange(GameLevel currentLevel)
    {
        if (body == null)
        {
            return;
        }

        if (GameLevel.IsGravityOn)
        {
            if (GameLevel.IsOutside)
            {
                body.useGravity = false;
                backpackBoostMultiplier = 1f;
            }
            else
            {
                body.useGravity = true;
                backpackBoostMultiplier = 1.5f;
            }
        }
    }

    private void OnGravitySwitch(bool changeGravity)
    {
        if (body == null || GameLevel.IsOutside)
        {
            return;
        }

        if (changeGravity)
        {
            body.useGravity = !body.useGravity;
        }
        else
        {
            body.useGravity = GameLevel.IsGravityOn;
        }
        backpackBoostMultiplier = body.useGravity ? 1.5f : 1f;
    }

    private void OnPauseMenuToggle(bool value)
    {
        if (value)
        {
            playerInputActions.Player.Disable();
        }
        else
        {
            playerInputActions.Player.Enable();
        }
    }

    private void OnDebugConsoleToggle(bool value)
    {
        if (value)
        {
            playerInputActions.Player.Disable();
        }
        else if (!PauseMenu.Instance.IsPaused)
        {
            playerInputActions.Player.Enable();
        }
    }

    //private void OnCinematicAIDialog()
    //{
    //    isControllerActive = false;
    //    if (body != null)
    //    {
    //        body.velocity = Vector3.zero;
    //    }
    //}

    private void OnEnable()
    {
        SerializationManager.AddPersistentObject(this);

        boost = playerInputActions.Player.Boost;
        boost.Enable();

        look = playerInputActions.Player.Look;
        look.performed += Look;
        look.Enable();

        leftHook = playerInputActions.Player.LeftHook;
        leftHook.performed += LMBStart;
        leftHook.canceled += LMBRelease;
        leftHook.Enable();

        rightHook = playerInputActions.Player.RightHook;
        rightHook.performed += RMBStart;
        rightHook.canceled += RMBRelease;
        rightHook.Enable();

        GameEvents.LevelChangeEvent += OnLevelChange;
        GameEvents.RopeAttachedEvent += OnRopeAttached;
        GameEvents.GravitySwitchEvent += OnGravitySwitch;
        GameEvents.PauseMenuToggleEvent += OnPauseMenuToggle;
        GameEvents.DebugConsoleToggleEvent += OnDebugConsoleToggle;
        //GameEvents.CinematicAIDialogEvent += OnCinematicAIDialog;
    }

    private void OnDisable()
    {
        SerializationManager.RemovePersistentObject(this);

        boost.Disable();

        look.performed -= Look;
        look.Disable();

        leftHook.performed -= LMBStart;
        leftHook.canceled -= LMBRelease;
        leftHook.Disable();

        rightHook.performed -= RMBStart;
        rightHook.canceled -= RMBRelease;
        rightHook.Disable();

        GameEvents.LevelChangeEvent -= OnLevelChange;
        GameEvents.RopeAttachedEvent -= OnRopeAttached;
        GameEvents.GravitySwitchEvent -= OnGravitySwitch;
        GameEvents.PauseMenuToggleEvent -= OnPauseMenuToggle;
        GameEvents.DebugConsoleToggleEvent -= OnDebugConsoleToggle;
        //GameEvents.CinematicAIDialogEvent -= OnCinematicAIDialog;
    }

    public void SaveData(GameData data)
    {
        if (root != null)
        {
            data.playerPosition = root.transform.position + GameLevel.PlayerOffset;
        }
    }

    public void LoadData(GameData data)
    {
        if (root != null)
        {
            StartCoroutine(RootTeleportCoroutine(data.playerPosition));
        }
    }

    private IEnumerator RootTeleportCoroutine(Vector3 target)
    {
        Rigidbody[] rigidbodies = root.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
        root.transform.position = target;

        yield return null;
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = false;
        }
    }
}
