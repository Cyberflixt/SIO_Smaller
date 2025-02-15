using System;
using UnityEngine;

/// <summary>
/// Unity's fabric algorithm (modified)
/// </summary>
public class ChainIK : MonoBehaviour
{
    public int ChainLength = 2;
    public Transform Hint;
    public Transform Target;
    [Range(0, 1)] public float Weight = 1f;


    // Hidden settings
    private const int Iterations = 10;
    private const float Delta = 0.001f; // Stop trying when threshold is reached
    private const float SnapBackStrength = 1f; // Strength of going back to the start position. [0-1]


    protected float[] BonesLength; //Target to Origin
    protected float CompleteLength;
    protected Transform[] Bones;
    protected Vector3[] Positions;
    protected Vector3[] StartDirectionSucc;
    protected Quaternion[] StartRotationBone;
    protected Quaternion StartRotationTarget;
    protected Transform Root;


    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }

    void Init()
    {
        // Initialize array
        Bones = new Transform[ChainLength + 1];
        Positions = new Vector3[ChainLength + 1];
        BonesLength = new float[ChainLength];
        StartDirectionSucc = new Vector3[ChainLength + 1];
        StartRotationBone = new Quaternion[ChainLength + 1];

        // Get root bone
        Root = transform;
        for (int i = 0; i <= ChainLength; i++)
            Root = Root.parent;

        //init target
        if (Target == null)
        {
            Target = new GameObject(gameObject.name + " Target").transform;
            SetPositionRootSpace(Target, GetPositionRootSpace(transform));
        }
        StartRotationTarget = GetRotationRootSpace(Target);


        //init data
        Transform current = transform;
        CompleteLength = 0;
        for (int i = Bones.Length-1; i >= 0; i--)
        {
            Bones[i] = current;
            StartRotationBone[i] = GetRotationRootSpace(current);

            if (i == Bones.Length - 1)
            {
                //leaf
                StartDirectionSucc[i] = GetPositionRootSpace(Target) - GetPositionRootSpace(current);
            }
            else
            {
                //mid bone
                StartDirectionSucc[i] = GetPositionRootSpace(Bones[i + 1]) - GetPositionRootSpace(current);
                BonesLength[i] = StartDirectionSucc[i].magnitude;
                CompleteLength += BonesLength[i];
            }

            current = current.parent;
        }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        ResolveIK_Weight();
    }
    public void ResolveIK_Weight(){
        // Save rotations
        Quaternion[] rotations = new Quaternion[ChainLength+1];
        Transform current = transform;
        for (int i=0; i<=ChainLength; i++){
            rotations[i] = current.localRotation;
            current = current.parent;
        }

        ResolveIK();

        // Interpolation
        current = transform;
        for (int i=0; i<=ChainLength; i++){
            current.localRotation = Quaternion.Lerp(rotations[i], current.localRotation, Weight);
            current = current.parent;
        }
    }

    public void ResolveIK()
    {
        // Unity kys
        if (Target == null)
            return;

        if (BonesLength.Length != ChainLength)
            Init();

        // Fabric

        // root
        // (bone0) (bonelen 0) (bone1) (bonelen 1) (bone2)...
        // x--------------------x--------------------x---...

        //get position
        for (int i = 0; i < Bones.Length; i++)
            Positions[i] = GetPositionRootSpace(Bones[i]);

        Vector3 targetPosition = GetPositionRootSpace(Target);
        Quaternion targetRotation = GetRotationRootSpace(Target);

        // 1st is possible to reach?
        if ((targetPosition - GetPositionRootSpace(Bones[0])).sqrMagnitude >= CompleteLength * CompleteLength)
        {
            // Just strech it
            Vector3 direction = (targetPosition - Positions[0]).normalized;
            // Set everything after root
            for (int i = 1; i < Positions.Length; i++)
                Positions[i] = Positions[i - 1] + direction * BonesLength[i - 1];
        }
        else
        {
            for (int i = 0; i < Positions.Length - 1; i++)
                Positions[i + 1] = Vector3.Lerp(Positions[i + 1], Positions[i] + StartDirectionSucc[i], SnapBackStrength);

            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                // https://www.youtube.com/watch?v=UNoX65PRehA
                // Back
                for (int i = Positions.Length - 1; i > 0; i--)
                {
                    if (i == Positions.Length - 1)
                        Positions[i] = targetPosition; //set it to target
                    else
                        Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * BonesLength[i]; //set in line on distance
                }

                // Forward
                for (int i = 1; i < Positions.Length; i++)
                    Positions[i] = Positions[i - 1] + (Positions[i] - Positions[i - 1]).normalized * BonesLength[i - 1];

                // Close enough?
                if ((Positions[Positions.Length - 1] - targetPosition).sqrMagnitude < Delta * Delta)
                    break;
            }
        }

        // Move towards pole
        if (Hint != null)
        {
            Vector3 polePosition = GetPositionRootSpace(Hint);
            for (int i = 1; i < Positions.Length - 1; i++)
            {
                Plane plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);
                Vector3 projectedPole = plane.ClosestPointOnPlane(polePosition);
                Vector3 projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                float angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1], plane.normal);
                Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) + Positions[i - 1];
            }
        }

        //set position & rotation
        for (int i = 0; i < Positions.Length; i++)
        {
            if (i == Positions.Length - 1)
                SetRotationRootSpace(Bones[i], Quaternion.Inverse(targetRotation) * StartRotationTarget * Quaternion.Inverse(StartRotationBone[i]));
            else
                SetRotationRootSpace(Bones[i], Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i + 1] - Positions[i]) * Quaternion.Inverse(StartRotationBone[i]));
            SetPositionRootSpace(Bones[i], Positions[i]);
        }

        transform.rotation = Target.rotation;
    }

    private Vector3 GetPositionRootSpace(Transform current)
    {
        if (Root == null)
            return current.position;
        else
            return Quaternion.Inverse(Root.rotation) * (current.position - Root.position);
    }

    private void SetPositionRootSpace(Transform current, Vector3 position)
    {
        if (Root == null)
            current.position = position;
        else
            current.position = Root.rotation * position + Root.position;
    }

    private Quaternion GetRotationRootSpace(Transform current)
    {
        //inverse(after) * before => rot: before -> after
        if (Root == null)
            return current.rotation;
        else
            return Quaternion.Inverse(current.rotation) * Root.rotation;
    }

    private void SetRotationRootSpace(Transform current, Quaternion rotation)
    {
        if (Root == null)
            current.rotation = rotation;
        else
            current.rotation = Root.rotation * rotation;
    }
}
