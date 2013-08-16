using UnityEngine;
using System.Collections;
[AddComponentMenu("Dynamic Occlusion/LOD")]
public class IOClod : MonoBehaviour {
	
	public float Lod1;
	public float Lod2;
	public float LodMargin;
	public bool LodOnly;
	
	private float lod_1;
	private float lod_2;
	private float lodMargin;
	private IOCcam iocCam;
	private int counter;
	private Renderer[] rs0;
	private Renderer[] rs1;
	private Renderer[] rs2;
	private Renderer[] rs;
	private bool hidden;
	private int currentLod;
	private float prevDist;
	private float distOffset;
	private int lods;
	private float dt;
	private float distFromCam;
	private float hitTimeOffset;
	private float prevHitTime;
	private bool sleeping;

	void Start () {
		iocCam =  Camera.mainCamera.GetComponent<IOCcam>();
		if(iocCam == null)
		{
			this.enabled = false;
			return;
		}
		if(Lod1 != 0)
		{
			lod_1 = Lod1;
		}
		else lod_1 = iocCam.lod1Distance;
		if(Lod2 != 0)
		{
			lod_2 = Lod2;
		}
		else lod_2 = iocCam.lod2Distance;
		if(LodMargin != 0)
		{
			lodMargin = LodMargin;
		}
		else lodMargin = iocCam.lodMargin;
		
		prevDist = 0f;
		if(transform.Find("Lod_0"))
		{
			lods = 1;
			rs0 = transform.Find("Lod_0").GetComponentsInChildren<Renderer>(false);
			if(transform.Find("Lod_1"))
			{
				lods++;
				rs1 = transform.Find("Lod_1").GetComponentsInChildren<Renderer>(false);
				if(transform.Find("Lod_2"))
				{
					lods++;
					rs2 = transform.Find("Lod_2").GetComponentsInChildren<Renderer>(false);
				}
			}
		}
		else
		{
			lods = 0;
		}
		rs = GetComponentsInChildren<Renderer>(false);
		
		if(iocCam == null || iocCam.enabled == false)
		{
			this.enabled = false;
			ShowLod(1);
		}
		else
		{
			switch(LodOnly)
			{
			case false:
				HideAll();
				break;
			case true:
				ShowLod(3000f);
				break;
			}
		}
		prevHitTime = Time.time;
		sleeping = true;
	}
	
	void Update () {
		switch(LodOnly)
		{
		case false:
			if(!hidden)
			{
				counter--;
				if(counter <= 0)
				{
					Hide();
				}
			}
			break;
		case true:
			if(!sleeping)
			{
				if(counter <= 0)
				{
					ShowLod(3000f);
					sleeping = true;
				}
				else
				{
					counter--;
				}
			}
			break;
		}
	}
	
	
	public void UnHide(float d)
	{
		counter = iocCam.hideDelay;
		if(hidden)
		{
			hidden = false;
			ShowLod(d);
		}
		else
		{
			if(lods > 0)
			{
				distOffset = prevDist - d;
				hitTimeOffset = Time.time - prevHitTime;
				prevHitTime = Time.time;
				if(Mathf.Abs(distOffset) > lodMargin | hitTimeOffset > 3f)
				{
					ShowLod(d);
					prevDist = d;
					sleeping = false;
				}
				
			}
		}
	}
	
	private void ShowLod(float d)
	{
		switch(lods)
		{
		case 0:
			currentLod = -1;
			break;
		case 2:
			if(d < lod_1)
			{
				currentLod = 0;
			}
			else
			{
				currentLod = 1;
			}
			break;
		case 3:
			if(d < lod_1)
			{
				currentLod = 0;
			}
			else if(d > lod_1 & d < lod_2)
			{
				currentLod = 1;
			}
			else
			{
				currentLod = 2;
			}
			break;
		}

		switch(currentLod)
		{
		case 0:
			foreach(Renderer r in rs0)
			{
				r.enabled = true;
			}
			foreach(Renderer r in rs1)
			{
				r.enabled = false;
			}
			if(lods == 3)
			{
				foreach(Renderer r in rs2)
				{
					r.enabled = false;
				}
			}
			break;
		case 1:
			foreach(Renderer r in rs1)
			{
				r.enabled = true;
			}
			foreach(Renderer r in rs0)
			{
				r.enabled = false;
			}
			if(lods == 3)
			{
				foreach(Renderer r in rs2)
				{
					r.enabled = false;
				}
			}
			break;
		case 2:
			foreach(Renderer r in rs2)
			{
				r.enabled = true;
			}
			foreach(Renderer r in rs0)
			{
				r.enabled = false;
			}
			foreach(Renderer r in rs1)
			{
				r.enabled = false;
			}
			break;
		default:
			foreach(Renderer r in rs)
			{
				r.enabled = true;
			}
			break;
		}
	}
	public void Hide()
	{
		hidden = true;
		switch(currentLod)
		{
		case 0:
			foreach(Renderer r in rs0)
			{
				r.enabled = false;
			}
			break;
			
		case 1:
			foreach(Renderer r in rs1)
			{
				r.enabled = false;
			}
			break;
		case 2:
			foreach(Renderer r in rs2)
			{
				r.enabled = false;
			}
			break;
		default:
			foreach(Renderer r in rs)
			{
				r.enabled = false;
			}
			break;
		}
	}
	public void HideAll()
	{
		hidden = true;
		foreach(Renderer r in rs)
		{
			r.enabled = false;
		}
	}
}
