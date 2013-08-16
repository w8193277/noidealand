/*================================================================
  Scene manager class, manages assets, etc.
  -Screen cursor locking
  -Menu management
  -Level bounds management
*/
using UnityEngine;
using System.Collections;
namespace Managers{
	public class KSGameManager : MonoBehaviour {
		// Cursor logic
		public bool _hideCursor;
		
		// Menu logic
		public bool _showMenu;
		public KeyCode menuKey;
		public float menuPositionY;
		public float menuPositionX;
		public GUISkin _menuSkin;
		public ThirdPersonCameraController _cameraScript;
		public ThirdPersonController _controller;
		
		// Bounds logic
		public GameObject[] levelBounds;
		public GameObject player;
		
		// define and create our GUI delegate
	    private delegate void GUIMethod(); 
	    private GUIMethod currentGUIMethod;
	    
	    // Cursor logic
	    void LateUpdate() {
	    	if(!_hideCursor && _showMenu){
	    		Time.timeScale = 0.0f;
	    		Screen.lockCursor = false;
	    		Screen.showCursor = true;
	    	}
	    	if(_hideCursor && !_showMenu){
	    		Time.timeScale = 1.0f;
	    		Screen.lockCursor = true;
	    		Screen.showCursor = false;
	    	}
	    	if(Input.GetKeyDown(menuKey)){
	    		_cameraScript.enabled = !_cameraScript.enabled;
	    		
	    		_hideCursor = !_hideCursor;
	    		_showMenu = !_showMenu;
	    	}
	    	if(_cameraScript == null){
	    		Debug.LogWarning("Camera input null. Please assign a camera script of the type \"Third Person Camera Controller\" in the inspector");
	    	}
	    }
	    
	 
	    public void Start () 
	    { 	
			// start with the main menu GUI
			this.currentGUIMethod = MainMenu; 
	    } 
	 
	    public void MainMenu() 
	    { 	
	    	if(_showMenu){
		    	if (GUI.Button (new Rect (Screen.width / 2 - menuPositionX,Screen.height + menuPositionY,Screen.width - 220,40), "Options")) 
		        {
		            // options button clicked, switch to new menu
		            this.currentGUIMethod = OptionsMenu;
		        }
	    	}
	    	if(!_showMenu){
	    		return;
	    	} 
	    } 
	 
	    private void OptionsMenu()
	    {
	    	if(_showMenu){
	    		if (GUI.Button (new Rect (Screen.width / 2 - menuPositionX,Screen.height + menuPositionY,Screen.width - 220,40), "Main Menu")) 
	       		 {
					// go back to the main menu
					this.currentGUIMethod = MainMenu;
	       		 }
	    	}
	    }    
	 
	    // Update is called once per frame 
	    public void OnGUI () 
	    { 	
	    	GUI.skin = _menuSkin;
	    	if(!_showMenu){
	    		return;
	    	}
	    	if(_showMenu){
	        	this.currentGUIMethod(); 
	    	}
	    } 
	}
}