using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using Microsoft.MixedReality.Toolkit.UI;

public class ButtonsManager : MonoBehaviour
{
    [SerializeField] private ROSPublisher publisher;
    [SerializeField] private TaskStateManager taskManager;
    [SerializeField] private GameObject vButton;
    [SerializeField] private GameObject spatialAnchor;

    private Interactable toggleInteractable;

    // Start is called before the first frame update
    void Start()
    {
        vButton.SetActive(false);
    }

    public void AppearRelativeTo()
    {
        // Position the objectToActivate in front of you
        Vector3 positionWorldSpace = gameObject.transform.position;
        positionWorldSpace -= spatialAnchor.transform.position;
        Vector3 desiredPosition = spatialAnchor.transform.InverseTransformDirection(positionWorldSpace);

        desiredPosition += new Vector3(0f, 0.15f, 0f);

        // Convert the position back to world space
        Vector3 desiredPositionWorld = spatialAnchor.transform.TransformDirection(desiredPosition);
        desiredPositionWorld += spatialAnchor.transform.position;

        Vector3 cameraEulerAngles = Camera.main.transform.rotation.eulerAngles;
        Quaternion desiredRotation = Quaternion.Euler(0, cameraEulerAngles.y, 0);

        //if (taskManager.setTarget || taskManager.calibratingAnchor)
        //{
        //    toggleSwitchTracking.SetActive(false);
        //    vButton.SetActive(true);
        //    vButton.transform.position = desiredPositionWorld;
        //    vButton.transform.rotation = desiredRotation;
        //}
        //else if (taskManager.moveArm)
        //{
        //    vButton.SetActive(false);
        //    toggleSwitchTracking.SetActive(true);
        //    toggleSwitchTracking.transform.position = desiredPositionWorld;
        //    toggleSwitchTracking.transform.rotation = desiredRotation;
        //}
        //else{
        //    vButton.SetActive(false);
        //    toggleSwitchTracking.SetActive(false);
        //}

        if (taskManager.setTarget || taskManager.calibratingAnchor)
        {
            vButton.SetActive(true);
            vButton.transform.position = desiredPositionWorld;
            vButton.transform.rotation = desiredRotation;
        }
        else
        {
            vButton.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
