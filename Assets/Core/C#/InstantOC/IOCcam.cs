using UnityEngine;
using System.Collections;
[AddComponentMenu("Dynamic Occlusion/Camera")]
public class IOCcam : MonoBehaviour {
	
	public LayerMask layerMsk;
	public int samples;
	public float viewDistance;
	public int hideDelay;
	public float lod1Distance;
	public float lod2Distance;
	public float lodMargin;
	
	private RaycastHit hit;
	private Ray r;
	private int layerMask;
	private IOClod l;
	
	void Awake () {
		hit = new RaycastHit();
		if(viewDistance == 0) viewDistance = 100;
		camera.farClipPlane = viewDistance;
	}

	
	void Update () {
		for(int k=0; k <= samples; k++)
		{
			r = camera.ScreenPointToRay(new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 0f));
			if(Physics.Raycast(r, out hit, viewDistance, layerMsk.value))
			{
				if(l = hit.transform.GetComponent<IOClod>())
				{
					l.UnHide(hit.distance);
				}
				else{
					return;
				}
				//else if(l = hit.transform.parent.GetComponent<IOClod>())
				//{
				//	l.UnHide(hit.distance);
				//}
			}
		}
	}
}
