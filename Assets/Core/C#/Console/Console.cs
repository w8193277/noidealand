//####################################################################//
// Console.cs														  //
// Main class for runtime debugging and command execution.			  //
// currently incomplete.											  //
// (c)2013- Daniel Cornelius, Kerfuffle Studios. All rights reserved. //
//####################################################################//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Console : MonoBehaviour {
	public GUISkin				consoleSkin;
	public CharacterController	_characterController;
	string 						command = ""; // default value of the textfield
	
	private bool 				showConsole;
	private Vector2 			scrollPosition; // scroll position of the content scroll view
	public Rect					consoleRect = new Rect(20,20,Screen.width,Screen.height / 2);
	
	// values to check for commands, if they arent listed here... they arent valid.
	private string				TESTLIGHT = "testlight"; // the value for the "testlight" command, which spawns a spot light with a large outer angle.
	private string				DEFAULT = ""; // default textfield value for checking.
//================================================================================================================================================================	
	// message type handling.
	//============================================================================
	struct ConsoleMessage{
		public readonly string message;
		public readonly string stackTrace;
		public readonly LogType type;
		public ConsoleMessage(string message, string stackTrace, LogType type){
			this.message	= message;
			this.stackTrace = stackTrace;
			this.type		= type;
		}
	}
	List<ConsoleMessage> entries = new List<ConsoleMessage>();
//================================================================================================================================================================
	void OnEnable(){
		Application.RegisterLogCallback(HandleLog);
	}
	void OnDisable(){
		Application.RegisterLogCallback(null);
	}
	
	void LateUpdate(){
		if(Input.GetKeyDown(KeyCode.BackQuote)){
			showConsole = !showConsole;
		}
		
	}
//================================================================================================================================================================	
	void OnGUI(){
		GUI.skin = consoleSkin;
		if(!showConsole){
			command = ""; // does the text field currently contain characters? if so, clear it.
			return;
		}
		if(showConsole){
			// draw the main part of the console window and its contents when the player opens the console.
			
			consoleRect = GUILayout.Window(0, consoleRect, ConsoleContent, "Game Version 0A.6.0.1C", GUILayout.MaxWidth(Screen.width), GUILayout.MinWidth(Screen.width));
			
			// Command checking and execution
			//============================================================================
			if(command == "testlight" && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return)){
				TestLight();
				command = ""; 
				scrollPosition.y = Mathf.Infinity;
				return;
			}
			if(command != DEFAULT && command != TESTLIGHT && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return)){
				Debug.LogError("> Invalid");
				command = "";
				scrollPosition.y = Mathf.Infinity;
				return;
			}
			if(command == "" && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return)){
				print ("> ");
				command = "";
				scrollPosition.y = Mathf.Infinity;
				return;
			}
			command = GUI.TextField(new Rect(0, 200, Screen.width, 16), command, 128);	
		}
	}
//================================================================================================================================================================
	// Console window content & console message catching.
	//============================================================================
	void ConsoleContent(int windowID){
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		for(int i = 0; i < entries.Count; i++){
			ConsoleMessage entry = entries[i];
			switch (entry.type){
				case LogType.Error : GUI.contentColor = Color.red;
					break;
				case LogType.Warning : GUI.contentColor = Color.yellow;
					break;
				default : GUI.contentColor = Color.cyan;
					break;
			}
			GUILayout.Label((entry.message));
		}
		GUILayout.EndScrollView();
		GUILayout.BeginHorizontal();
		GUILayout.EndHorizontal();
	}
	void HandleLog (string message, string stackTrace, LogType type){
		ConsoleMessage entry = new ConsoleMessage(message,stackTrace,type);
		entries.Add(entry);
	}
//================================================================================================================================================================
	// Command functions.
	//============================================================================
	void TestLight(){
		print ("> Created a testlight @: " + _characterController.transform.position.ToString());
		GameObject _testlight = new GameObject("Testlight");
		_testlight.AddComponent<Light>();
		_testlight.light.shadows = LightShadows.Hard;
		_testlight.light.range = 64.0f;
		_testlight.light.intensity = 0.5f;
		_testlight.transform.position = _characterController.transform.position + new Vector3(0.0f, 10.0f, 0.0f);
		scrollPosition.y = Mathf.Infinity; // scroll to the bottom of the scroll view.
		return;
	}
}
//================================================================================================================================================================
