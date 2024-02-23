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
        ROSConnection.GetOrCreateInstance().RegisterPublisher<StringMsg>("/debug");
        ROSConnection.GetOrCreateInstance().Subscribe<PoseMsg>("/my_gen3/target_hologram", TargetUpdate);

        ROSConnection.GetOrCreateInstance().Subscribe<TargetInfoMsg>("/target_identifier", TargetIdentifierCallback);

        //ROSConnection.GetOrCreateInstance().Subscribe<StringMsg>("/medicine_name", MedicineNameCallback);
        ROSConnection.GetOrCreateInstance().Subscribe<Int32Msg>("/my_gen3/pick_and_place", RobotStateCallback);

        ROSConnection.GetOrCreateInstance().Subscribe<Int32Msg>("/target_counter", TargetCounterCallback);
        ROSConnection.GetOrCreateInstance().RegisterRosService<UpdateStateRequest, UpdateStateResponse>("/resume_task");
        ROSConnection.GetOrCreateInstance().ImplementService<UpdateStateRequest, UpdateStateResponse>("/local_help_request_service", RequestHelpCallback);

        textToSpeech = TODOMenu.GetComponent<TextToSpeech>();

        targetBox.SetActive(false);
        targetMedicine.SetActive(false);
        tray.SetActive(false);

        radialView = TODOMenu.GetComponent<RadialView>();
        //FollowMeToggle followMeToggle = TODOMenu.GetComponent<FollowMeToggle>();
    }

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

        publisher.StringMessage("/debug", expirationMonth);
    }

    //void MedicineNameCallback(StringMsg name)
    //{
    //    medicineName = name.data;

    //    publisher.StringMessage("/debug", medicineName);
    //}

    //void MedicineIDCallback(Int32Msg id)
    //{
    //    markerID = id.data;
    //}

    void TargetCounterCallback(Int32Msg targetCounter)
    {
        counter = targetCounter.data;
    }

    void TargetUpdate(PoseMsg targetPose)
    {
        Vector3 positionTargetASA = new Vector3((float)targetPose.position.x, (float)targetPose.position.y, -(float)targetPose.position.z);
        positionTargetWorld = ConvertWorldASA(positionTargetASA, "ToWorld");
        Quaternion orientationTargetWorld = new Quaternion((float)targetPose.orientation.x, -(float)targetPose.orientation.y, -(float)targetPose.orientation.z, (float)targetPose.orientation.w);

        targetMedicine.transform.position = positionTargetWorld;
        targetMedicine.transform.rotation = spatialAnchor.transform.rotation;

        if (positionTargetWorld != spatialAnchor.transform.position)
        {
            if (requestID != 0 & lhHelp)
            {
                if (requestID == 1)
                {
                    targetMedicine.SetActive(true);
                    targetBox.SetActive(false);
                    UpdateTrayPosition();
                    tray.SetActive(true);
                }
                else if (requestID == 2)
                {
                    targetMedicine.SetActive(true);
                    targetBox.SetActive(false);

                    UpdateTrayPosition();
                    tray.SetActive(true);
                }
                else if (requestID == 3)
                {

                    targetBox.transform.position = positionTargetWorld;
                    targetBox.transform.rotation = spatialAnchor.transform.rotation;

                    targetMedicine.SetActive(false);

                    targetBox.SetActive(true);

                    UpdateTrayPosition();
                    tray.SetActive(true);
                }
            }
            else
            {
                if (robotState == 0 || robotState == 1 || robotState == 2)
                {
                    targetMedicine.SetActive(true);
                    tray.SetActive(false);
                    targetBox.SetActive(false);
                }
                else if (robotState == 3 || robotState == 4)
                {
                    targetMedicine.SetActive(false);
                    targetBox.SetActive(false);

                    UpdateTrayPosition();

                    tray.SetActive(true);
                }
            }
            
        }
        else
        {
            if (requestID == 1)
            {
                targetBox.SetActive(false);
                UpdateTrayPosition();
                tray.SetActive(true);
            }
            targetMedicine.SetActive(false);
        }
    }

    Vector3 UpdateTrayPosition()
    {
        //if (counter < 3)
        //{
        //    positionTray = ConvertWorldASA(new Vector3(0.76f, -0.02f, -0.18f), "ToWorld");
        //}
        //else
        //{
        //    positionTray = ConvertWorldASA(new Vector3(0.39f, -0.02f, -0.15f), "ToWorld");
        //}

        
        positionTray = ConvertWorldASA(new Vector3(1.03f, -0.03f, -0.18f), "ToWorld");

        tray.transform.position = positionTray;
        tray.transform.rotation = spatialAnchor.transform.rotation;

        return positionTray;
    }

    private UpdateStateResponse RequestHelpCallback(UpdateStateRequest request)
    {
        requestID = request.state;
        lhHelp = true;
        TODOMenu.SetActive(true);
        radialView.enabled = true;

        //publisher.StringMessage("/debug", medicineName);
        //publisher.StringMessage("/debug", requestID.ToString());

        //publisher.StringMessage("/debug", requestID.ToString());

        // Switch to help state
        if (requestID == 0)
        {
            messageMenu = $"Remote nurse requested your help.";
        }

        if (requestID == 1)
        {
            // Object is not detected
            messageMenu = $"Help the robot look for {medicineName} (ID {markerID}).";
        }

        else if (requestID == 2)
        {
            // Medicine is expired
            messageMenu = $"Remove the expired medicine {medicineName} (ID {markerID}) and replace it.";
        }

        else if (requestID == 3)
        {
            // Object too big to grasp
            // Highlight object
            messageMenu = $"Grasp {medicineName} (ID {markerID}) from the highlighted box and place it on the tray.";
        }

        //Dialog.Open(DialogPrefab, DialogButtonType.OK, "Help requested", message, true);
        ButtonConfigHelper buttonConfigHelper = TODOMenu.GetComponentInChildren<ButtonConfigHelper>();
        Interactable menuInteractable = TODOMenu.GetComponentInChildren<Interactable>();

        menuInteractable.IsToggled = false;

        buttonConfigHelper.MainLabelText = messageMenu;
        textToSpeech.StartSpeaking("Remote operator has requested your help.");

        UpdateStateResponse updateReceived = new UpdateStateResponse();
        updateReceived.response = true;

        return updateReceived;
    }

    // Call service to resume task
    public void ResumeTask()
    {
        CallResumeService(0);
    }

    public void CallResumeService(short id)
    {
        UpdateStateRequest resumeRequest = new UpdateStateRequest(id);

        lhHelp = false;

        // Send message to ROS and return the response
        ROSConnection.GetOrCreateInstance().SendServiceMessage<UpdateStateResponse>("/resume_task", resumeRequest, Callback_Service);
        awaitingResponseUntilTimestamp = Time.time + 0.2f;
    }

    public void UpdateTarget()
    {
        CallResumeService(1);
    }

    void Callback_Service(UpdateStateResponse response){
        awaitingResponseUntilTimestamp = -1;

        if (response.response) {

        }

        TODOMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
