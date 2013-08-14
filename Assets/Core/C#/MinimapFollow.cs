using UnityEngine;
using System.Collections;

public class MinimapFollow : MonoBehaviour {

	public Transform Target;
	
	void LateUpdate(){
		transform.position = new Vector3(Target.transform.position.x, transform.position.y , Target.transform.position.z);
	}
}
