using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraChan : MonoBehaviour
{
    public Camera maincamera;
    private WebCamTexture camTexture = null;
    void Start()
    {
        float h = maincamera.orthographicSize * 2;
        float w = h * maincamera.aspect; 
        //if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
        {
            transform.localScale = new Vector3(w, h,1);
        }
        //if (Input.deviceOrientation == DeviceOrientation.FaceUp)
        {
            //transform.localScale = new Vector3(h,w, 1);
            transform.localRotation *= Quaternion.Euler(0, 0, -90);
        }
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("Device is not found.");
            return;
        }
        Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("Permission denied");
            return;
        }
        Renderer rend = GetComponent<Renderer>();
        {
            WebCamDevice cam = WebCamTexture.devices[0];
            WebCamTexture wcam = new WebCamTexture(cam.name);
            wcam.Play();
            int width = wcam.width, height = wcam.height;
            if (width < 1280 || height < 720) { width *= 2; height *= 2; }
            camTexture = new WebCamTexture(cam.name, width, height,10);
            wcam.Stop();

            rend.material.mainTexture = camTexture;
            camTexture.Play();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
