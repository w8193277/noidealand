using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class ScreenCaptureManager : MonoBehaviour {
	string _1 = "Currently disabled due to issues and incompleteness.";
	void Awake(){
		Debug.Log( _1 );
	}
/*
	public KeyCode capturePNGKey;
	//Resolution of image, int of 1 - 4 
	public int resolutionQuality = 1;
	
	static string lastCapture = Directory.GetFiles(Application.dataPath + "/ScreenCapture/").ToString();
	static int a = Convert.ToInt32(lastCapture);
	static int i = a;
	void LateUpdate(){
		
		if(Input.GetKeyUp(capturePNGKey)){
			i += 1;
			if(i == null){
				i = 0;
			}
			if (i == a){
				return;
			}
			Application.CaptureScreenshot(Application.dataPath + "/ScreenCapture/" + i + ".png", resolutionQuality);
			Debug.Log("Created screen capture");
			if(!System.IO.File.Exists(Application.dataPath + "/ScreenCapture/")){
				System.IO.Directory.CreateDirectory(Application.dataPath + "/ScreenCapture/");
			}
		}
	}*/
}
//FormatException: Input string was not in the correct format
//Rethrow as TypeInitializationException: An exception was thrown by the type initializer for ScreenCaptureManager
//Assets/Core/C#/ScreenCaptureManager.cs(19,38): warning CS0162: Unreachable code detected