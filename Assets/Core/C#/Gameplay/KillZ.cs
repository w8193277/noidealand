using UnityEngine;
using System.Collections;

public class KillZ : MonoBehaviour {

	public GameObject telepos;
	
	void OnTriggerEnter(Collider col){
		if(col.tag == "KillZoneVolume"){
			Debug.LogWarning("Player went out of bounds @ " + transform.position);
			transform.position = telepos.transform.position;
		}
	}
}
