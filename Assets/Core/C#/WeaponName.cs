using UnityEngine;
using System.Collections;

public class WeaponName : MonoBehaviour
{
	public string weaponName = "default name";
	public GUIStyle HUDWeapon;
	void OnGUI(){
		string w = weaponName;
		GUIStyle g = HUDWeapon;
		GUI.Box(new Rect(0.0f, Screen.height - 30, 128.0f, 32.0f), w, g);
	}
}

