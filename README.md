# Set up Azure Spatial Anchor in Unity project
Most of the steps outlined below have been sourced from Microsoft's official documentation. This repository contains a Unity project with an interface to create and locate Azure Spatial Anchors.

## Prerequisites
You will need:
* **PC** - A PC running Windows.
* **Visual Studio** - [(Visual Studio 2019)](https://visualstudio.microsoft.com/downloads/) installed with the **Universal Windows Platform development workload** and the **Windows 10 SDK** (10.0.18362.0 or newer) component.
* **HoloLens device**.
* **Unity** - Unity 2020.3.48 with modules **Universal Windows Platform Build Support** and **Windows Build Support (IL2CPP)**.

## Set up your Project
Clone this repository by running the following command:
```
git clone https://github.com/lagenuina/hololens_unity_asa.git
```
In Unity, open the project in the Unity folder.
Navigate to Scenes and select "ASA".

## Set up your project for HoloLens
Follow this [tutorial](https://learn.microsoft.com/en-us/training/modules/learn-mrtk-tutorials/1-3-exercise-configure-unity-for-windows-mixed-reality) (starting from **Switch Build Platform** section) and make sure you:
* Can successfully deploy Unity to Hololens.
* Have your Unity project configured for Windows Mixed Reality.

## Import ASA
1. Launch [Mixed Reality Feauture Tool](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool)
2. Select your project path - the folder that contains folders such as Assets, Packages, ProjectSettings, and so on - and select **Discover Features**
3. Under *Azure Mixed Reality Services*, select both **Azure Spatial Anchors SDK Core** and **Azure Spatial Anchors SDK for Windows**
4. Press **Get Features** --> **Import** --> **Approve** --> **Exit**

## Set Capabilities
1. Go to **Edit** > **Project Setting** > **Player**
2. Make sure the **Universal Windows Platform Settings** tab is selected
3. In the Publishing Settings Configuration section, enable the following
* InternetClient
* InternetClientServer
* PrivateNetworkClientServer
* SpatialPerception (might already be enabled)

## Set up the main camera
1. In the Hierarchy Panel, select **Main Camera** under MixedRealityPlayspace.
2. In the **Inspector**, set its transform position to **0,0,0**.
3. Find the Clear Flags property, and change the dropdown from **Skybox** to **Solid Color**.
4. Select the **Background** field to open a color picker.
5. Set R, G, B, and A to 0.
6. Select **Add Component** at the bottom and add the **Tracked Pose Driver Component** to the camera

## Create an Azure Spatial Anchors account
1. Follow [this guide](https://learn.microsoft.com/en-us/azure/spatial-anchors/how-tos/create-asa-account?tabs=azure-portal) to create an Azure Spatial Anchors account.
2. In the Unity project, navigate to ASA GameObject, and paste your **Accound ID**, **Account Key** and **Account Domain** in the **Spatial Anchor Manager** Component.
3. If you have an Anchor ID to localize, paste it in the **ASA Script** component.

## Deploy the HoloLens App
You're all set to run the project!

Cubes serve as visual representations of anchors.
The Hand Menu activates when the palm of your *Left Hand* is detected. You can:
  1) **Create a new Spatial Anchor**: a yellow cube will appear. You can move the cube to your desired location for anchor creation. If the anchor is successfully created, a Popup Dialog will appear with the associated AnchorID.
  2) **Locate a Spatial Anchor**: the anchor with the associated ID is automatically located on startup. However, if it's not successfully located, you can select this option from the menu.
  3) **Visually hide the Located Anchor**.
