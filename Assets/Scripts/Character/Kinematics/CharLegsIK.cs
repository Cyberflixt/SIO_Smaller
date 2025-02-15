
using System;
using UnityEngine;

public class CharLegsIK : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform bonePelvis;
    [SerializeField] private ChainIK IK_leftLeg;
    [SerializeField] private ChainIK IK_rightLeg;

    [Header("Settings")]
    [SerializeField] private LayerMask environmentLayer;
    [NonSerialized] private bool displayDebug = true;
    private const float footHeight = .1f; // Shoe height (height between foot bone and ground)

    // Internal
    private float pelvisOffset = 0;
    private string leftFootAnimVariableName = "LeftFootIk";
    private string rightFootAnimVariableName = "RightFootIk";

    private Animator animator;
    private float legLength;
    private void Start(){
        animator = GetComponent<Animator>();

        // Total leg length
        legLength = Vector3.Distance(IK_leftLeg.transform.position, IK_leftLeg.transform.parent.position)
                  + Vector3.Distance(IK_leftLeg.transform.parent.position, IK_leftLeg.transform.parent.parent.position);
    }

    private float idleThreshold = .1f;
    private float idleDelay = .5f;
    private float idleAccu = 0;
    private bool ikEnabled = false;
    private Vector3 oldPosition = Vector3.zero;

    private float rightFootWeight = 0;
    private float leftFootWeight = 0;
    private Vector3 leftFootIkPosition = Vector3.zero;
    private Vector3 rightFootIkPosition = Vector3.zero;
    private Quaternion leftFootIkRotation = Quaternion.identity;
    private Quaternion rightFootIkRotation = Quaternion.identity;
    private Quaternion leftFootIkRotation_target = Quaternion.identity;
    private Quaternion rightFootIkRotation_target = Quaternion.identity;


    private void Awake(){
        IK_leftLeg.enabled = false;
        IK_rightLeg.enabled = false;
    }
    
    private void LateUpdate(){
        // Is idling?
        float speed = (transform.position-oldPosition).magnitude;
        oldPosition = transform.position;

        if (speed < idleThreshold * Time.deltaTime){
            idleAccu += Time.deltaTime;
        } else {
            idleAccu = 0;
        }

        ikEnabled = idleAccu > idleDelay;
        ikEnabled = true;

        float lerp = Mathf.Clamp01(Time.deltaTime * 30);
        
        bool leftHit = GetFootGround(IK_leftLeg.transform, out Vector3 leftFootIkPosition_new, out Quaternion leftFootIkRotation_new);
        bool rightHit = GetFootGround(IK_rightLeg.transform, out Vector3 rightFootIkPosition_new, out Quaternion rightFootIkRotation_new);
        UpdateWeights(leftHit, rightHit);

        leftFootIkRotation_target = leftFootIkRotation_new;
        rightFootIkRotation_target = rightFootIkRotation_new;
        
        // Interpolations
        leftFootIkRotation = Quaternion.Lerp(leftFootIkRotation, leftFootIkRotation_target, lerp);
        rightFootIkRotation = Quaternion.Lerp(rightFootIkRotation, rightFootIkRotation_target, lerp);

        leftFootIkPosition = new Vector3(leftFootIkPosition_new.x, Mathf.Lerp(leftFootIkPosition.y, leftFootIkPosition_new.y, lerp), leftFootIkPosition_new.z);
        rightFootIkPosition = new Vector3(rightFootIkPosition_new.x, Mathf.Lerp(rightFootIkPosition.y, rightFootIkPosition_new.y, lerp), rightFootIkPosition_new.z);

        // Adjust pelvis height
        MovePelvisHeight(leftFootIkPosition, rightFootIkPosition);

        // Set IK targets
        IK_leftLeg.Target.position = leftFootIkPosition;
        IK_leftLeg.Target.rotation = leftFootIkRotation;
        IK_leftLeg.Weight = leftFootWeight;
        IK_leftLeg.ResolveIK_Weight();

        IK_rightLeg.Target.position = rightFootIkPosition;
        IK_rightLeg.Target.rotation = rightFootIkRotation;
        IK_rightLeg.Weight = rightFootWeight;
        IK_rightLeg.ResolveIK_Weight();
    }

    private void UpdateWeights(bool leftEnabled, bool rightEnabled){
        // Weight depends on the animation curve (0 if foot is not touching the ground)
        float leftFootWeight_tar = 0;
        float rightFootWeight_tar = 0;
        if (ikEnabled){
            if (leftEnabled)
                leftFootWeight_tar  = animator.GetFloat(leftFootAnimVariableName);
            if (rightEnabled)
                rightFootWeight_tar = animator.GetFloat(rightFootAnimVariableName);
        }

        float lerp = Mathf.Clamp01(Time.deltaTime * 20);
        leftFootWeight = Mathf.Lerp(leftFootWeight, leftFootWeight_tar, lerp);
        rightFootWeight = Mathf.Lerp(rightFootWeight, rightFootWeight_tar, lerp);
    }

    float lastPelvis = 0;
    private void MovePelvisHeight(Vector3 leftFootIkPosition, Vector3 rightFootIkPosition){
        float offset_left = leftFootIkPosition.y - IK_leftLeg.transform.position.y;
        float offset_right = rightFootIkPosition.y - IK_rightLeg.transform.position.y;
        float offset_lowest = Mathf.Min(offset_left, offset_right);

        float newPelvisOffset = offset_lowest;
        lastPelvis = Mathf.Lerp(lastPelvis, newPelvisOffset, Mathf.Clamp01(Time.deltaTime * 5));
        bonePelvis.position += new Vector3(0, lastPelvis * Mathf.Max(leftFootWeight, rightFootWeight));
    }
    
    private bool GetFootGround(Transform foot, out Vector3 hitPosition, out Quaternion hitRotation){
        // Raycast towards the ground
        float heightUp = .7f;
        float heightDown = .7f;
        float dist = heightUp + heightDown + footHeight;

        Vector3 footPosition = foot.position;
        Vector3 start = footPosition + Vector3.up * heightUp;

        if (displayDebug)
            Debug.DrawLine(start, start + Vector3.down * dist, Color.yellow);
        
        if (Physics.Raycast(start, Vector3.down, out RaycastHit feetOutHit, dist, environmentLayer)){
            // Feet ik from the sky position
            hitPosition = new Vector3(footPosition.x, feetOutHit.point.y + pelvisOffset + footHeight, footPosition.z);

            Quaternion correction = Quaternion.Euler(90, 0, 0);
            hitRotation = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation * correction;
            return true;
        }

        // didn't work
        hitPosition = footPosition;
        hitRotation = foot.rotation;
        return false;
    }
}
