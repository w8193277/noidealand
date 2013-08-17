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

namespace Managers{
	public class KSGameManager : MonoBehaviour {
		// Cursor logic
		public bool _hideCursor;
		
		// Menu logic
		public bool _showMenu;
		public KeyCode menuKey;
		public KeyCode acceptKey;
		public float volumeLevel = 1.0f;
		private string saveName = "config";
		public string extensionType = ".svdt";
		public GUISkin _menuSkin;
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
			//set cursor position relative to the screen and confine it to the game window
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
	       		 volumeLevel = GUI.HorizontalSlider(new Rect(70, 80, 245, 40), AudioListener.volume = volumeLevel, 0.0f, 1.0f);
	       		 GUI.Box(new Rect(64,56,256,40),"Master Volume: " + volumeLevel.ToString("0.#"));
	       		// if(){
	       		 	if (GUI.Button (new Rect (64, 106, 128,40), "Save Settings")) 
	       		 	{
	       		 		SaveConfig();
	       		 	}
	       		// }
	       		 if (GUI.Button (new Rect (64, 146, 128,40), "Game")) 
	       		 {
	       		 	this.currentGUIMethod = GameMenu;
	       		 }
	    	} 
	    }    
	 	private void SaveMenu(){
	 		if(_showMenu){
	    		if (GUI.Button (new Rect (64, 16, 256,40), "Return")) 
	       		{
					this.currentGUIMethod = GameMenu;
	       		}
	    	}
	 	}
	 	private void LoadMenu(){
	 		if (GUI.Button (new Rect (64, 16, 256,40), "Return")) 
	       	{
				this.currentGUIMethod = GameMenu;
	       	}
	 	}
	 	private void GameMenu(){
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
	    public void SaveConfig()
	    {
	    	FileIniDataParser fileIniData = new FileIniDataParser();
			if(!File.Exists(Application.dataPath + "\\SaveGame\\" + saveName + extensionType))
			{
				File.Create(Application.dataPath + "\\SaveGame\\" + saveName + extensionType);
			}
			IniData saveData = fileIniData.ReadFile(Application.dataPath + "\\saveGame\\" + saveName + extensionType);
			
			IniData modifiedData = ModifySaveData(saveData);
			fileIniData.SaveFile(Application.dataPath + "\\SaveGame\\" + saveName + extensionType, modifiedData);
	    }
	    public void LoadGame(){
	    	
	    }
	    private static IniData ModifySaveData(IniData modifiedData)
	    {	
	    	string volumeSection = "VolumeConfiguration";
	    	//modifiedData.Sections.GetSectionData(volumeSection).Comments.Add(volumeSection);
	    	modifiedData.Sections.AddSection(volumeSection);
	    	modifiedData.Sections.GetSectionData(volumeSection).Keys.AddKey("volume","1.0");
	    	return modifiedData;
	    }
	}
}