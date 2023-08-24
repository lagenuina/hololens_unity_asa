using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProperties : MonoBehaviour
{
    public void Start()
    {
        gameObject.SetActive(false);
    }

    public void Disappear()
    {
        gameObject.SetActive(false);
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

    public void Transparent()
    {
        // Turn off the mesh renderer
        gameObject.GetComponent<Renderer>().enabled = false;
    }
}
