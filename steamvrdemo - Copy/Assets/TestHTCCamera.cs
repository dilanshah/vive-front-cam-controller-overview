using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Logging;
using Valve.VR;
using IBM.Watson.DeveloperCloud.Services.VisualRecognition.v3;
using IBM.Watson.DeveloperCloud.Utilities;

public class TestHTCCamera : MonoBehaviour
{
    public RawImage m_RawImage;
    public Material m_Material;
    private WebCamTexture m_WebCamTexture;

    VisualRecognition m_VisualRecognition;

    void Start()
    {
        LogSystem.InstallDefaultReactors();
        m_VisualRecognition = new VisualRecognition();

        StartCoroutine(StartCamera());
    }

    private IEnumerator StartCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        WebCamDevice[] devices = WebCamTexture.devices;
        foreach (WebCamDevice device in devices)
            Log.Debug("TestHTCCamera", "devices: {0}, isFrontFacing: {1}", device.name, device.isFrontFacing);

        m_WebCamTexture = new WebCamTexture(612, 460, 30);
        yield return null;

        if (m_WebCamTexture == null)
        {
            Log.Debug("TestHTCCamera", "cannot start camera!");
            yield break;
        }

        m_WebCamTexture.deviceName = devices[0].name;
        Log.Debug("TestHTCCamera", "selecting device {0}", devices[0].name);

        m_RawImage.texture = m_WebCamTexture;

        m_Material.mainTexture = m_WebCamTexture;
        m_WebCamTexture.Play();
    }

    #region Public Functions
    public void Classify()
    {
        Log.Debug("WebCamRecognition", "Attempting to classify image!");
        Runnable.Run(ClassifyImage());
    }
    #endregion


    #region Private Functions

    private IEnumerator ClassifyImage()
    {
        yield return new WaitForEndOfFrame();

        //Color32[] colors = m_WebCamWidget.WebCamTexture.GetPixels32();
        //byte[] rawImageData = Utility.Color32ArrayToByteArray(colors);

        Texture2D image = new Texture2D(m_WebCamTexture.width, m_WebCamTexture.height, TextureFormat.RGB24, false);
        image.SetPixels32(m_WebCamTexture.GetPixels32());

        byte[] imageData = image.EncodeToPNG();
        string owners = "me";
        string myClassifierId = "classifier_name_651309947";
        string[] ownersArray = new string[] { owners };
        string[] classifierIdArray = new string[] { myClassifierId };


        m_VisualRecognition.Classify(OnClassify, imageData, ownersArray, classifierIdArray);
    }
    #endregion

    #region Event Handlers
    public void OnClassify(ClassifyTopLevelMultiple classify, string data)
    {
        if (classify != null)
        {
            Log.Debug("WebCamRecognition", "images processed: " + classify.images_processed);
            foreach (ClassifyTopLevelSingle image in classify.images)
            {
                Log.Debug("WebCamRecognition", "\tsource_url: " + image.source_url + ", resolved_url: " + image.resolved_url);
                foreach (ClassifyPerClassifier classifier in image.classifiers)
                {
                    Log.Debug("WebCamRecognition", "\t\tclassifier_id: " + classifier.classifier_id);
                    foreach (ClassResult classResult in classifier.classes)
                    {
                        Log.Debug("WebCamRecognition", "\t\t\tclass: " + classResult.m_class + ", score: " + classResult.score + ", type_hierarchy: " + classResult.type_hierarchy);
                        if (classResult.m_class == "htcvive" && classResult.score > 0.3)
                        {
                            Log.Debug("TESTHtcCamera", "classResult: {0}, score: {1}", classResult.m_class, classResult.score);
                            EventManager.Instance.SendEvent("OnViveDetected");


                        }
                    }
                }
            }
        } else
        {
            Log.Debug("WebCamRecognition", "Classification failed!");
        }
    }



    private void OnDetectFaces(FacesTopLevelMultiple multipleImages, string data)
    {
        if (multipleImages != null)
        {
            Log.Debug("WebCamRecognition", "images processed: {0}", multipleImages.images_processed);
            foreach (FacesTopLevelSingle faces in multipleImages.images)
            {
                Log.Debug("WebCamRecognition", "\tsource_url: {0}, resolved_url: {1}", faces.source_url, faces.resolved_url);
                foreach (OneFaceResult face in faces.faces)
                {
                    if (face.face_location != null)
                        Log.Debug("WebCamRecognition", "\t\tFace location: {0}, {1}, {2}, {3}", face.face_location.left, face.face_location.top, face.face_location.width, face.face_location.height);
                    if (face.gender != null)
                        Log.Debug("WebCamRecognition", "\t\tGender: {0}, Score: {1}", face.gender.gender, face.gender.score);
                    if (face.age != null)
                        Log.Debug("WebCamRecognition", "\t\tAge Min: {0}, Age Max: {1}, Score: {2}", face.age.min, face.age.max, face.age.score);

                    if (face.identity != null)
                        Log.Debug("WebCamRecognition", "\t\tName: {0}, Score: {1}, Type Heiarchy: {2}", face.identity.name, face.identity.score, face.identity.type_hierarchy);
                }
            }
        } else
        {
            Log.Debug("WebCamRecognition", "Detect faces failed!");
        }
    }

    private void OnRecognizeText(TextRecogTopLevelMultiple multipleImages, string data)
    {
        if (multipleImages != null)
        {
            Log.Debug("WebCamRecognition", "images processed: {0}", multipleImages.images_processed);
            foreach (TextRecogTopLevelSingle texts in multipleImages.images)
            {
                Log.Debug("WebCamRecognition", "\tsource_url: {0}, resolved_url: {1}", texts.source_url, texts.resolved_url);
                Log.Debug("WebCamRecognition", "\ttext: {0}", texts.text);
                foreach (TextRecogOneWord text in texts.words)
                {
                    Log.Debug("WebCamRecognition", "\t\ttext location: {0}, {1}, {2}, {3}", text.location.left, text.location.top, text.location.width, text.location.height);
                    Log.Debug("WebCamRecognition", "\t\tLine number: {0}", text.line_number);
                    Log.Debug("WebCamRecognition", "\t\tword: {0}, Score: {1}", text.word, text.score);
                }
            }
        } else
        {
            Log.Debug("WebCamRecognition", "RecognizeText failed!");
        }
    }
    #endregion
}
