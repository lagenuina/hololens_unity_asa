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
    private Vector3 vectorWorldFrame, desiredPosition;
    private bool moveFrame = false;
    public bool moveArm = false;
    private bool followEE = false;
    public bool setTarget = false, settingTarget = false;
    private bool updatePosition = false;

    //private string serviceName = "/my_gen3/request_pose";

    //float awaitingResponseUntilTimestamp = -1;

    private TextToSpeech textToSpeech;

    [SerializeField]
    [Tooltip("Assign DialogSmall_192x96.prefab")]
    private GameObject dialogPrefab;

    // Small dialog prefab to be displayed
    public GameObject DialogPrefab
    {
        get => dialogPrefab;
        set => dialogPrefab = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        //ROSConnection.GetOrCreateInstance().RegisterRosService<UpdatePositionRequest, UpdatePositionResponse>(serviceName);
        ROSConnection.GetOrCreateInstance().RegisterPublisher<PoseMsg>("/hologram_feedback/pose");
        ROSConnection.GetOrCreateInstance().RegisterPublisher<BoolMsg>("/my_gen3/teleoperation/state");
        ROSConnection.GetOrCreateInstance().RegisterPublisher<StringMsg>("/debug");

        //ROSConnection.GetOrCreateInstance().Subscribe<BoolMsg>("/my_gen3/tracking", TrackingState);
        ROSConnection.GetOrCreateInstance().Subscribe<PointMsg>("/my_gen3/tf_toolframe", ToolFrameUpdate);

        // Get the TextToSpeech component from the GameObject
        textToSpeech = toolFrameObject.GetComponent<TextToSpeech>();
    }

    void ToolFrameUpdate(PointMsg toolFramePos)
    {
        //publisher.StringMessage("/debug", (new Vector3((float)toolFramePos.x, (float)toolFramePos.y, -(float)toolFramePos.z)).ToString());
        Vector3 vectorToolFrame = new Vector3((float)toolFramePos.x, (float)toolFramePos.y, -(float)toolFramePos.z);
        vectorWorldFrame = spatialAnchor.transform.TransformDirection(vectorToolFrame);
        vectorWorldFrame += spatialAnchor.transform.position;

        if (updatePosition)
        {
            toolFrameObject.transform.position = vectorWorldFrame;
            updatePosition = false;
        }

        if (followEE)
        {
            toolFrameObject.transform.position = vectorWorldFrame;
        }

        //if (setTarget)
        //{
        //    toolFrameObject.transform.position = vectorWorldFrame;
        //}
    }

    //public void MoveToolFrame()
    //{
    //    UpdatePositionRequest positionRequest = new UpdatePositionRequest(true);

    //    // Send message to ROS and return the response
    //    ROSConnection.GetOrCreateInstance().SendServiceMessage<UpdatePositionResponse>(serviceName, positionRequest, Callback_Service);
    //    awaitingResponseUntilTimestamp = Time.time + 0.2f; // don't send again for 1 second, or until we receive a response
    //}

    //void Callback_Service(UpdatePositionResponse response)
    //{
    //    awaitingResponseUntilTimestamp = -1;

    //    Vector3 vectorToolFrame = new Vector3((float)response.position.x, (float)response.position.y, -(float)response.position.z);

    //    //publisher.StringMessage("/debug", vectorToolFrame.ToString());

    //    // Convert the local position of the tool frame (with respect to the spatial anchor) to global
    //    vectorWorldFrame = spatialAnchor.transform.TransformDirection(vectorToolFrame);
    //    vectorWorldFrame += spatialAnchor.transform.position;

    //    toolFrameObject.transform.position = vectorWorldFrame;
    //}

    public void FollowEE()
    {
        if (followEE)
        {
            followEE = false;
        }
        else
        {
            followEE = true;
            Dialog.Open(DialogPrefab, DialogButtonType.OK, "Now tracking end-effector position.", "Press 'Follow EE' on the Hand Menu to disable.", true);
        }
    }
    public void MoveArm()
    {
        if (moveArm)
        {
            moveArm = false;
            textToSpeech.StartSpeaking("Tracking off.");
        }
        else
        {
            moveArm = true;
            Dialog.Open(DialogPrefab, DialogButtonType.OK, "", "Grab and move the hologram to manually move the end-effector.", true);
            textToSpeech.StartSpeaking("Tracking on.");
        }
    }
    public void SetTarget()
    {
        if (setTarget)
        {
            setTarget = false;
        }
        else
        {
            setTarget = true;
            Dialog.Open(DialogPrefab, DialogButtonType.OK, "Set target", "Place the sphere where you want the robotic arm to move.", true);
            }
    }

    public void SendTargetPosition()
    {
        //publisher.Pose("/hologram_feedback/pose", desiredPosition, new Quaternion(0, 0, 0, 1));
        settingTarget = false;
        setTarget = false;
    }

    public void pressedTrackingBool()
    {
        moveFrame = true;
        updatePosition = true;
    }

    public void releasedTrackingBool()
    {
        moveFrame = false;

        if (setTarget)
        {
            settingTarget = true;
        }
    }

    //public void ChangeTrackingState()
    //{
    //    if (tracking)
    //    {
    //        textToSpeech.StartSpeaking("Tracking off.");
    //        tracking = false;
    //    }
    //    else
    //    {
    //        // Update tool frame position
    //        MoveToolFrame();

    //        // Open Dialog and Speech
    //        Dialog.Open(DialogPrefab, DialogButtonType.OK, "", "Grab and move the hologram to manually move the end-effector.", true);
    //        textToSpeech.StartSpeaking("Tracking on.");

    //        // Start tracking
    //        tracking = true;
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        Vector3 positionWorldSpace = toolFrameObject.transform.position;
        positionWorldSpace -= spatialAnchor.transform.position;
        desiredPosition = spatialAnchor.transform.InverseTransformDirection(positionWorldSpace);
        
        bool trackingState = moveFrame;
        publisher.BoolMessage("/my_gen3/teleoperation/state", trackingState);

        if (!settingTarget)
        {
            publisher.Pose("/hologram_feedback/pose", desiredPosition, new Quaternion(0, 0, 0, 1));
        }
    }
}