using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FD : WebCameraO
{

	public TextAsset faces;
	public TextAsset eyes;
	public TextAsset shapes;
	public Text textkun;

	private FaceProcessorLive<WebCamTexture> processor;

	[Serializable]
	private class jsondata
	{
		public List<int[]> faces;
	}
	/// <summary>
	/// Default initializer for MonoBehavior sub-classes
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		base.forceFrontalCamera = false; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly

		byte[] shapeDat = shapes.bytes;
		if (shapeDat.Length == 0)
		{
			string errorMessage =
				"In order to have Face Landmarks working you must download special pre-trained shape predictor " +
				"available for free via DLib library website and replace a placeholder file located at " +
				"\"OpenCV+Unity/Assets/Resources/shape_predictor_68_face_landmarks.bytes\"\n\n" +
				"Without shape predictor demo will only detect face rects.";

#if UNITY_EDITOR
			// query user to download the proper shape predictor
			if (UnityEditor.EditorUtility.DisplayDialog("Shape predictor data missing", errorMessage, "Download", "OK, process with face rects only"))
				Application.OpenURL("http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2");
#else
             UnityEngine.Debug.Log(errorMessage);
#endif
		}

		processor = new FaceProcessorLive<WebCamTexture>();
		processor.Initialize(faces.text, eyes.text, shapes.bytes);

		// data stabilizer - affects face rects, face landmarks etc.
		processor.DataStabilizer.Enabled = true;        // enable stabilizer
		processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
		processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

		// performance data - some tricks to make it work faster
		processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
		processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
	}

	/// <summary>
	/// Per-frame video capture processor
	/// </summary>
	protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
	{
		// detect everything we're interested in
		processor.ProcessTexture(input, TextureParameters);
		var url = "http://192.168.0.199:5454/posttest";
		// mark detected objects
		processor.MarkDetected();/*
		jsondata dtkun = new jsondata();
		dtkun.faces = new List<int[]>();*/
		string output_str = "";
        if (processor.Faces.Count == 0)
        {
			output = OpenCvSharp.Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created

			return true;
		}
		foreach(DetectedFaceO akun in processor.Faces)
        {
			OpenCvSharp.Mat destmat = processor.Image.Clone(new OpenCvSharp.Rect(akun.Region.TopLeft.X, akun.Region.TopLeft.Y, akun.Region.Width, akun.Region.Height));
			var tex2dkun = OpenCvSharp.Unity.MatToTexture(destmat);
			var out_bin = tex2dkun.EncodeToPNG();
			//output_str += $"{akun.Region.TopRight.X}, {akun.Region.TopRight.Y}, {akun.Region.BottomLeft.X}, {akun.Region.BottomLeft.Y }\n";

			var postData = Encoding.UTF8.GetBytes(output_str);
			var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
			{
				uploadHandler = new UploadHandlerRaw(out_bin),
				downloadHandler = new DownloadHandlerBuffer()
			};

			request.SetRequestHeader("Content-Type", "application/json");

			var operation = request.SendWebRequest();
			operation.completed += _ =>
			{
				//processor.Image.PutText(operation.webRequest.downloadHandler.text, akun.Region.TopRight, OpenCvSharp.HersheyFonts.HersheyPlain, 4, new OpenCvSharp.Scalar(255, 0, 255), 5);
				Debug.Log(operation.isDone);
				Debug.Log(operation.webRequest.downloadHandler.text);
				Debug.Log(operation.webRequest.isHttpError);
				Debug.Log(operation.webRequest.isNetworkError);
				textkun.text = operation.webRequest.downloadHandler.text;
				/*// apply
				var output22 = OpenCvSharp.Unity.MatToTexture(processor.Image);
				Surface.GetComponent<RawImage>().texture = output22;

				// Adjust image ration according to the texture sizes 
				Surface.GetComponent<RectTransform>().sizeDelta = new Vector2(output22.width, output22.height);*/
			};
		}
		// processor.Image now holds data we'd like to visualize
		output = OpenCvSharp.Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created

		return true;
	}
}
