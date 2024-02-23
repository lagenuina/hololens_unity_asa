using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProperties : MonoBehaviour
{
    [SerializeField] private GameObject referenceObject, spatialAnchor;

    public void Start()
    {
        gameObject.SetActive(false);
    }

    public void Disappear()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void Appear()
    {
        // Activate the objectToActivate GameObject
        gameObject.SetActive(true);

        // Position the objectToActivate in front of you
        gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        Vector3 cameraEulerAngles = Camera.main.transform.rotation.eulerAngles;
        Quaternion desiredRotation = Quaternion.Euler(0, cameraEulerAngles.y, 0);
        gameObject.transform.rotation = desiredRotation;
    }

    public void AppearRelativeTo()
    {
        // Activate the objectToActivate GameObject
        gameObject.SetActive(true);

        // Position the objectToActivate in front of you
        Vector3 positionWorldSpace = referenceObject.transform.position;
        positionWorldSpace -= spatialAnchor.transform.position;
        Vector3 desiredPosition = spatialAnchor.transform.InverseTransformDirection(positionWorldSpace);

        desiredPosition += new Vector3(0f, 0.15f, 0f);

        // Convert the position back to world space
        Vector3 desiredPositionWorld = spatialAnchor.transform.TransformDirection(desiredPosition);
        desiredPositionWorld += spatialAnchor.transform.position;

        gameObject.transform.position = desiredPositionWorld;
        
        Vector3 cameraEulerAngles = Camera.main.transform.rotation.eulerAngles;
        Quaternion desiredRotation = Quaternion.Euler(0, cameraEulerAngles.y, 0);
        gameObject.transform.rotation = desiredRotation;
    }

    public void Transparent()
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();

        // Toggle the enabled state of the renderer
        renderer.enabled = !renderer.enabled;
    }
}
