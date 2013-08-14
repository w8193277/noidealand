using UnityEngine;
using System.Collections;

public class LightToggle : MonoBehaviour
{
	public bool on = false;
	public bool unlimited = false;
	public float batteryLife;
	public float rechargeRate;
	public float drainRate;
	public float _maxCharge;
	public float _minCharge;
	public GUIStyle HUDBattery;
	void Update(){
		if(unlimited){
			if (Input.GetKeyDown(KeyCode.F)){
				on = !on;
			}
			if (on){
				light.enabled = true;
			}
			else if (!on){
				light.enabled = false;
			}
		}
		if(!unlimited){
			if (Input.GetKeyDown(KeyCode.F)){
				on = !on;
			}
			if (on){
				light.enabled = true;
				Drain();
			}
			else if (!on){
				light.enabled = false;
				Recharge();
			}
		}
	}
	void Recharge(){
		if(batteryLife >= _maxCharge){
			return;
		}
		batteryLife += rechargeRate * Time.deltaTime;
	}
	void Drain(){
		if(batteryLife > _minCharge){
				batteryLife -= drainRate * Time.deltaTime;
		}
		else{
			light.enabled = false;
			on = !on;
		}
	}
	void OnGUI()
	{
		if(batteryLife < 20.0f){
			GUI.contentColor = Color.red;
		}
		if(!unlimited){
			GUI.Box(new Rect(100, Screen.height - 30 , 128.0f, 32.0f), "Battery " + (int)(batteryLife), HUDBattery);
		}
		if(unlimited){
			GUI.contentColor = Color.green;
			GUI.Box(new Rect(100, Screen.height - 30, 128.0f, 32.0f),"", HUDBattery);
		}
	}
}