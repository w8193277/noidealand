using UnityEngine;
using System.Collections;

public class PrintToConsole_debug : MonoBehaviour {
	public string debugOutput;
	// Use this for initialization
	void Start () {
		Debug.Log("Loaded asset " + debugOutput + " in " + Time.realtimeSinceStartup.ToString("######.##") + " seconds after startup.");
	}
	
}
