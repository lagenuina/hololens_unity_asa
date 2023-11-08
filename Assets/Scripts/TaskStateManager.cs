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
    private Vector3 vectorWorldFrame, desiredToolFrameASA, vectorToolFrame, toolFramePositionInitial;
    private bool moveFrame = false;
    public bool moveArm = false;
    private bool followEE = false;
    public bool setTarget = false, settingTarget = false, calibratingAnchor = false;
    private bool updatePosition = false;

    [SerializeField] private Interactable toggleSwitchArm;

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
        // ROSConnection.GetOrCreateInstance().RegisterPublisher<StringMsg>("/debug");
        ROSConnection.GetOrCreateInstance().RegisterPublisher<BoolMsg>("/setting_target");
        ROSConnection.GetOrCreateInstance().RegisterPublisher<PointMsg>("/my_gen3/calibrate_anchor");

        ROSConnection.GetOrCreateInstance().Subscribe<PointMsg>("/my_gen3/tf_toolframe", ToolFrameUpdate);

        // Get the TextToSpeech component from the GameObject
        textToSpeech = toolFrameObject.GetComponent<TextToSpeech>();
    }

    private Vector3 ConvertWorldASA(Vector3 positionVector, string ToFrame)
    {
        Vector3 newPositionVector = Vector3.zero; // Initialize the vector

        if (ToFrame == "ToASA")
        {
            positionVector -= spatialAnchor.transform.position;
            newPositionVector = spatialAnchor.transform.InverseTransformDirection(positionVector);
        }
        else if (ToFrame == "ToWorld")
        {
            Vector3 vectorWorldFrame = spatialAnchor.transform.TransformDirection(positionVector);
            newPositionVector = vectorWorldFrame + spatialAnchor.transform.position;
        }

        return newPositionVector;
    }

    void ToolFrameUpdate(PointMsg toolFramePos){

        //publisher.StringMessage("/debug", (new Vector3((float)toolFramePos.x, (float)toolFramePos.y, -(float)toolFramePos.z)).ToString());
        Vector3 vectorToolFrameASA = new Vector3((float)toolFramePos.x, (float)toolFramePos.y, -(float)toolFramePos.z);
        Vector3 vectorToolFrameWorld = ConvertWorldASA(vectorToolFrameASA, "ToWorld");

        if (updatePosition)
        {
            toolFrameObject.transform.position = vectorToolFrameWorld;
            toolFramePositionInitial = vectorToolFrameASA;
            updatePosition = false;
        }

        if (followEE)
        {
            toolFrameObject.transform.position = vectorToolFrameWorld;
        }
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
        setTarget = false;
        moveArm = false;
        followEE = true;
        Dialog.Open(DialogPrefab, DialogButtonType.OK, "Now tracking end-effector position.", "Press 'Follow EE' on the Hand Menu to disable.", true);
    }

    public void MoveArm()
    {
        followEE = false;
        setTarget = false;
        moveArm = true;
        Dialog.Open(DialogPrefab, DialogButtonType.OK, "", "Grab and move the hologram to manually move the end-effector.", true);
        textToSpeech.StartSpeaking("Tracking on. Grab and move the sphere to move the robot arm.");

        toggleSwitchArm.IsToggled = true;
    }
    
    public void SetTarget()
    {
        followEE = false;
        moveArm = false;
        setTarget = true;
        Dialog.Open(DialogPrefab, DialogButtonType.OK, "Set target", "Place the sphere where you want the robotic arm to move.", true);
        textToSpeech.StartSpeaking("Put the sphere where you want the robot arm to move.");

        toggleSwitchArm.IsToggled = true;
    }

    public void SendTargetPosition()
    {
        if (calibratingAnchor){
            UpdateAnchorPosition();
            calibratingAnchor = false;
        }
        else{
            settingTarget = false;
            setTarget = false;
        }
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

    public void ToggleArmControl(){
        if (toggleSwitchArm.IsToggled){
            textToSpeech.StartSpeaking("You can now control Kinova arm.");
        }
        else{

            followEE = false;
            moveArm = false;
            setTarget = false;
            textToSpeech.StartSpeaking("Kinova arm manual control was disabled.");
        }
    }

    public void calibrateAnchor(){
        calibratingAnchor = true;
        updatePosition = true;
    }

    public void UpdateAnchorPosition(){

        Vector3 anchorDifference = toolFramePositionInitial - desiredToolFrameASA;
        publisher.Position("/my_gen3/calibrate_anchor", anchorDifference);
    }


    // Update is called once per frame
    void Update()
    {
        desiredToolFrameASA = ConvertWorldASA(toolFrameObject.transform.position, "ToASA");

        bool trackingState = moveFrame;
        publisher.BoolMessage("/my_gen3/teleoperation/state", trackingState);
        publisher.BoolMessage("/setting_target", settingTarget);
        
        publisher.Pose("/hologram_feedback/pose", desiredToolFrameASA, new Quaternion(0, 0, 0, 1));
    }
}