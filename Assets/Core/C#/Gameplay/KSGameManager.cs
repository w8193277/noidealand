/*================================================================
  Scene manager class, manages assets, etc.
  -Screen cursor locking
  -Menu management
  -Level bounds management
  -Save management
*/
using UnityEngine;
using System.IO;
using IniParser;
using IniParser.Model;
using System.Collections;

namespace Managers
{
	public class KSGameManager : MonoBehaviour 
	{
		// Cursor logic
		public bool _hideCursor;
		
		// Menu logic
		public bool _showMenu;
		public KeyCode menuKey;
		public KeyCode acceptKey;
		public float volumeLevel = 1.0f;
		public GUISkin _menuSkin;
		public CharacterController _player;
		public ThirdPersonCameraController _cameraScript;
		public ThirdPersonController _controller;
		public AudioListener listener;
		
		// Bounds logic
		public GameObject[] levelBounds;
		public GameObject player;
		
		// define and create our GUI delegate
	    private delegate void GUIMethod(); 
	    private GUIMethod currentGUIMethod;
	    
	    // Cursor logic
	    void LateUpdate() 
	    {
	    	if(!_hideCursor && _showMenu)
	    	{
	    		//Time.timeScale = 0.0f;
	    		_player.enabled = false;
	    		_controller.enabled = false;
	    		Screen.lockCursor = false;
	    		Screen.showCursor = true;
	    	}
	    	if(_hideCursor && !_showMenu)
	    	{
	    		//Time.timeScale = 1.0f;
	    		_player.enabled = true;
	    		_controller.enabled = true;
	    		Screen.lockCursor = true;
	    		Screen.showCursor = false;
	    	}
	    	if(Input.GetKeyDown(menuKey))
	    	{
	    		_cameraScript.enabled = !_cameraScript.enabled;
	    		_hideCursor = !_hideCursor;
	    		_showMenu = !_showMenu;
	    	}
	    	if(_cameraScript == null)
	    	{
	    		Debug.LogWarning("Camera input null. Please assign a camera script of the type \"Third Person Camera Controller\" in the inspector");
	    	}
	    }
	    
	 
	    public void Start () 
	    { 	
			// start with the main menu GUI
			this.currentGUIMethod = MainMenu;
			//load configuration
	    } 
	 
	    public void MainMenu() 
	    { 	
	    	if(_showMenu){
	    		if (GUI.Button (new Rect (64, 16, 256,40), "Return to game")) 
		        {
		        	_cameraScript.enabled = true;
		            _showMenu = !_showMenu;
		            _hideCursor = true;
		        }
		    	if (GUI.Button (new Rect (64, 56, 256,40), "Options")) 
		        {
		            this.currentGUIMethod = OptionsMenu;
		        }
		        if (GUI.Button (new Rect (64, 96, 256,40), "Quit")) 
		        {
		            Application.Quit();
		        }
	    	}
	    	if(!_showMenu){
	    		return;
	    	} 
	    } 
	 
	    private void OptionsMenu()
	    {
	    	if(_showMenu){
	    		if (GUI.Button (new Rect (64, 16, 256,40), "Main Menu")) 
	       		 {
					// go back to the main menu
					this.currentGUIMethod = MainMenu;
	       		 }
	       		 if (GUI.Button (new Rect (64, 56, 128,40), "Audio Settings")) 
	       		 {
	       		 	this.currentGUIMethod = AudioMenu;
	       		 }
	       		 if (GUI.Button (new Rect (64, 106, 128,40), "Game")) 
	       		 {
	       		 	this.currentGUIMethod = GameMenu;
	       		 }
	       		 if (GUI.Button (new Rect (64, 146, 128,40), "Input")) 
	       		 {
	       		 	this.currentGUIMethod = InputMenu;
	       		 }
	    	} 
	    }
	    private void InputMenu()
	    {
	    	if(_showMenu)
	    	{
	    		Vector2 _scrollPosition = new Vector2();
		    	_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
				GUILayout.Label("Action");
				for (int n = 0; n < cInput.length; n++) 
				{
					GUILayout.Label(cInput.GetText(n, 0));
				}
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				GUILayout.Label("Primary");
				for (int n = 0; n < cInput.length; n++) {
					if (GUILayout.Button(cInput.GetText(n, 1)) && Input.GetMouseButtonUp(0)) 
					{
						cInput.ChangeKey(n, 1);
					}
				}
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				GUILayout.Label("Secondary");
				for (int n = 0; n < cInput.length; n++) {
					if (GUILayout.Button(cInput.GetText(n, 2)) && Input.GetMouseButtonUp(0)) 
					{
						cInput.ChangeKey(n, 2);
					}
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				GUILayout.EndScrollView();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Reset to Defaults") && Input.GetMouseButtonUp(0)) 
				{
					cInput.ResetInputs();
				}
				if (GUILayout.Button("Close") && Input.GetMouseButtonUp(0)) 
				{
					cGUI.ToggleGUI();
					this.currentGUIMethod = OptionsMenu;//cInput.ShowMenu(false);
				}
				GUILayout.EndHorizontal();
	    	}
	    	
	    }
	    private void AudioMenu()
	    {
	    	if(_showMenu){
		    	volumeLevel = GUI.HorizontalSlider(new Rect(70, 56, 245, 10), AudioListener.volume = volumeLevel, 0.0f, 1.0f);
		       	GUI.Box(new Rect(64,16,256,40),"Master Volume: " + volumeLevel.ToString("0.#"));
		       	if (GUI.Button (new Rect (64, 96, 128,40), "Save Settings")) 
		       	{
		       		 //SaveConfig();
		       	}
	    		if (GUI.Button (new Rect (64, 136, 256,40), "Return")) 
	       		{
					this.currentGUIMethod = OptionsMenu;
	       		}
	    	}
	    }
	 	private void SaveMenu()
	 	{
	 		if(_showMenu){
	    		if (GUI.Button (new Rect (64, 16, 256,40), "Return")) 
	       		{
					this.currentGUIMethod = GameMenu;
	       		}
	    	}
	 	}
	 	private void LoadMenu()
	 	{
	 		if (GUI.Button (new Rect (64, 16, 256,40), "Return")) 
	       	{
				this.currentGUIMethod = GameMenu;
	       	}
	 	}
	 	private void GameMenu()
	 	{
	 		if (GUI.Button (new Rect (64, 16, 256,40), "Save Game")) 
	 		{
				this.currentGUIMethod = SaveMenu;
	       	}
	       	if (GUI.Button (new Rect (64, 56, 256,40), "Load Game")) 
	       	{
				this.currentGUIMethod = LoadMenu;
	       	}
	       	if (GUI.Button (new Rect (64, 96, 256,40), "Return")) 
	       	{
				this.currentGUIMethod = OptionsMenu;
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
	    public void LoadGame()
	    {
	    }
	    public void SaveGame()
	    {
	    }
	}
}