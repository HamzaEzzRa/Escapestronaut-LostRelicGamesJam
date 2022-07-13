using UnityEngine;

public class ConfigurableCollisionJoint : CollisionJoint<ConfigurableJoint>
{
    [SerializeField] private Vector3 axis = Vector3.right;
    [SerializeField] private bool autoConfigureConnectedAnchor = true;
    //[SerializeField] private Vector3 connectedAnchor;

    [SerializeField] private Vector3 secondaryAxis = Vector3.up;
    [SerializeField] private ConfigurableJointMotion xMotion, yMotion, zMotion;
    [SerializeField] private ConfigurableJointMotion angularXMotion, angularYMotion, angularZMotion;

    [SerializeField] private float spring, damper;

    [SerializeField] private float limit, bounciness, contactDistance;

    [SerializeField] private float xPositionSpring, xPositionDamper;
    [SerializeField] private float yPositionSpring, yPositionDamper;
    [SerializeField] private float zPositionSpring, zPositionDamper;

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (!continueCollision || joint == null)
        {
            return;
        }

        joint.axis = axis;
        joint.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
        //joint.connectedAnchor = connectedAnchor;
        joint.secondaryAxis = secondaryAxis;

        joint.xMotion = xMotion;
        joint.yMotion = yMotion;
        joint.zMotion = zMotion;

        joint.angularXMotion = angularXMotion;
        joint.angularYMotion = angularYMotion;
        joint.angularZMotion = angularZMotion;

        joint.linearLimitSpring = new SoftJointLimitSpring
        {
            spring = spring,
            damper = damper,
        };

        joint.linearLimit = new SoftJointLimit
        {
            limit = limit,
            bounciness = bounciness,
            contactDistance = contactDistance,
        };

        joint.xDrive = new JointDrive
        {
            positionSpring = xPositionSpring,
            positionDamper = xPositionDamper,
        };

        joint.yDrive = new JointDrive
        {
            positionSpring = yPositionSpring,
            positionDamper = yPositionDamper,
        };

        joint.zDrive = new JointDrive
        {
            positionSpring = zPositionSpring,
            positionDamper = zPositionDamper,
        };
    }
}
