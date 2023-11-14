using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.ROSTCPConnector;

/// <summary>
///     This script allows robot joints to be controlled with 
///
///     SetJointTarget (use coroutine to move to target positions)
///     SetJointTargetStep (move to the target positions in one delta time)
///     SetJointSpeedStep (move at the target velocity in one delta time)
///     SetJointTrajectory (use coroutine to move along a trajectory).
/// </summary>
public class ArticulationJointController : MonoBehaviour
{
    [SerializeField] private ROSPublisher publisher;
    [SerializeField] private GameObject spatialAnchor, cube;

    // Control parameters
    [SerializeField] private float jointMaxSpeed = 1.0f; 
    
    private Vector3 positionBaseLinkWorld;
    private Quaternion rotationBaseLinkWorld, rotationBaseLinkASA;

    // Articulation Bodies Presets
    // When joint position is set to be IGNORE_VAL, don't change it
    public static float IGNORE_VAL = -100f;
    
    // Articulation Bodies
    [SerializeField] private ArticulationBody robotRoot;
    [SerializeField] private ArticulationBody[] articulationChain;
    private Collider[] colliders;

    // Coroutine for joint movement (move to target positions)
    private Coroutine currCoroutine;
    // trajectory control
    private List<float[]> targetPositions;
    private int currTrajectoryIndex = 0;

    private float[] targetArray = {0.38f, 1.42f, 2.69f, -1.64f, 1.95f, 1.18f, 2.17f};

    void Awake()
    {
        // Get colliders of all articulation bodies
        // Only consider those that are active by default
        colliders = articulationChain[0].GetComponentsInChildren<Collider>();
        colliders = colliders.Where(collider => collider.enabled == true).ToArray();
    }

    void Start(){
        ROSConnection.GetOrCreateInstance().RegisterPublisher<QuaternionMsg>("/unity_anchor_rotation");
        ROSConnection.GetOrCreateInstance().Subscribe<PoseMsg>("/my_gen3/tf_baselink", KinovaBaseLinkCallback);
        ROSConnection.GetOrCreateInstance().Subscribe<Float32MultiArrayMsg>("/my_gen3/joints", TargetJointsCallback);
        cube.GetComponent<Renderer>().enabled = false;
    }


    void Update(){
        
        publisher.RotationMessage("/unity_anchor_rotation", spatialAnchor);
        
        cube.transform.position = positionBaseLinkWorld;
        cube.transform.rotation = rotationBaseLinkWorld;
        robotRoot.TeleportRoot(cube.transform.position, cube.transform.rotation);

        for (int i = 0; i < articulationChain.Length; ++i)
        {
            ArticulationBody joint = articulationChain[i];
            ArticulationDrive drive = joint.xDrive;

            float target = targetArray[i] * Mathf.Rad2Deg;
            if (joint.twistLock == ArticulationDofLock.LimitedMotion)
            {
                target = Mathf.Clamp(target, drive.lowerLimit, drive.upperLimit);
            }

            drive.target = target;
            joint.xDrive = drive;
        }
    }

    void KinovaBaseLinkCallback(PoseMsg baseLinkPose){
        Vector3 vectorBaseLinkASA = new Vector3((float)baseLinkPose.position.x, (float)baseLinkPose.position.y, -(float)baseLinkPose.position.z);
        
        positionBaseLinkWorld = spatialAnchor.transform.TransformDirection(vectorBaseLinkASA);
        positionBaseLinkWorld += spatialAnchor.transform.position;
        
        // publisher.StringMessage("/debug", "World");
        // publisher.StringMessage("/debug", spatialAnchor.transform.rotation.ToString());
                
        rotationBaseLinkWorld = new Quaternion((float)baseLinkPose.orientation.x, (float)baseLinkPose.orientation.y, (float)baseLinkPose.orientation.z, (float)baseLinkPose.orientation.w);

        // Quaternion worldRotation = spatialAnchor.transform.rotation;

        // rotationBaseLinkWorld = worldRotation * localRotation;
        // publisher.StringMessage("/debug", "Product1");
        // publisher.StringMessage("/debug", rotationBaseLinkWorld.ToString());
    }

    void TargetJointsCallback(Float32MultiArrayMsg jointsArray){
        targetArray = jointsArray.data;
    }

    private bool CheckJointTargetStep(float[] positions)
    {
        // Check if current joint targets are set to the target positions
        float[] currTargets = GetCurrentJointTargets();
        for (int i = 0; i < positions.Length; ++i)
        {
            if ((positions[i] != IGNORE_VAL) && 
                (Mathf.Abs(currTargets[i] - positions[i]) > 0.0001))
            {
                return false;
            }
        }
        return true;
    }

    // Set joint target step
    // Directly setting target positions
    // Only recommended for real-time servoing / velocity control 
    // in which the angles change is certainly small
    public void SetJointTargetsStep(float[] targets)
    {
        for (int i = 0; i < articulationChain.Length; ++i)
        {
            if (targets[i] == IGNORE_VAL)
            {
                continue;
            }

            ArticulationBodyUtils.SetJointTargetStep(
            articulationChain[i], targets[i] * Mathf.Rad2Deg, jointMaxSpeed * Mathf.Rad2Deg
        );
        }
    }

    private bool SetJointTrajectoryStepAndCheck()
    {
        if (targetPositions.Count == 0)
        {
            return true;
        }
        
        // Set joint targets of the next frame in the trajectory
        SetJointTargetsStep(targetPositions[currTrajectoryIndex++]);
        // Check if done
        return currTrajectoryIndex == targetPositions.Count;
    }

    // Stop joints
    public void StopJoints()
    {
        foreach (ArticulationBody joint in articulationChain)
        {
            ArticulationBodyUtils.StopJoint(joint);
        }
    }

    // Getters
    public int GetNumJoints()
    {
        // Get joint length
        if (articulationChain == null)
        {
            return 0;
        }
        return articulationChain.Length;
    }

    public ArticulationBody[] GetJoints()
    {
        return articulationChain;
    }

    public float[] GetCurrentJointTargets()
    {
        // Container
        float[] targets = new float[articulationChain.Length];
        // Get each joint target from xDrive
        for (int i = 0; i < articulationChain.Length; ++i)
        {
            targets[i] = articulationChain[i].xDrive.target;
            targets[i] *= Mathf.Deg2Rad;
        }
        return targets;
    }
}
