using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Audio;

public class ChestPosition : MonoBehaviour
{
    public ROSPublisher publisher;
    ROSConnection ros;
    [SerializeField] private PinchSlider sliderObject;
    [SerializeField] private Interactable toggleSwitchChest;
    private TextToSpeech textToSpeech;
    private bool sendChestValues = false;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float32Msg>("/slider_value");
        textToSpeech = gameObject.GetComponent<TextToSpeech>();
    }

    public void UpdateChestPosition(){
        
        // Adjust chest only if the control chest button is toogled
        if (sendChestValues){
            
            float sliderValue = sliderObject.SliderValue;
            publisher.Float32Message("/slider_value", sliderValue);
        }
    }
    
    public void WarningControlDisabled()
    {
        if (!sendChestValues)
        {
            textToSpeech.StartSpeaking("Please enable chest control to move the chest.");
        }
    }

    public void ControlChestState()
    {
        if (toggleSwitchChest.IsToggled)
        {
            textToSpeech.StartSpeaking("Chest control enabled.");
            sendChestValues = true;
        }
        else if (!toggleSwitchChest.IsToggled)
        {
            textToSpeech.StartSpeaking("Chest control disabled.");
            sendChestValues = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
