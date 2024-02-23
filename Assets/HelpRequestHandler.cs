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
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Security.Authentication.ExtendedProtection;
using System.Collections.Specialized;
using System.Linq.Expressions;

public class HelpRequestHandler : MonoBehaviour
{
    [SerializeField] private ROSPublisher publisher;
    [SerializeField] private GameObject spatialAnchor, TODOMenu, targetMedicine, targetBox, tray;
    private Vector3 positionTargetWorld;

    private TextToSpeech textToSpeech;
    private RadialView radialView;
    private Interactable menuInteractable;

    [SerializeField]
    [Tooltip("Assign DialogSmall_192x96.prefab")]
    private GameObject dialogPrefab;

    private Vector3 positionTray = Vector3.zero;
    private bool lhHelp = false;
    private int requestID, markerID, counter;
    private int robotState = 0;
    private string expirationMonth, expirationYear;
    private string messageMenu;
    private string medicineName = "";

    float awaitingResponseUntilTimestamp = -1;

    // Small dialog prefab to be displayed
    public GameObject DialogPrefab
    {
        get => dialogPrefab;
        set => dialogPrefab = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        medicineName = "None";

        // Subscribers
        ROSConnection.GetOrCreateInstance().Subscribe<PoseMsg>("/my_gen3/target_hologram", TargetUpdate);
        ROSConnection.GetOrCreateInstance().Subscribe<TargetInfoMsg>("/target_identifier", TargetIdentifierCallback);
        ROSConnection.GetOrCreateInstance().Subscribe<Int32Msg>("/my_gen3/pick_and_place", RobotStateCallback);
        ROSConnection.GetOrCreateInstance().Subscribe<Int32Msg>("/target_counter", TargetCounterCallback);

        // Services
        ROSConnection.GetOrCreateInstance().RegisterRosService<UpdateStateRequest, UpdateStateResponse>("/resume_task");
        ROSConnection.GetOrCreateInstance().ImplementService<UpdateStateRequest, UpdateStateResponse>("/local_help_request_service", RequestHelpCallback);

        textToSpeech = TODOMenu.GetComponent<TextToSpeech>();

        targetBox.SetActive(false);
        targetMedicine.SetActive(false);
        tray.SetActive(false);

        radialView = TODOMenu.GetComponent<RadialView>();
    }

    // Call service to resume task
    public void ResumeTask()
    {
        CallResumeService(0);
    }

    public void UpdateTarget()
    {
        CallResumeService(1);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetActiveObjects(bool targetMedicineActive, bool targetBoxActive, bool trayActive)
    {
        targetMedicine.SetActive(targetMedicineActive);
        targetBox.SetActive(targetBoxActive);
        tray.SetActive(trayActive);
    }

    void RobotStateCallback(Int32Msg pickandplaceState)
    {
        robotState = pickandplaceState.data;
    }

    void TargetIdentifierCallback(TargetInfoMsg target)
    {
        medicineName = target.name;
        expirationMonth = target.expiration.Substring(0, 2);
        expirationYear = target.expiration.Substring(2);
        markerID = target.id;
    }

    void TargetCounterCallback(Int32Msg targetCounter)
    {
        counter = targetCounter.data;
    }

    // Converts position from World (HoloLens) to ASA coordinate system or vice versa
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

    private void UpdateTargetVisibility(Vector3 positionTargetWorld)
    {
        bool shouldUpdateTray = requestID == 1 || requestID == 2 || requestID == 3;
        bool isMedicineActive = robotState == 0 || robotState == 1 || robotState == 2 || (requestID != 0 && lhHelp);
        bool isBoxActive = requestID == 3 && lhHelp;

        SetActiveObjects(isMedicineActive, isBoxActive, shouldUpdateTray);

        if (shouldUpdateTray)
        {
            UpdateTrayPosition();
        }

        targetMedicine.transform.position = positionTargetWorld;
        targetMedicine.transform.rotation = spatialAnchor.transform.rotation;
    }

    void TargetUpdate(PoseMsg targetPose)
    {
        Vector3 positionTargetASA = new Vector3((float)targetPose.position.x, (float)targetPose.position.y, -(float)targetPose.position.z);
        positionTargetWorld = ConvertWorldASA(positionTargetASA, "ToWorld");
        Quaternion orientationTargetWorld = new Quaternion((float)targetPose.orientation.x, -(float)targetPose.orientation.y, -(float)targetPose.orientation.z, (float)targetPose.orientation.w);

        UpdateTargetVisibility(positionTargetWorld);
    }

    private UpdateStateResponse RequestHelpCallback(UpdateStateRequest request)
    {
        requestID = request.state;
        lhHelp = true;
        TODOMenu.SetActive(true);
        radialView.enabled = true;

        messageMenu = requestID switch
        {
            0 => "Remote nurse requested your help.",
            1 => $"Help the robot look for {medicineName} (ID {markerID}).",
            2 => $"Remove the expired medicine {medicineName} (ID {markerID}) and replace it.",
            3 => $"Grasp {medicineName} (ID {markerID}) from the highlighted box and place it on the tray.",
            _ => messageMenu // In case of an unexpected requestID, keep the current message
        };

        TODOMenu.GetComponentInChildren<ButtonConfigHelper>().MainLabelText = messageMenu;
        TODOMenu.GetComponentInChildren<Interactable>().IsToggled = false;

        textToSpeech.StartSpeaking("Remote operator has requested your help.");

        return new UpdateStateResponse { response = true };
    }

    public void CallResumeService(short id)
    {
        UpdateStateRequest resumeRequest = new UpdateStateRequest(id);

        lhHelp = false;

        // Send message to ROS and return the response
        ROSConnection.GetOrCreateInstance().SendServiceMessage<UpdateStateResponse>("/resume_task", resumeRequest, Callback_Service);
        awaitingResponseUntilTimestamp = Time.time + 0.2f;
    }

    void Callback_Service(UpdateStateResponse response)
    {
        awaitingResponseUntilTimestamp = -1;

        TODOMenu.SetActive(false);
    }

    Vector3 UpdateTrayPosition()
    {
        positionTray = ConvertWorldASA(new Vector3(1.03f, -0.03f, -0.18f), "ToWorld");

        tray.transform.position = positionTray;
        tray.transform.rotation = spatialAnchor.transform.rotation;

        return positionTray;
    }
}
