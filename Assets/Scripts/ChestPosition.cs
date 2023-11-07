using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using Microsoft.MixedReality.Toolkit.UI;

public class ChestPosition : MonoBehaviour
{
    public ROSPublisher publisher;
    ROSConnection ros;
    [SerializeField] private PinchSlider sliderObject;
    [SerializeField] private Interactable toggleSwitchChest;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float32Msg>("/slider_value");
    }

    public void UpdateChestPosition(){
        
        // Adjust chest only if the control chest button is toogled
        if (toggleSwitchChest.IsToggled){
            
            float sliderValue = sliderObject.SliderValue;
            publisher.Float32Message("/slider_value", sliderValue);
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
