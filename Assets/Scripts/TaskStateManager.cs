using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using RosMessageTypes.Scripts;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Audio;

public class TaskStateManager : MonoBehaviour
{
    [SerializeField] private ROSPublisher publisher;
    [SerializeField] private GameObject spatialAnchor, toolFrameObject;
    [SerializeField] private bool moveArm = false, settingTarget = false;
    public bool isTracking = false, setTarget = false, calibratingAnchor = false;
    [SerializeField] private Interactable toggleSwitchArm;
    private int gripperState = 0; // gripper is open
    private Vector3 vectorToolFrameASA, desiredToolFrameASA, vectorToolFrameWorld, toolFramePositionInitial;
    float awaitingResponseUntilTimestamp = -1;

    private TextToSpeech textToSpeech;
    private Renderer toolFrameRenderer;

    [SerializeField]
    [Tooltip("Assign DialogSmall_192x96.prefab")]
    private GameObject dialogPrefab;

    // Small dialog prefab to be displayed
    public GameObject DialogPrefab
    {
        get => dialogPrefab;
        set => dialogPrefab = value;
    }

    void Start()
    {
        //Service
        ROSConnection.GetOrCreateInstance().RegisterRosService<TrackingStateRequest, TrackingStateResponse>("/tracking_state_service");

        // Publishers
        ROSConnection.GetOrCreateInstance().RegisterPublisher<PoseMsg>("/hologram_feedback/pose");
        ROSConnection.GetOrCreateInstance().RegisterPublisher<BoolMsg>("/my_gen3/teleoperation/tracking");
        ROSConnection.GetOrCreateInstance().RegisterPublisher<PointMsg>("/my_gen3/calibrate_anchor");
        ROSConnection.GetOrCreateInstance().RegisterPublisher<Int32Msg>("/my_gen3/teleoperation/gripper_state");

        // Subscribers
        ROSConnection.GetOrCreateInstance().Subscribe<PointMsg>("/my_gen3/tf_toolframe", ToolFrameUpdate);

        // Get TextToSpeech and Renderer components of the sphere
        textToSpeech = toolFrameObject.GetComponent<TextToSpeech>();
        toolFrameRenderer = toolFrameObject.GetComponent<Renderer>();
    }

    // Callback function for handling tool frame updates
    void ToolFrameUpdate(PointMsg toolFramePos)
    {
        vectorToolFrameASA = new Vector3((float)toolFramePos.x, (float)toolFramePos.y, -(float)toolFramePos.z);
        vectorToolFrameWorld = ConvertWorldASA(vectorToolFrameASA, "ToWorld");
    }

    // Convert position vectors between world and spatial anchor reference frames
    private Vector3 ConvertWorldASA(Vector3 positionVector, string toFrame)
    {
        Vector3 newPositionVector = Vector3.zero; // Initialize the vector

        switch (toFrame)
        {
            case "ToASA":
                newPositionVector = spatialAnchor.transform.InverseTransformDirection(positionVector - spatialAnchor.transform.position);
                break;
            case "ToWorld":
                newPositionVector = spatialAnchor.transform.TransformDirection(positionVector) + spatialAnchor.transform.position;
                break;
        }

        return newPositionVector;
    }

    private void ActivateControl(bool isMoveArm, string startSpeakingMessage)
    {
        toolFrameRenderer.enabled = true;

        if (!isTracking)
        {
            string dialogTitle = isMoveArm ? string.Empty : "Set target";
            string dialogContent = isMoveArm 
                ? "Grab and move the hologram to manually move the end-effector." 
                : "Place the sphere where you want the robotic arm to move.";

            Dialog.Open(DialogPrefab, DialogButtonType.OK, dialogTitle, dialogContent, true);
            textToSpeech.StartSpeaking($"Tracking on. {startSpeakingMessage}");

            isTracking = true;
        }
        else
        {
            string controlType = isMoveArm 
                ? "Move arm" 
                : "Set target";

            if ((isMoveArm && moveArm) || (!isMoveArm && setTarget))
            {
                textToSpeech.StartSpeaking($"{controlType} control is already active.");
            }
            else
            {
                textToSpeech.StartSpeaking($"Switched to {controlType} control.");
            }
        }

        moveArm = isMoveArm;
        setTarget = !isMoveArm;
        settingTarget = !isMoveArm;

        ChangeTrackingState(isTracking);
    }

    // Call service to change tracking state
    public void ChangeTrackingState(bool activate)
    {
        TrackingStateRequest trackingRequest = new TrackingStateRequest(activate);

        // Send message to ROS and return the response
        ROSConnection.GetOrCreateInstance().SendServiceMessage<TrackingStateResponse>("/tracking_state_service", trackingRequest, Callback_Service);
        awaitingResponseUntilTimestamp = Time.time + 0.2f;
    }

    void Callback_Service(TrackingStateResponse response)
    {
        awaitingResponseUntilTimestamp = -1;
    }

    public void MoveArm()
    {
        ActivateControl(true, "Grab and move the sphere to move the robot arm.");
    }

    public void SetTarget()
    {
        ActivateControl(false, "Put the sphere where you want the robot arm to move.");
    }

    public void SendTargetPosition()
    {
        if (calibratingAnchor)
        {
            UpdateAnchorPosition();
            calibratingAnchor = false;
            textToSpeech.StartSpeaking("Spatial Anchor was calibrated.");
        }
        else
        {
            settingTarget = false;
        }
    }

    public void ToggleArmControl()
    {

        if (toggleSwitchArm.IsToggled)
        {
            textToSpeech.StartSpeaking("You can now control the arm.");
            isTracking = true;
        }
        else
        {
            moveArm = false;
            setTarget = false;
            settingTarget = false;
            textToSpeech.StartSpeaking("Manual control was disabled.");

            toolFrameRenderer.enabled = false;
            isTracking = false;
        }

        ChangeTrackingState(isTracking);
    }

    public void calibrateAnchor()
    {
        calibratingAnchor = true;
        toolFrameRenderer.enabled = true;
    }

    public void UpdateAnchorPosition()
    {
        toolFramePositionInitial = vectorToolFrameASA;
        Vector3 anchorDifference = toolFramePositionInitial - desiredToolFrameASA;

        publisher.Position("/my_gen3/calibrate_anchor", anchorDifference);
    }

    public void ManageGripper()
    {
        // Switch gripper state (open/close)
        gripperState = gripperState ^ 1;
    }

    // Update is called once per frame
    void Update()
    {   

        if (isTracking || calibratingAnchor)
        {
            toggleSwitchArm.IsToggled = true;
        }
        else


        {
            toggleSwitchArm.IsToggled = false;
            toolFrameObject.transform.position = vectorToolFrameWorld;
        }

        desiredToolFrameASA = ConvertWorldASA(toolFrameObject.transform.position, "ToASA");

        if (!settingTarget)
        {
            publisher.Pose("/hologram_feedback/pose", desiredToolFrameASA, new Quaternion(0, 0, 0, 1));
            
            if (setTarget)
            {
                settingTarget = true;
            }
        }

        publisher.BoolMessage("/my_gen3/teleoperation/tracking", isTracking);
        publisher.Int32Message("/my_gen3/teleoperation/gripper_state", gripperState);
    }
}