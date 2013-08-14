using UnityEngine;
using System.Collections;

public class LocationVolume : MonoBehaviour
{
	public string locationName = "Unknown";
	public string deflocationName = "Unknown";
	public GUIStyle HUDLocation;
	private string location;
	public CharacterController controller;
	void Start(){
		location = deflocationName;
	}
	void OnTriggerEnter(Collider col){
		if(col.tag == "LocationVolume"){
			location = col.name.ToString();
		}
		else{
			location = deflocationName;
		}
	}
	void OnTriggerExit(Collider col){
		location = deflocationName;
	}
	void OnGUI(){
		string n = location;
		GUIStyle l = HUDLocation;
		GUI.Box(new Rect(0.0f, Screen.height - 30, 128.0f, 32.0f), n, l);
	}
}

