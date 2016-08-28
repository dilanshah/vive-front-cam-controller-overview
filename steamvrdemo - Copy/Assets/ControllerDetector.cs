using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Logging;

public class ControllerDetector : MonoBehaviour {
    void OnEnable()
    {
        EventManager.Instance.RegisterEventReceiver("OnViveDetected", OnViveDetected);
}



    void OnDisable()
    {
        EventManager.Instance.UnregisterEventReceiver("OnViveDetected", OnViveDetected);
    }

    private void OnViveDetected(object[] args)
    {
        //  introduce a vive controller here 
        //  the trigger, trackpad, menu button 
        Log.Debug("controllerDetector", "Controller Detected!");


    }
}
