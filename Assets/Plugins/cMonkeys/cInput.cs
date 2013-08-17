//#define Use_cInputGUI // Comment out this line to use your own GUI instead of cInput's built-in GUI.

#region Namespaces

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#endregion

/***********************************************************************
 *  cInput 2.5.3 by Ward Dewaele & Deozaan
 *  This script is NOT free, unlike Custom Inputmanager 1.x.
 *  Therefore the use of this script is strictly personal and 
 *  may not be spread without permission of me (Ward Dewaele).
 *  
 *  Any technical or license questions can be mailed
 *  to ward.dewaele@pandora.be, but read the 
 *  included help documents first please.
 ***********************************************************************/

public class cInput : MonoBehaviour {

	#region cInput Variables and Properties

	public static GUISkin cSkin {
		get {
			Debug.LogWarning("cInput.cSkin has been deprecated. Please use cGUI.cSkin.");
			return cGUI.cSkin;
		}
		set {
			Debug.LogWarning("cInput.cSkin has been deprecated. Please use cGUI.cSkin.");
			cGUI.cSkin = value;
		}
	} // DEPRECATED!
	public static float gravity = 3;
	public static float sensitivity = 3;
	public static float deadzone = 0.001f;
	public static bool scanning { get { return _scanning; } } // this is read-only
	public static int length {
		get {
			_cInputInit(); // if cInput doesn't exist, create it
			return _inputLength + 1;
		}
	} // this is read-only
	public static bool allowDuplicates {
		get {
			_cInputInit(); // if cInput doesn't exist, create it
			return _allowDuplicates;
		}
		set {
			_allowDuplicates = value;
			PlayerPrefs.SetString("cInput_dubl", value.ToString());
			_exAllowDuplicates = value.ToString();
		}
	}

	// Private variables
	private static bool _allowDuplicates = false;
	private static string[,] _defaultStrings = new string[99, 5];
	private static string[] _inputName = new string[99]; // name of the input action (e.g., "Jump")
	private static KeyCode[] _inputPrimary = new KeyCode[99]; // primary input assigned to action (e.g., "Space")
	private static KeyCode[] _modifierUsedPrimary = new KeyCode[99]; // modfier used on primary input
	private static KeyCode[] _inputSecondary = new KeyCode[99]; // secondary input assigned to action
	private static KeyCode[] _modifierUsedSecondary = new KeyCode[99]; // modfier used on secondary input
	private static List<KeyCode> _modifiers = new List<KeyCode>(); // list that holds the allowed modifiers
	private static List<int> _markedAsAxis = new List<int>(); // list that keeps track of which actions are used to make axis
	private static string[] _axisName = new string[99];
	private static string[] _axisPrimary = new string[99];
	private static string[] _axisSecondary = new string[99];
	private static float[] _individualAxisSens = new float[99]; // individual axis sensitivity settings
	private static float[] _individualAxisGrav = new float[99]; // individual axis gravity settings
	private static bool[] _invertAxis = new bool[99];
	private static int[,] _makeAxis = new int[99, 3];
	private static int _inputLength = -1;
	private static int _axisLength = -1;
	private static List<KeyCode> _forbiddenKeys = new List<KeyCode>();
	private static bool[] _virtAxis = new bool[99];

	private static bool[] _getKeyArray = new bool[99]; // values stored for GetKey function
	private static bool[] _getKeyDownArray = new bool[99]; // values stored for GetKeyDown
	private static bool[] _getKeyUpArray = new bool[99]; // values stored for GetKeyUp
	private static bool[] _axisTriggerArray = new bool[99]; // values that help to check if an axis is up or down
	private static float[] _getAxis = new float[99];
	private static float[] _getAxisRaw = new float[99];
	private static float[] _getAxisArray = new float[99];
	private static float[] _getAxisArrayRaw = new float[99];

	// which types of inputs to allow when assigning inputs to actions
	private static bool _allowMouseAxis = false;
	private static bool _allowMouseButtons = true;
	private static bool _allowJoystickButtons = true;
	private static bool _allowJoystickAxis = true;
	private static bool _allowKeyboard = true;

	private static int _numGamepads = 5; // number of gamepads supported by built-in Input Manager settings

	private Vector2 _scrollPosition;
	// these strings are set by ShowMenu() to customize the look of cInput's menu
	private static string _menuHeaderString = "label";
	private static string _menuActionsString = "box";
	private static string _menuInputsString = "box";
	private static string _menuButtonsString = "button";

	private static bool _scanning; // are we scanning inputs to make a new assignment?
	private static int _cScanIndex; // the index of the array for inputs
	private static int _cScanInput; // which input (primary or secondary)
	private static bool _cInputExists; // whether cInput is initialized or not
	private static bool _cKeysLoaded;

	// External saving related variables
	private static string _exAllowDuplicates;
	private static string _exAxis;
	private static string _exAxisInverted;
	private static string _exDefaults;
	private static string _exInputs;
	private static string _exCalibrations;
	private static bool _externalSaving = false;

	private static Dictionary<string, KeyCode> _string2Key = new Dictionary<string, KeyCode>();

	private static int[] _axisType = new int[10 * _numGamepads];
	// Note: this wastes one slot
	private static string[,] _joyStrings = new string[_numGamepads, 11];
	private static string[,] _joyStringsPos = new string[_numGamepads, 11];
	private static string[,] _joyStringsNeg = new string[_numGamepads, 11];

	#endregion // cInput Variables and Properties

	#region Awake/Start/Update functions

	void Awake() {
		DontDestroyOnLoad(this); // Keep this thing from getting destroyed if we change levels.
	}

	void Start() {
		_CreateDictionary();
		if (_externalSaving) {
			_LoadExternalInputs();
			//Debug.Log("cInput loaded inputs from external source.");
		} else {
			_LoadInputs();
			//Debug.Log("cInput settings loaded inputs from PlayerPrefs.");
		}

		AddModifier(KeyCode.None); // we need to initialize the modifiers with this one
	}

	void Update() {
		if (_scanning && _cScanInput == 0) {
			string _prim;
			string _sec;

			if (string.IsNullOrEmpty(_axisPrimary[_cScanIndex])) {
				_prim = _inputPrimary[_cScanIndex].ToString();
			} else {
				_prim = _axisPrimary[_cScanIndex];
			}

			if (string.IsNullOrEmpty(_axisSecondary[_cScanIndex])) {
				_sec = _inputSecondary[_cScanIndex].ToString();
			} else {
				_sec = _axisSecondary[_cScanIndex];
			}

			_ChangeKey(_cScanIndex, _inputName[_cScanIndex], _prim, _sec);

			_scanning = false;
		}

		if (!_scanning) {
			CheckInputs();
		}

		if (_cScanInput != 0) {
			_InputScans();
		}
	}

	#endregion

	public static void Init() {
		_cInputInit(); // if cInput doesn't exist, create it 
	}

	private static void _CreateDictionary() {
		if (_string2Key.Count == 0) { // don't create the dictionary more than once
			for (int i = (int)KeyCode.None; i < (int)KeyCode.Joystick4Button19 + 1; i++) {
				KeyCode key = (KeyCode)i;
				_string2Key.Add(key.ToString(), key);
			}

			// Create joystrings dictionaries
			for (int i = 1; i < _numGamepads; i++) {
				for (int j = 1; j <= 10; j++) {
					_joyStrings[i, j] = "Joy" + i + " Axis " + j;
					_joyStringsPos[i, j] = "Joy" + i + " Axis " + j + "+";
					_joyStringsNeg[i, j] = "Joy" + i + " Axis " + j + "-";
				}
			}
		}
	}

	public static void ForbidKey(KeyCode key) {
		_CreateDictionary();
		if (!_forbiddenKeys.Contains(key)) {
			_forbiddenKeys.Add(key);
		}
	}

	public static void ForbidKey(string keyString) {
		_CreateDictionary();
		KeyCode key = _ConvertString2Key(keyString);
		ForbidKey(key);
	}

	#region AddModifier and RemoveModifier functions

	/// <summary>Designates a key for use as a modifier.</summary>
	/// <param name="modifierKey">The KeyCode for the key to be used as a modifier.</param>
	public static void AddModifier(KeyCode modifierKey) {
		_modifiers.Add(modifierKey);
	}

	/// <summary>Designates a key for use as a modifier.</summary>
	/// <param name="modifier">The string name of the key to be used as a modifier.</param>
	public static void AddModifier(string modifier) {
		AddModifier(_ConvertString2Key(modifier));
	}

	/// <summary>Removes a key for use as a modifier.</summary>
	/// <param name="modifierKey">The KeyCode for the key which should no longer be used as a modifier.</param>
	public static void RemoveModifier(KeyCode modifierKey) {
		_modifiers.Remove(modifierKey);
	}

	/// <summary>Removes a key for use as a modifier.</summary>
	/// <param name="modifier">The string name of the key which should no longer be used as a modifier.</param>
	public static void RemoveModifier(string modifier) {
		RemoveModifier(_ConvertString2Key(modifier));
	}

	#endregion

	private static KeyCode _ConvertString2Key(string str) {
		if (String.IsNullOrEmpty(str) || _string2Key.Count == 0) { return KeyCode.None; }

		if (_string2Key.ContainsKey(str)) {
			KeyCode _key = _string2Key[str];
			return _key;
		} else {
			if (!_IsAxisValid(str)) {
				Debug.Log("cInput error: " + str + " is not a valid input.");
			}

			return KeyCode.None;
		}
	}

	#region SetKey functions

	// this is for compatibility with UnityScript which doesn't accept default parameters
	public static void SetKey(string action, string primary) {
		SetKey(action, primary, Keys.None, primary, Keys.None);
	}

	public static void SetKey(string action, string primary, string secondary) {
		SetKey(action, primary, secondary, primary, secondary);
	}

	// Defines a Key with the a modifier on the primary input
	public static void SetKey(string action, string primary, string secondary, string primaryModifier) {
		SetKey(action, primary, secondary, primaryModifier, secondary);
	}

	// Defines a Key with modifiers
	public static void SetKey(string action, string primary, string secondary, string primaryModifier, string secondaryModifier) {
		// make sure this key hasn't already been set
		if (_FindKeyByDescription(action) == -1) {
			int _num = _inputLength + 1;
			// make sure we pass valid values for the modifiers
			primaryModifier = (primaryModifier == Keys.None) ? primary : primaryModifier;
			secondaryModifier = (secondaryModifier == Keys.None) ? secondary : secondaryModifier;
			// actually set the key
			_SetDefaultKey(_num, action, primary, secondary, primaryModifier, secondaryModifier);
		} else {
			// skip this warning if we loaded from an external source or we already created the cInput object
			if (_externalSaving == false || GameObject.Find("cInput").GetComponent<cInput>() == null) {
				// Whoops! Key with this name already exists!
				//Debug.LogWarning("A key with the name of " + action + " already exists. You should use ChangeKey() if you want to change an existing key!");
			}
		}
	}

	private static void _SetDefaultKey(int _num, string _name, string _input1, string _input2, string pMod, string sMod) {
		_defaultStrings[_num, 0] = _name;
		_defaultStrings[_num, 1] = _input1;
		_defaultStrings[_num, 2] = (string.IsNullOrEmpty(_input2)) ? KeyCode.None.ToString() : _input2;
		_defaultStrings[_num, 3] = string.IsNullOrEmpty(pMod) ? _input1 : pMod;
		_defaultStrings[_num, 4] = string.IsNullOrEmpty(sMod) ? _input2 : sMod;

		if (_num > _inputLength) { _inputLength = _num; }

		_modifierUsedPrimary[_num] = _ConvertString2Key(_defaultStrings[_num, 3]);
		_modifierUsedSecondary[_num] = _ConvertString2Key(_defaultStrings[_num, 4]);
		_SetKey(_num, _name, _input1, _input2);
		_SaveDefaults();
	}

	private static void _SetKey(int _num, string _name, string _input1, string _input2 = "") {
		// input description 
		_inputName[_num] = _name;
		_axisPrimary[_num] = "";

		if (_string2Key.Count == 0) { return; }

		if (!string.IsNullOrEmpty(_input1)) {
			// enter keyboard input in the input  array
			KeyCode _keyCode1 = _ConvertString2Key(_input1);
			_inputPrimary[_num] = _keyCode1;

			// enter mouse and gamepad axis inputs in the inputstring array
			string axisName = _ChangeStringToAxisName(_input1);
			if (_input1 != axisName) {
				_axisPrimary[_num] = _input1;
			}
		}

		_axisSecondary[_num] = "";

		if (!string.IsNullOrEmpty(_input2)) {
			// enter input in the alt input  array
			KeyCode _keyCode2 = _ConvertString2Key(_input2);
			_inputSecondary[_num] = _keyCode2;

			// enter mouse and gamepad axis inputs in the inputstring array
			string axisName = _ChangeStringToAxisName(_input2);
			if (_input2 != axisName) {
				_axisSecondary[_num] = _input2;
			}
		}
	}

	#endregion

	#region SetAxis and SetAxisSensitivity & related functions

	#region Overloaded SetAxis Functions

	// overload method to allow you to set an axis with two inputs
	public static void SetAxis(string description, string negativeInput, string positiveInput) {
		SetAxis(description, negativeInput, positiveInput, sensitivity, gravity);
	}

	// overload method to allow you to set the sensitivity of the axis
	public static void SetAxis(string description, string negativeInput, string positiveInput, float axisSensitivity) {
		SetAxis(description, negativeInput, positiveInput, axisSensitivity, gravity);
	}

	// overload method to allow you to set an axis with only one input
	public static void SetAxis(string description, string input) {
		SetAxis(description, input, "-1", sensitivity, gravity);
	}

	// overload method to allow you to set an axis with only one input, and set sensitivity
	public static void SetAxis(string description, string input, float axisSensitivity) {
		SetAxis(description, input, "-1", axisSensitivity, gravity);
	}

	// overload method to allow you to set an axis with only one input, and set sensitivity and gravity
	public static void SetAxis(string description, string input, float axisSensitivity, float axisGravity) {
		SetAxis(description, input, "-1", axisSensitivity, axisGravity);
	}

	#endregion

	// This is the function that all other SetAxis overload methods call to actually set the axis
	public static void SetAxis(string description, string negativeInput, string positiveInput, float axisSensitivity, float axisGravity) {
		if (IsKeyDefined(negativeInput)) {
			int _num = _FindAxisByDescription(description); // overwrite existing axis of same name
			if (_num == -1) {
				// this axis doesn't exist, so make a new one
				_num = _axisLength + 1;
			}

			int posInput = -1; // -1 by default, which means no input.
			if (IsKeyDefined(positiveInput)) {
				posInput = _FindKeyByDescription(positiveInput);
				_markedAsAxis.Add(_FindKeyByDescription(positiveInput)); // add the actions in the marked list
				_markedAsAxis.Add(_FindKeyByDescription(negativeInput));
			} else if (positiveInput != "-1") {
				// the key isn't defined and we're not passing in -1 as a value, so there's a problem
				Debug.LogError("Can't define Axis named: " + description + ". Please define '" + positiveInput + "' with SetKey() first.");
				return; // break out of this function without trying to assign the axis
			}

			_SetAxis(_num, description, _FindKeyByDescription(negativeInput), posInput);
			_individualAxisSens[_num] = axisSensitivity;
			_individualAxisGrav[_num] = axisGravity;
		} else {
			Debug.LogError("Can't define Axis named: " + description + ". Please define '" + negativeInput + "' with SetKey() first.");
		}
	}

	private static void _SetAxis(int _num, string _description, int _negative, int _positive) {
		if (_num > _axisLength) {
			_axisLength = _num;
		}

		_invertAxis[_num] = false;
		_axisName[_num] = _description;
		_makeAxis[_num, 0] = _negative;
		_makeAxis[_num, 1] = _positive;
		_SaveAxis();
		_SaveAxInverted();
	}

	// this allows you to set the axis sensitivity directly (after the axis has been defined)
	public static void SetAxisSensitivity(string axisName, float sensitivity) {
		int axis = _FindAxisByDescription(axisName);
		if (axis == -1) {
			// axis not defined!
			Debug.LogError("Cannot set sensitivity of " + axisName + ". Have you defined this axis with SetAxis() yet?");
		} else {
			// axis has been defined
			_individualAxisSens[axis] = sensitivity;
		}
	}

	// this allows you to set the axis gravity directly (after the axis has been defined)
	public static void SetAxisGravity(string axisName, float gravity) {
		int axis = _FindAxisByDescription(axisName);
		if (axis == -1) {
			// axis not defined!
			Debug.LogError("Cannot set gravity of " + axisName + ". Have you defined this axis with SetAxis() yet?");
		} else {
			// axis has been defined
			_individualAxisGrav[axis] = gravity;
		}
	}

	#endregion

	#region Calibration functions

	public static void Calibrate() {
		string _saveCals = "";
		for (int i = 1; i < _numGamepads; i++) {
			for (int n = 1; n < 11; n++) {
				int index = 10 * (i - 1) + (n - 1);
				string _joystring = _joyStrings[i, n];
				float axis = Input.GetAxisRaw(_joystring);
				_axisType[index] = axis < -deadzone ? 1 :
					axis > deadzone ? -1 :
					0;
				_saveCals += _axisType[index] + "*";
				PlayerPrefs.SetString("cInput_saveCals", _saveCals);
				_exCalibrations = _saveCals;
			}
		}
	}

	private static float _GetCalibratedAxisInput(string description) {
		float rawValue = Input.GetAxis(_ChangeStringToAxisName(description));

		for (int i = 1; i < _numGamepads; i++) {
			for (int j = 1; j < 10; j++) {
				string joyPos = _joyStringsPos[i, j];
				string joyNeg = _joyStringsNeg[i, j];
				if (description == joyPos || description == joyNeg) {
					int index = 10 * (i - 1) + (j - 1);
					switch (_axisType[index]) {
						case 0: {
								return rawValue;
							}
						case 1: {
								return (rawValue + 1) / 2;
							}
						case -1: {
								return (rawValue - 1) / 2;
							}
					}
				}
			}
		}

		return rawValue;
	}

	#endregion

	#region ChangeKey functions

	public static void ChangeKey(string action, int input, bool mouseAx, bool mouseBut, bool joyAx, bool joyBut, bool keyb) {
		_cInputInit(); // if cInput doesn't exist, create it
		int _num = _FindKeyByDescription(action);
		_ScanForNewKey(_num, input, mouseAx, mouseBut, joyAx, joyBut, keyb);
	}

	#region overloaded ChangeKey(string) functions for UnityScript compatibility

	public static void ChangeKey(string action) {
		ChangeKey(action, 1, _allowMouseAxis, _allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, int input) {
		ChangeKey(action, input, _allowMouseAxis, _allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, int input, bool mouseAx) {
		ChangeKey(action, input, mouseAx, _allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, int input, bool mouseAx, bool mouseBut) {
		ChangeKey(action, input, mouseAx, mouseBut, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, int input, bool mouseAx, bool mouseBut, bool joyAx) {
		ChangeKey(action, input, mouseAx, mouseBut, joyAx, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, int input, bool mouseAx, bool mouseBut, bool joyAx, bool joyBut) {
		ChangeKey(action, input, mouseAx, mouseBut, joyAx, joyBut, _allowKeyboard);
	}

	#endregion

	// use an int with ChangeKey, useful in for loops for GUI
	public static void ChangeKey(int index, int input, bool mouseAx, bool mouseBut, bool joyAx, bool joyBut, bool keyb) {
		_cInputInit(); // if cInput doesn't exist, create it
		_ScanForNewKey(index, input, mouseAx, mouseBut, joyAx, joyBut, keyb);
	}

	#region overloaded ChangeKey(int) functions for UnityScript compatibility

	public static void ChangeKey(int index) {
		ChangeKey(index, 1, false, true, true, true, true);
	}

	public static void ChangeKey(int index, int input) {
		ChangeKey(index, input, false, true, true, true, true);
	}

	public static void ChangeKey(int index, int input, bool mouseAx) {
		ChangeKey(index, input, mouseAx, true, true, true, true);
	}

	public static void ChangeKey(int index, int input, bool mouseAx, bool mouseBut) {
		ChangeKey(index, input, mouseAx, mouseBut, true, true, true);
	}

	public static void ChangeKey(int index, int input, bool mouseAx, bool mouseBut, bool joyAx) {
		ChangeKey(index, input, mouseAx, mouseBut, joyAx, true, true);
	}

	public static void ChangeKey(int index, int input, bool mouseAx, bool mouseBut, bool joyAx, bool joyBut) {
		ChangeKey(index, input, mouseAx, mouseBut, joyAx, joyBut, true);
	}

	#endregion

	// this lets the dev directly change the key without waiting for the player to push buttons.
	public static void ChangeKey(string action, string primary, string secondary) {
		_cInputInit(); // if cInput doesn't exist, create it
		int _num = _FindKeyByDescription(action);

		_ChangeKey(_num, action, primary, secondary);
	}

	#region overloaded ChangeKey(string, primary, secondary) function for UnityScript compatibility)

	public static void ChangeKey(string action, string primary) {
		ChangeKey(action, primary, "");
	}

	#endregion

	private static void _ScanForNewKey(int num, int input = 1, bool mouseAx = false, bool mouseBut = true,
								bool joyAx = true, bool joyBut = true, bool keyb = true) {
		_allowMouseAxis = mouseAx;
		_allowMouseButtons = mouseBut;
		_allowJoystickButtons = joyBut;
		_allowJoystickAxis = joyAx;
		_allowKeyboard = keyb;

		_cScanInput = input;
		_cScanIndex = num;
		_scanning = true;
	}

	private static void _ChangeKey(int num, string action, string primary, string secondary = "") {
		_SetKey(num, action, primary, secondary);
		_SaveInputs();
	}

	#endregion

	#region _DefaultsExist, IsKeyDefined, and IsAxisDefined functions

	private static bool _DefaultsExist() {
		return (_defaultStrings.Length > 0) ? true : false;
	}

	public static bool IsKeyDefined(string keyName) {
		if (_FindKeyByDescription(keyName) >= 0) {
			return true;
		}

		// if we got here then no key or axis was found
		return false;
	}

	public static bool IsAxisDefined(string axisName) {
		if (_FindAxisByDescription(axisName) >= 0) {
			return true;
		}

		// if we got here then no key or axis was found
		return false;
	}

	#endregion

	#region CheckInputs function

	// this is the magic that updates the values for all the inputs in cInput
	private void CheckInputs() {
		bool input1 = false;
		bool input2 = false;
		bool axis1 = false;
		bool axis2 = false;
		float axFloat1 = 0f;
		float axFloat2 = 0f;

		for (int n = 0; n < _inputLength + 1; n++) {
			// handle inputs
			input1 = Input.GetKey(_inputPrimary[n]);
			input2 = Input.GetKey(_inputSecondary[n]);

			bool _pModPressed = false; // is the primary modifier currently being pressed?
			bool _sModPressed = false; // is the secondary modifier currently being pressed?
			bool _modifierPressed = false; // is any modifier currently being pressed?

			for (int i = 0; i < _modifiers.Count; i++) {
				if (Input.GetKey(_modifiers[i])) {
					_modifierPressed = true; // at least one modifier is active
					if (!_pModPressed && _modifiers[i] == _modifierUsedPrimary[n]) { _pModPressed = true; }
					if (!_sModPressed && _modifiers[i] == _modifierUsedSecondary[n]) { _pModPressed = true; }
				}
			}

			// These bools are used to determine if this key's modifier (if any) is being pushed.
			bool _primaryModifierPassed = false;
			bool _secondaryModifierPassed = false;
			/* These next two lines are realy ugly, so here's an explanation of the parts:
			 * (_modifierUsedPrimary[n] == _inputPrimary[n]) <-- means there is no modifier for this input
			 * (!_modifierPressed) <-- means there was no modifier key pushed
			 * (_modifierUsedPrimary[n] != _inputPrimary[n]) <-- means there is a modifier for this input
			 * (_pModPressed) <-- means the modifier for this input has been pushed.
			 * 
			 * So what this does is checks two things:
			 * If there's no modifier AND no modifier keys are being pressed, we're good to go.
			 * OR
			 * If there is a modifier AND the modifier key is being pressed, we're good to go.
			 * */
			if (((_modifierUsedPrimary[n] == _inputPrimary[n]) && !_modifierPressed) || ((_modifierUsedPrimary[n] != _inputPrimary[n]) && _pModPressed)) { _primaryModifierPassed = true; }
			if (((_modifierUsedSecondary[n] == _inputSecondary[n]) && !_modifierPressed) || (_modifierUsedSecondary[n] != _inputSecondary[n] && _sModPressed)) { _secondaryModifierPassed = true; }

			if (!string.IsNullOrEmpty(_axisPrimary[n])) {
				axis1 = true;
				axFloat1 = _GetCalibratedAxisInput(_axisPrimary[n]) * _PosOrNeg(_axisPrimary[n]);
			} else {
				axis1 = false;
				axFloat1 = 0f;
			}

			if (!string.IsNullOrEmpty(_axisSecondary[n])) {
				axis2 = true;
				axFloat2 = _GetCalibratedAxisInput(_axisSecondary[n]) * _PosOrNeg(_axisSecondary[n]);
			} else {
				axis2 = false;
				axFloat2 = 0f;
			}

			if ((input1 && _primaryModifierPassed) || (input2 && _secondaryModifierPassed) || (axis1 && axFloat1 > 0.1f) || (axis2 && axFloat2 > 0.1f)) {
				_getKeyArray[n] = true;
			} else {
				_getKeyArray[n] = false;
			}

			if ((Input.GetKeyDown(_inputPrimary[n]) && _primaryModifierPassed) || (Input.GetKeyDown(_inputSecondary[n]) && _secondaryModifierPassed)) {
				_getKeyDownArray[n] = true;
			} else if ((axis1 && axFloat1 > deadzone && !_axisTriggerArray[n]) ||
						(axis2 && axFloat2 > deadzone && !_axisTriggerArray[n])) {
				_axisTriggerArray[n] = true;
				_getKeyDownArray[n] = true;
			} else {
				_getKeyDownArray[n] = false;
			}

			if ((Input.GetKeyUp(_inputPrimary[n]) && _primaryModifierPassed) || (Input.GetKeyUp(_inputSecondary[n]) && _secondaryModifierPassed)) {
				_getKeyUpArray[n] = true;
			} else if ((axis1 && axFloat1 <= deadzone && _axisTriggerArray[n]) ||
						(axis2 && axFloat2 <= deadzone && _axisTriggerArray[n])) {
				_axisTriggerArray[n] = false;
				_getKeyUpArray[n] = true;
			} else {
				_getKeyUpArray[n] = false;
			}

			// handle axis
			if (axis1 && !_virtAxis[n]) {
				_getAxis[n] = axFloat1;
				_getAxisRaw[n] = axFloat1;
			}

			if (axis2 && !_virtAxis[n]) {
				_getAxis[n] = axFloat2;
				_getAxisRaw[n] = axFloat2;
			}

			if (axis1 && axis2 && !_virtAxis[n]) {
				if (axFloat1 > 0) {
					_getAxis[n] = axFloat1;
					_getAxisRaw[n] = axFloat1;
				}

				if (axFloat2 > 0) {
					_getAxis[n] = axFloat2;
					_getAxisRaw[n] = axFloat2;
				}

				if (axFloat1 > 0 && axFloat2 > 0) {
					_getAxis[n] = (axFloat1 + axFloat2) / 2;
					_getAxisRaw[n] = (axFloat1 + axFloat2) / 2;
				}
			}

			// sensitivity (not for axis)
			if ((input1) || (input2)) {
				_virtAxis[n] = true;
				_getAxis[n] += sensitivity * Time.deltaTime;
				_getAxisRaw[n] = 1;
				if (_getAxis[n] > 1) { _getAxis[n] = 1; }
				if ((axis1 && axFloat1 < deadzone) || (axis2 && axFloat2 < deadzone)) {
					_getAxisRaw[n] = 1;
				}
			}

			// gravity (not for axis)
			if (!input1 && !input2) {
				if ((axis1 && axFloat1 < deadzone) || (axis2 && axFloat2 < deadzone) || (!axis1 && !axis2)) {
					_getAxis[n] -= gravity * Time.deltaTime;
					_getAxisRaw[n] = 0;
					if (_getAxis[n] < 0) { _getAxis[n] = 0; }
				}
			}

			if (_getAxis[n] == 0) { _virtAxis[n] = false; }
		}

		// compile the axis (negative and positive)
		for (int n = 0; n <= _axisLength; n++) {
			int neg = _makeAxis[n, 0];
			int pos = _makeAxis[n, 1];
			if (_makeAxis[n, 1] == -1) {
				_getAxisArray[n] = _getAxis[neg];
				_getAxisArrayRaw[n] = _getAxisRaw[neg];
			} else {
				_getAxisArray[n] = _getAxis[pos] - _getAxis[neg];
				_getAxisArrayRaw[n] = _getAxisRaw[pos] - _getAxisRaw[neg];
			}

			if (input1 || input2) {
				// apply individual axis sensitivity
				_getAxisArray[n] *= _individualAxisSens[n] * Time.deltaTime;
			}

			if (!input1 && !input2) {
				// apply individual axis gravity
				_getAxisArray[n] = Mathf.Clamp01(Mathf.Abs(_getAxisArray[n]) - _individualAxisGrav[n] * Time.deltaTime) * Mathf.Sign(_getAxisArray[n]);
			}
		}
	}

	#endregion

	#region GetKey, GetAxis, GetText, and related functions

	#region GetKey functions

	// returns -1 only if there was an error
	private static int _FindKeyByDescription(string description) {
		for (int i = 0; i < _inputName.Length; i++) {
			if (_inputName[i] == description) {
				return i;
			}
		}

		// uh oh, the string didn't match!
		return -1;
	}

	// Returns true if the key is currently being pressed (continual)
	public static bool GetKey(string description) {
		if (!_DefaultsExist()) {
			Debug.LogError("No default inputs found. Please setup your default inputs with SetKey first.");
			return false;
		}

		_cInputInit(); // if cInput doesn't exist, create it
		if (!_cKeysLoaded) { return false; } // make sure we've saved/loaded keys before trying to access them.
		int _index = _FindKeyByDescription(description);

		if (_index > -1) {
			return _getKeyArray[_index];
		} else {
			// if we got this far then the string didn't match and there's a problem
			Debug.LogError("Couldn't find a key match for " + description + ". Is it possible you typed it wrong or forgot to setup your defaults after making changes?");
			return false;
		}
	}

	// Returns true just once when the key is first pressed down
	public static bool GetKeyDown(string description) {
		if (!_DefaultsExist()) {
			Debug.LogError("No default inputs found. Please setup your default inputs with SetKey first.");
			return false;
		}

		_cInputInit(); // if cInput doesn't exist, create it
		if (!_cKeysLoaded) { return false; } // make sure we've saved/loaded keys before trying to access them.
		int _index = _FindKeyByDescription(description);

		if (_index > -1) {
			return _getKeyDownArray[_FindKeyByDescription(description)];
		} else {
			// if we got this far then the string didn't match and there's a problem
			Debug.LogError("Couldn't find a key match for " + description + ". Is it possible you typed it wrong or forgot to setup your defaults after making changes?");
			return false;
		}
	}

	// Returns true just once when the key is released
	public static bool GetKeyUp(string description) {
		if (!_DefaultsExist()) {
			Debug.LogError("No default inputs found. Please setup your default inputs with SetKey first.");
			return false;
		}

		_cInputInit(); // if cInput doesn't exist, create it
		if (!_cKeysLoaded) { return false; } // make sure we've saved/loaded keys before trying to access them.
		int _index = _FindKeyByDescription(description);

		if (_index > -1) {
			return _getKeyUpArray[_FindKeyByDescription(description)];
		} else {
			// if we got this far then the string didn't match and there's a problem
			Debug.LogError("Couldn't find a key match for " + description + ". Is it possible you typed it wrong or forgot to setup your defaults after making changes?");
			return false;
		}
	}

	#region GetButton functions -- they just call GetKey functions

	public static bool GetButton(string description) {
		return GetKey(description);
	}

	public static bool GetButtonDown(string description) {
		return GetKeyDown(description);
	}

	public static bool GetButtonUp(string description) {
		return GetKeyUp(description);
	}

	#endregion

	#endregion

	#region GetAxis and related functions

	private static int _FindAxisByDescription(string axisName) {
		for (int i = 0; i < _axisName.Length; i++) {
			if (_axisName[i] == axisName) {
				return i;
			}
		}

		return -1; // uh oh, the string didn't match!
	}

	public static float GetAxis(string axisName) {
		_cInputInit(); // if cInput doesn't exist, create it
		if (!_DefaultsExist()) {
			Debug.LogError("No default inputs found. Please setup your default inputs with SetKey first.");
			return 0;
		}

		int index = _FindAxisByDescription(axisName);
		if (index > -1) {
			if (_invertAxis[index]) {
				// this axis should be inverted, so invert the value!
				return _getAxisArray[index] * -1;
			} else {
				// this axis is normal, return the normal value
				return _getAxisArray[index];
			}
		}

		// if we got this far then the string didn't match and there's a problem
		Debug.LogError("Couldn't find an axis match for " + axisName + ". Is it possible you typed it wrong?");
		return 0;
	}

	public static float GetAxisRaw(string axisName) {
		_cInputInit(); // if cInput doesn't exist, create it
		if (!_DefaultsExist()) {
			Debug.LogError("No default inputs found. Please setup your default inputs with SetKey first.");
			return 0;
		}

		int index = _FindAxisByDescription(axisName);
		if (index > -1) {
			if (_invertAxis[index]) {
				// this axis should be inverted, so invert the value!
				return _getAxisArrayRaw[index] * -1;
			} else {
				// this axis is normal, return the normal value
				return _getAxisArrayRaw[index];
			}
		}

		// if we got this far then the string didn't match and there's a problem
		Debug.LogError("Couldn't find an axis match for " + axisName + ". Is it possible you typed it wrong?");
		return 0;
	}

	#endregion

	#region GetText, _ChangeStringToAxisName, _PosOrNeg functions

	#region overloaded GetText(string) and GetText(int) functions for UnityScript compatibility

	public static string GetText(string action) {
		return GetText(action, 1);
	}

	public static string GetText(int index) {
		return GetText(index, 0);
	}

	#endregion

	public static string GetText(string action, int input) {
		int index = _FindKeyByDescription(action);
		return GetText(index, input);
	}

	// use an int to getText of input assignments. Useful in for loops for GUIs.
	public static string GetText(int index, int input) {
		_cInputInit(); // if cInput doesn't exist, create it
		// make sure a valid value is passed in
		if (input < 0 || input > 2) {
			Debug.LogWarning("Can't look for text #" + input + " for " + _inputName[index] + " input. Only 0, 1, or 2 is acceptable. Clamping to this range.");
			input = Mathf.Clamp(input, 0, 2);
		}

		string name;

		if (input == 1) {
			if (!string.IsNullOrEmpty(_axisPrimary[index])) {
				name = _axisPrimary[index];
			} else {
				string _prefix = "";
				// if modifier is not empty and isn't the same as the key, and the key isn't empty
				if (_modifierUsedPrimary[index] != KeyCode.None && _modifierUsedPrimary[index] != _inputPrimary[index] && _inputPrimary[index] != KeyCode.None) {
					_prefix = _modifierUsedPrimary[index].ToString() + " + ";
				}
				name = _prefix + _inputPrimary[index].ToString();
			}
		} else if (input == 2) {
			if (!string.IsNullOrEmpty(_axisSecondary[index])) {
				name = _axisSecondary[index];
			} else {
				string _prefix = "";
				// if modifier is not empty and isn't the same as the key, and the key isn't empty
				if (_modifierUsedSecondary[index] != KeyCode.None && _modifierUsedSecondary[index] != _inputSecondary[index] && _inputSecondary[index] != KeyCode.None) {
					_prefix = _modifierUsedSecondary[index].ToString() + " + ";
				}
				name = _prefix + _inputSecondary[index].ToString();
			}
		} else {
			name = _inputName[index];
			return name;
		}

		// check to see if this key is currently waiting to be reassigned
		if (_scanning && (index == _cScanIndex) && (input == _cScanInput)) {
			name = ". . .";
		}

		return name;
	}

	private static string _ChangeStringToAxisName(string description) {
		// First we need to change the name of some of these things. . .
		switch (description) {
			case "Mouse Left": { return "Mouse Horizontal"; }
			case "Mouse Right": { return "Mouse Horizontal"; }
			case "Mouse Up": { return "Mouse Vertical"; }
			case "Mouse Down": { return "Mouse Vertical"; }
			case "Mouse Wheel Up": { return "Mouse Wheel"; }
			case "Mouse Wheel Down": { return "Mouse Wheel"; }
		}

		string joystring = _FindJoystringByDescription(description);
		if (joystring != null) {
			return joystring;
		}

		return description;
	}


	private static string _FindJoystringByDescription(string desc) {
		for (int i = 1; i < _numGamepads; i++) {
			for (int j = 1; j <= 10; j++) {
				string joyPos = _joyStringsPos[i, j];
				string joyNeg = _joyStringsNeg[i, j];
				if (desc == joyPos || desc == joyNeg) {
					return _joyStrings[i, j];
				}
			}
		}

		return null;
	}

	private static bool _IsAxisValid(string _ax) {
		switch (_ax) {
			case "Mouse Left": { return true; }
			case "Mouse Right": { return true; }
			case "Mouse Up": { return true; }
			case "Mouse Down": { return true; }
			case "Mouse Wheel Up": { return true; }
			case "Mouse Wheel Down": { return true; }
		}

		bool _state = false;
		for (int i = 1; i < _numGamepads; i++) {
			for (int j = 1; j <= 10; j++) {
				string joyPos = _joyStringsPos[i, j];
				string joyNeg = _joyStringsNeg[i, j];
				if (_ax == joyPos || _ax == joyNeg) {
					_state = true;
				}
			}
		}

		return _state;
	}

	// This function returns -1 for negative axes
	private static int _PosOrNeg(string description) {
		int posneg = 1;

		switch (description) {
			case "Mouse Left": { return -1; }
			case "Mouse Right": { return 1; }
			case "Mouse Up": { return 1; }
			case "Mouse Down": { return -1; }
			case "Mouse Wheel Up": { return 1; }
			case "Mouse Wheel Down": { return -1; }
		}

		for (int i = 1; i < _numGamepads; i++) {
			for (int j = 1; j < 10; j++) {
				string joyPos = _joyStringsPos[i, j];
				string joyNeg = _joyStringsNeg[i, j];
				if (description == joyPos) {
					return 1;
				} else if (description == joyNeg) {
					return -1;
				}
			}
		}

		return posneg;
	}

	#endregion

	#endregion

	#region Save, Load, Reset & Clear functions

	private static void _SaveAxis() {
		int _num = _axisLength + 1;
		string _axName = "";
		string _axNeg = "";
		string _axPos = "";
		string _indAxSens = "";
		string _indAxGrav = "";
		for (int n = 0; n < _num; n++) {
			_axName += _axisName[n] + "*";
			_axNeg += _makeAxis[n, 0] + "*";
			_axPos += _makeAxis[n, 1] + "*";
			_indAxSens += _individualAxisSens[n] + "*";
			_indAxGrav += _individualAxisGrav[n] + "*";
		}

		string _axis = _axName + "#" + _axNeg + "#" + _axPos + "#" + _num;
		PlayerPrefs.SetString("cInput_axis", _axis);
		PlayerPrefs.SetString("cInput_indAxSens", _indAxSens);
		PlayerPrefs.SetString("cInput_indAxGrav", _indAxGrav);
		_exAxis = _axis + "¿" + _indAxSens + "¿" + _indAxGrav;
	}

	private static void _SaveAxInverted() {
		int _num = _axisLength + 1;
		string _axInv = "";

		for (int n = 0; n < _num; n++) {
			_axInv += _invertAxis[n] + "*";
		}

		PlayerPrefs.SetString("cInput_axInv", _axInv);
		_exAxisInverted = _axInv;
	}

	private static void _SaveDefaults() {
		// saving default inputs
		int _num = _inputLength + 1;
		string _defName = "";
		string _def1 = "";
		string _def2 = "";
		string _defmod1 = "";
		string _defmod2 = "";
		for (int n = 0; n < _num; n++) {

			_defName += _defaultStrings[n, 0] + "*";
			_def1 += _defaultStrings[n, 1] + "*";
			_def2 += _defaultStrings[n, 2] + "*";
			_defmod1 += _defaultStrings[n, 3] + "*";
			_defmod2 += _defaultStrings[n, 4] + "*";
		}

		string _Default = _defName + "#" + _def1 + "#" + _def2 + "#" + _defmod1 + "#" + _defmod2;
		PlayerPrefs.SetInt("cInput_count", _num);
		PlayerPrefs.SetString("cInput_defaults", _Default);
		_exDefaults = _num + "¿" + _Default;
	}

	private static void _SaveInputs() {
		int _num = _inputLength + 1;
		// *** save input configuration ***
		string _descr = "";
		string _inp = "";
		string _alt_inp = "";
		string _inpStr = "";
		string _alt_inpStr = "";
		string _modifierStr = "";
		string _alt_modifierStr = "";

		for (int n = 0; n < _num; n++) {
			// make the strings
			_descr += _inputName[n] + "*";
			_inp += _inputPrimary[n] + "*";
			_alt_inp += _inputSecondary[n] + "*";
			_inpStr += _axisPrimary[n] + "*";
			_alt_inpStr += _axisSecondary[n] + "*";
			_modifierStr += _modifierUsedPrimary[n] + "*";
			_alt_modifierStr += _modifierUsedSecondary[n] + "*";
		}

		// save the strings to the PlayerPrefs
		PlayerPrefs.SetString("cInput_descr", _descr);
		PlayerPrefs.SetString("cInput_inp", _inp);
		PlayerPrefs.SetString("cInput_alt_inp", _alt_inp);
		PlayerPrefs.SetString("cInput_inpStr", _inpStr);
		PlayerPrefs.SetString("cInput_alt_inpStr", _alt_inpStr);
		PlayerPrefs.SetString("cInput_modifierStr", _modifierStr);
		PlayerPrefs.SetString("cInput_alt_modifierStr", _alt_modifierStr);
		_exInputs = _descr + "¿" + _inp + "¿" + _alt_inp + "¿" + _inpStr + "¿" + _alt_inpStr + "¿" + _modifierStr + "¿" + _alt_modifierStr;
	}

	public static string externalInputs {
		get {
			return _exAllowDuplicates + "æ" + _exAxis + "æ" + _exAxisInverted + "æ" + _exDefaults + "æ" + _exInputs + "æ" + _exCalibrations;
			//string tmpExternalString = _exAllowDuplicates + "æ" + _exAxis + "æ" + _exAxisInverted + "æ" + _exDefaults + "æ" + _exInputs + "æ" + _exCalibrations;
			//return tmpExternalString;
		}
	}

	public static void LoadExternal(string externString) {
		string[] tmpExternalStrings = externString.Split('æ');
		_exAllowDuplicates = tmpExternalStrings[0];
		_exAxis = tmpExternalStrings[1];
		_exAxisInverted = tmpExternalStrings[2];
		_exDefaults = tmpExternalStrings[3];
		_exInputs = tmpExternalStrings[4];
		_exCalibrations = tmpExternalStrings[5];
		_LoadExternalInputs();
	}

	private static void _LoadInputs() {
		if (!PlayerPrefs.HasKey("cInput_count")) { return; }
		if (PlayerPrefs.HasKey("cInput_dubl")) {
			if (PlayerPrefs.GetString("cInput_dubl") == "True") {
				allowDuplicates = true;
			} else {
				allowDuplicates = false;
			}
		}

		int _count = PlayerPrefs.GetInt("cInput_count");
		_inputLength = _count - 1;

		string _defaults = PlayerPrefs.GetString("cInput_defaults");
		string[] ar_defs = _defaults.Split('#');
		string[] ar_defName = ar_defs[0].Split('*');
		string[] ar_defPrime = ar_defs[1].Split('*');
		string[] ar_defSec = ar_defs[2].Split('*');
		string[] ar_modPrime = ar_defs[3].Split('*');
		string[] ar_modSec = ar_defs[4].Split('*');

		for (int n = 0; n < ar_defName.Length - 1; n++) {
			_SetDefaultKey(n, ar_defName[n], ar_defPrime[n], ar_defSec[n], ar_modPrime[n], ar_modSec[n]);
		}

		if (PlayerPrefs.HasKey("cInput_inp")) {
			string _descr = PlayerPrefs.GetString("cInput_descr");
			string _inp = PlayerPrefs.GetString("cInput_inp");
			string _alt_inp = PlayerPrefs.GetString("cInput_alt_inp");
			string _inpStr = PlayerPrefs.GetString("cInput_inpStr");
			string _alt_inpStr = PlayerPrefs.GetString("cInput_alt_inpStr");
			string _modifierStr = PlayerPrefs.GetString("cInput_modifierStr");
			string _alt_modifierStr = PlayerPrefs.GetString("cInput_alt_modifierStr");

			string[] ar_descr = _descr.Split('*');
			string[] ar_inp = _inp.Split('*');
			string[] ar_alt_inp = _alt_inp.Split('*');
			string[] ar_inpStr = _inpStr.Split('*');
			string[] ar_alt_inpStr = _alt_inpStr.Split('*');
			string[] ar_modifierStr = _modifierStr.Split('*');
			string[] ar_alt_modifierStr = _alt_modifierStr.Split('*');

			for (int n = 0; n < ar_descr.Length - 1; n++) {
				if (ar_descr[n] == _defaultStrings[n, 0]) {
					_inputName[n] = ar_descr[n];
					_inputPrimary[n] = _ConvertString2Key(ar_inp[n]);
					_inputSecondary[n] = _ConvertString2Key(ar_alt_inp[n]);
					_axisPrimary[n] = ar_inpStr[n];
					_axisSecondary[n] = ar_alt_inpStr[n];
					_modifierUsedPrimary[n] = _ConvertString2Key(ar_modifierStr[n]);
					_modifierUsedSecondary[n] = _ConvertString2Key(ar_alt_modifierStr[n]);
				}
			}

			// fixes inputs when defaults are being changed
			for (int m = 0; m < ar_defName.Length - 1; m++) {
				for (int n = 0; n < ar_descr.Length - 1; n++) {
					if (ar_descr[n] == _defaultStrings[m, 0]) {
						_inputName[m] = ar_descr[n];
						_inputPrimary[m] = _ConvertString2Key(ar_inp[n]);
						_inputSecondary[m] = _ConvertString2Key(ar_alt_inp[n]);
						_axisPrimary[m] = ar_inpStr[n];
						_axisSecondary[m] = ar_alt_inpStr[n];
						_modifierUsedPrimary[n] = _ConvertString2Key(ar_modifierStr[n]);
						_modifierUsedSecondary[n] = _ConvertString2Key(ar_alt_modifierStr[n]);
					}
				}
			}
		}

		if (PlayerPrefs.HasKey("cInput_axis")) {

			string _invAx = PlayerPrefs.GetString("cInput_axInv");
			string[] _axInv = _invAx.Split('*');
			string _ax = PlayerPrefs.GetString("cInput_axis");

			string[] _axis = _ax.Split('#');
			string[] _axName = _axis[0].Split('*');
			string[] _axNeg = _axis[1].Split('*');
			string[] _axPos = _axis[2].Split('*');

			int _axCount = int.Parse(_axis[3]);
			for (int n = 0; n < _axCount; n++) {
				int _neg = int.Parse(_axNeg[n]);
				int _pos = int.Parse(_axPos[n]);
				_SetAxis(n, _axName[n], _neg, _pos);
				if (_axInv[n] == "True") {
					_invertAxis[n] = true;
				} else {
					_invertAxis[n] = false;
				}
			}
		}

		if (PlayerPrefs.HasKey("cInput_indAxSens")) {
			string _tmpAxisSens = PlayerPrefs.GetString("cInput_indAxSens");
			string[] _arrAxisSens = _tmpAxisSens.Split('*');
			for (int n = 0; n < _arrAxisSens.Length - 1; n++) {
				_individualAxisSens[n] = float.Parse(_arrAxisSens[n]);
			}
		}

		if (PlayerPrefs.HasKey("cInput_indAxGrav")) {
			string _tmpAxisGrav = PlayerPrefs.GetString("cInput_indAxGrav");
			string[] _arrAxisGrav = _tmpAxisGrav.Split('*');
			for (int n = 0; n < _arrAxisGrav.Length - 1; n++) {
				_individualAxisGrav[n] = float.Parse(_arrAxisGrav[n]);
			}
		}

		// calibration loading
		if (PlayerPrefs.HasKey("cInput_saveCals")) {
			string _saveCals = PlayerPrefs.GetString("cInput_saveCals");
			string[] _saveCalsArr = _saveCals.Split('*');
			for (int n = 1; n <= _saveCalsArr.Length - 2; n++) {
				_axisType[n] = int.Parse(_saveCalsArr[n]);
			}
		}

		_cKeysLoaded = true;

	}

	private static void _LoadExternalInputs() {
		_externalSaving = true;
		// splitting the external strings
		string[] _es1 = _exAxis.Split('¿');
		string[] _es3 = _exDefaults.Split('¿');
		string[] _es4 = _exInputs.Split('¿');

		if (_exAllowDuplicates == "True") {
			allowDuplicates = true;
		} else {
			allowDuplicates = false;
		}
		int _count = int.Parse(_es3[0]);
		_inputLength = _count - 1;

		string _defaults = _es3[1];
		string[] ar_defs = _defaults.Split('#');
		string[] ar_defName = ar_defs[0].Split('*');
		string[] ar_defPrime = ar_defs[1].Split('*');
		string[] ar_defSec = ar_defs[2].Split('*');
		string[] ar_modPrime = ar_defs[3].Split('*');
		string[] ar_modSec = ar_defs[4].Split('*');

		for (int n = 0; n < ar_defName.Length - 1; n++) {
			_SetDefaultKey(n, ar_defName[n], ar_defPrime[n], ar_defSec[n], ar_modPrime[n], ar_modSec[n]);
		}

		if (!string.IsNullOrEmpty(_es4[0])) {
			string _descr = _es4[0];
			string _inp = _es4[1];
			string _alt_inp = _es4[2];
			string _inpStr = _es4[3];
			string _alt_inpStr = _es4[4];
			string _modifierStr = _es4[5];
			string _alt_modifierStr = _es4[6];

			string[] ar_descr = _descr.Split('*');
			string[] ar_inp = _inp.Split('*');
			string[] ar_alt_inp = _alt_inp.Split('*');
			string[] ar_inpStr = _inpStr.Split('*');
			string[] ar_alt_inpStr = _alt_inpStr.Split('*');
			string[] ar_modifierStr = _modifierStr.Split('*');
			string[] ar_alt_modifierStr = _alt_modifierStr.Split('*');

			for (int n = 0; n < ar_descr.Length - 1; n++) {
				if (ar_descr[n] == _defaultStrings[n, 0]) {
					_inputName[n] = ar_descr[n];
					_inputPrimary[n] = _ConvertString2Key(ar_inp[n]);
					_inputSecondary[n] = _ConvertString2Key(ar_alt_inp[n]);
					_axisPrimary[n] = ar_inpStr[n];
					_axisSecondary[n] = ar_alt_inpStr[n];
					_modifierUsedPrimary[n] = _ConvertString2Key(ar_modifierStr[n]);
					_modifierUsedSecondary[n] = _ConvertString2Key(ar_alt_modifierStr[n]);
				}
			}

			// fixes inputs when defaults are being changed
			for (int m = 0; m < ar_defName.Length - 1; m++) {
				for (int n = 0; n < ar_descr.Length - 1; n++) {
					if (ar_descr[n] == _defaultStrings[m, 0]) {
						_inputName[m] = ar_descr[n];
						_inputPrimary[m] = _ConvertString2Key(ar_inp[n]);
						_inputSecondary[m] = _ConvertString2Key(ar_alt_inp[n]);
						_axisPrimary[m] = ar_inpStr[n];
						_axisSecondary[m] = ar_alt_inpStr[n];
						_modifierUsedPrimary[n] = _ConvertString2Key(ar_modifierStr[n]);
						_modifierUsedSecondary[n] = _ConvertString2Key(ar_alt_modifierStr[n]);
					}
				}
			}
		}

		if (!string.IsNullOrEmpty(_es1[0])) {

			string _invAx = _exAxisInverted;
			string[] _axInv = _invAx.Split('*');
			string _ax = _es1[0];

			string[] _axis = _ax.Split('#');
			string[] _axName = _axis[0].Split('*');
			string[] _axNeg = _axis[1].Split('*');
			string[] _axPos = _axis[2].Split('*');

			int _axCount = int.Parse(_axis[3]);
			for (int n = 0; n < _axCount; n++) {
				int _neg = int.Parse(_axNeg[n]);
				int _pos = int.Parse(_axPos[n]);
				_SetAxis(n, _axName[n], _neg, _pos);
				if (_axInv[n] == "true") {
					_invertAxis[n] = true;
				} else {
					_invertAxis[n] = false;
				}
			}
		}

		if (!string.IsNullOrEmpty(_es1[1])) {
			string _tmpAxisSens = _es1[1];
			string[] _arrAxisSens = _tmpAxisSens.Split('*');
			for (int n = 0; n < _arrAxisSens.Length - 1; n++) {
				_individualAxisSens[n] = float.Parse(_arrAxisSens[n]);
			}
		}

		if (!string.IsNullOrEmpty(_es1[2])) {
			string _tmpAxisGrav = _es1[2];
			string[] _arrAxisGrav = _tmpAxisGrav.Split('*');
			for (int n = 0; n < _arrAxisGrav.Length - 1; n++) {
				_individualAxisGrav[n] = float.Parse(_arrAxisGrav[n]);
			}
		}

		// calibration loading
		if (!string.IsNullOrEmpty(_exCalibrations)) {
			string _saveCals = _exCalibrations;
			string[] _saveCalsArr = _saveCals.Split('*');
			for (int n = 1; n <= _saveCalsArr.Length - 2; n++) {
				_axisType[n] = int.Parse(_saveCalsArr[n]);
			}
		}

		_cKeysLoaded = true;
	}

	public static void ResetInputs() {
		_cInputInit(); // if cInput doesn't exist, create it
		// reset inputs to default values
		for (int n = 0; n < _inputLength + 1; n++) {
			_SetKey(n, _defaultStrings[n, 0], _defaultStrings[n, 1], _defaultStrings[n, 2]);
			_modifierUsedPrimary[n] = _ConvertString2Key(_defaultStrings[n, 3]);
			_modifierUsedSecondary[n] = _ConvertString2Key(_defaultStrings[n, 4]);
		}

		for (int n = 0; n < _axisLength; n++) {
			_invertAxis[n] = false;
		}

		Clear();
		_SaveDefaults();
		_SaveInputs();
		_SaveAxInverted();
	}

	public static void Clear() {
		PlayerPrefs.DeleteKey("cInput_axInv");
		PlayerPrefs.DeleteKey("cInput_axis");
		PlayerPrefs.DeleteKey("cInput_indAxSens");
		PlayerPrefs.DeleteKey("cInput_indAxGrav");
		PlayerPrefs.DeleteKey("cInput_count");
		PlayerPrefs.DeleteKey("cInput_defaults");
		PlayerPrefs.DeleteKey("cInput_descr");
		PlayerPrefs.DeleteKey("cInput_inp");
		PlayerPrefs.DeleteKey("cInput_alt_inp");
		PlayerPrefs.DeleteKey("cInput_inpStr");
		PlayerPrefs.DeleteKey("cInput_alt_inpStr");
		PlayerPrefs.DeleteKey("cInput_dubl");
		PlayerPrefs.DeleteKey("cInput_saveCals");
		PlayerPrefs.DeleteKey("cInput_modifierStr");
		PlayerPrefs.DeleteKey("cInput_alt_modifierStr");
	}

	#endregion

	#region InvertAxis and IsAxisInverted functions

	// this sets the inversion of axisName to invertedStatus
	public static bool AxisInverted(string axisName, bool invertedStatus) {
		_cInputInit(); // if cInput doesn't exist, create it
		int index = _FindAxisByDescription(axisName);
		if (index > -1) {
			_invertAxis[index] = invertedStatus;
			_SaveAxInverted();
			return invertedStatus;
		}

		// if we got this far then the string didn't match and there's a problem.
		Debug.LogWarning("Couldn't find an axis match for " + axisName + " while trying to set inversion status. Is it possible you typed it wrong?");
		return false;
	}

	// this just returns inversion status of axisName
	public static bool AxisInverted(string axisName) {
		_cInputInit(); // if cInput doesn't exist, create it
		int index = _FindAxisByDescription(axisName);
		if (index > -1) {
			return _invertAxis[index];
		}

		// if we got this far then the string didn't match and there's a problem.
		Debug.LogWarning("Couldn't find an axis match for " + axisName + " while trying to get inversion status. Is it possible you typed it wrong?");
		return false;
	}

	#endregion

	#region ShowMenu functions

	public static bool ShowMenu() {
		Debug.LogError("cInput.ShowMenu() has been deprecated. Please use the appropriate cGUI variable, such as cGUI.showingAnyGUI");
		return false;
	}

	#region overloaded ShowMenu functions

	public static void ShowMenu(bool state) {
		ShowMenu(state, _menuHeaderString, _menuActionsString, _menuInputsString, _menuButtonsString);
	}

	public static void ShowMenu(bool state, string menuHeader) {
		ShowMenu(state, menuHeader, _menuActionsString, _menuInputsString, _menuButtonsString);
	}

	public static void ShowMenu(bool state, string menuHeader, string menuActions) {
		ShowMenu(state, menuHeader, menuActions, _menuInputsString, _menuButtonsString);
	}

	public static void ShowMenu(bool state, string menuHeader, string menuActions, string menuInputs) {
		ShowMenu(state, menuHeader, menuActions, menuInputs, _menuButtonsString);
	}

	#endregion overloaded showMenu functions

	// this is an old method of showing the menu, it's just in for backwards compatibility - 
	public static void ShowMenu(bool state, string menuHeader, string menuActions, string menuInputs, string menuButtons) {
		Debug.LogError("cInput.ShowMenu() has been deprecated. Please use the appropriate cGUI function, such as cGUI.ToggleGUI()");
	}

	#endregion

	private static void _cInputInit() {
		if (!_cInputExists) {
			GameObject cObject;
			if (GameObject.Find("cObject")) {
				// GameObject named cObject already exists
				cObject = GameObject.Find("cObject");
			} else {
				// We need to create a GameObject named cObject
				cObject = new GameObject();
				cObject.name = "cObject";
			}

			_cInputExists = true;

			// make sure the GameObject also has the cInput component attached
			if (cObject.GetComponent<cInput>() == null) {
				cObject.AddComponent<cInput>();
			}

#if Use_cInputGUI

			// make sure the GameObject also has the cInputGUI component attached
			if (cObject.GetComponent<cInputGUI>() == null) {
				cObject.AddComponent<cInputGUI>();
			}

#endif

		}
	}

	private void _CheckingDuplicates(int _num, int _count) {
		if (allowDuplicates) { return; }

		for (int n = 0; n < length; n++) {
			if (_count == 1) {
				if (_num != n && _inputPrimary[_num] == _inputPrimary[n] && _modifierUsedPrimary[_num] == _modifierUsedPrimary[n]) {
					_inputPrimary[n] = KeyCode.None;
				}

				if (_inputPrimary[_num] == _inputSecondary[n] && _modifierUsedPrimary[_num] == _modifierUsedSecondary[n]) {
					_inputSecondary[n] = KeyCode.None;
				}
			}

			if (_count == 2) {
				if (_inputSecondary[_num] == _inputPrimary[n] && _modifierUsedSecondary[_num] == _modifierUsedPrimary[n]) {
					_inputPrimary[n] = KeyCode.None;
				}

				if (_num != n && _inputSecondary[_num] == _inputSecondary[n] && _modifierUsedSecondary[_num] == _modifierUsedSecondary[n]) {
					_inputSecondary[n] = KeyCode.None;
				}
			}
		}
	}

	private void _CheckingDuplicateStrings(int _num, int _count) {
		if (allowDuplicates) { return; }

		for (int n = 0; n < length; n++) {
			if (_count == 1) {
				if (_num != n && _axisPrimary[_num] == _axisPrimary[n]) {
					_axisPrimary[n] = "";
					_inputPrimary[n] = KeyCode.None;
				}

				if (_axisPrimary[_num] == _axisSecondary[n]) {
					_axisSecondary[n] = "";
					_inputSecondary[n] = KeyCode.None;
				}
			}

			if (_count == 2) {
				if (_axisSecondary[_num] == _axisPrimary[n]) {
					_axisPrimary[n] = "";
					_inputPrimary[n] = KeyCode.None;
				}

				if (_num != n && _axisSecondary[_num] == _axisSecondary[n]) {
					_axisSecondary[n] = "";
					_inputSecondary[n] = KeyCode.None;
				}
			}
		}
	}

	private void _InputScans() {
		KeyCode _tmpModifier = KeyCode.None;
		if (Input.GetKey(KeyCode.Escape)) {
			if (_cScanInput == 1) {
				_inputPrimary[_cScanIndex] = KeyCode.None;
				_axisPrimary[_cScanIndex] = "";
				_cScanInput = 0;
			}

			if (_cScanInput == 2) {
				_inputSecondary[_cScanIndex] = KeyCode.None;
				_axisSecondary[_cScanIndex] = "";
				_cScanInput = 0;
			}
		}

		// keyboard + mouse + joystick button scanning
		if (_scanning && Input.anyKeyDown && !Input.GetKey(KeyCode.Escape)) {
			KeyCode _key = KeyCode.None;

			for (int i = (int)KeyCode.None; i < 450; i++) {
				KeyCode _ckey = (KeyCode)i;
				if (_ckey.ToString().StartsWith("Mouse")) {
					if (!_allowMouseButtons) {
						continue;
					}
				} else if (_ckey.ToString().StartsWith("Joystick")) {
					if (!_allowJoystickButtons) {
						continue;
					}
				} else if (!_allowKeyboard) {
					continue;
				}

				// loop through modifier list and set the input key
				for (int n = 0; n < _modifiers.Count; n++) {
					for (int m = 0; m < _modifiers.Count; m++) {
						if (Input.GetKeyDown(_modifiers[n])) {
							return;
						}
					}

					if (Input.GetKeyDown(_ckey)) {
						_key = _ckey;
						_tmpModifier = _ckey; // if this doesn't change it means there is no modifier used to set this input
						bool markedAsAxis = false; // has this key been marked as an axis?
						for (int m = 0; m < _markedAsAxis.Count; m++) {
							if (_cScanIndex == _markedAsAxis[m]) {
								markedAsAxis = true;
								break; // no need to loop through the rest
							}
						}

						// check if modifier is been pressed and that the inputs aren't part of an axis
						if (Input.GetKey(_modifiers[n]) && !markedAsAxis) {
							_tmpModifier = _modifiers[n]; // if this is being set here it means we have a modifier being pressed down
							break;
						}
					}
				}
			}

			if (_key != KeyCode.None) {
				bool _keyCleared = true;
				// check if the entered key is forbidden
				for (int b = 0; b < _forbiddenKeys.Count; b++) {
					if (_key == _forbiddenKeys[b]) {
						_keyCleared = false;
					}
				}

				if (_keyCleared) {
					if (_cScanInput == 1) {
						_inputPrimary[_cScanIndex] = _key;
						_modifierUsedPrimary[_cScanIndex] = _tmpModifier; // set the modifier being used 
						_axisPrimary[_cScanIndex] = "";
						_CheckingDuplicates(_cScanIndex, _cScanInput);
					}

					if (_cScanInput == 2) {
						_inputSecondary[_cScanIndex] = _key;
						_modifierUsedSecondary[_cScanIndex] = _tmpModifier; // set the modifier being used
						_axisSecondary[_cScanIndex] = "";
						_CheckingDuplicates(_cScanIndex, _cScanInput);
					}
				}

				_cScanInput = 0;
			}
		}

		// mouse scroll wheel scanning (considered to be a mousebutton)
		if (_allowMouseButtons) {
			if (Input.GetAxis("Mouse Wheel") > 0 && !Input.GetKey(KeyCode.Escape)) {
				if (_cScanInput == 1) {
					_axisPrimary[_cScanIndex] = "Mouse Wheel Up";
				}

				if (_cScanInput == 2) {
					_axisSecondary[_cScanIndex] = "Mouse Wheel Up";
				}

				_CheckingDuplicateStrings(_cScanIndex, _cScanInput);
				_cScanInput = 0;
			} else if (Input.GetAxis("Mouse Wheel") < 0 && !Input.GetKey(KeyCode.Escape)) {
				if (_cScanInput == 1) {
					_axisPrimary[_cScanIndex] = "Mouse Wheel Down";
				}

				if (_cScanInput == 2) {
					_axisSecondary[_cScanIndex] = "Mouse Wheel Down";
				}

				_CheckingDuplicateStrings(_cScanIndex, _cScanInput);
				_cScanInput = 0;
			}
		}

		// mouse axis scanning
		if (_allowMouseAxis) {
			if (Input.GetAxis("Mouse Horizontal") < -deadzone && !Input.GetKey(KeyCode.Escape)) {
				if (_cScanInput == 1) {
					_axisPrimary[_cScanIndex] = "Mouse Left";
				}

				if (_cScanInput == 2) {
					_axisSecondary[_cScanIndex] = "Mouse Left";
				}

				_CheckingDuplicateStrings(_cScanIndex, _cScanInput);
				_cScanInput = 0;
			} else if (Input.GetAxis("Mouse Horizontal") > deadzone && !Input.GetKey(KeyCode.Escape)) {
				if (_cScanInput == 1) {
					_axisPrimary[_cScanIndex] = "Mouse Right";
				}

				if (_cScanInput == 2) {
					_axisSecondary[_cScanIndex] = "Mouse Right";
				}

				_CheckingDuplicateStrings(_cScanIndex, _cScanInput);
				_cScanInput = 0;
			}

			if (Input.GetAxis("Mouse Vertical") > deadzone && !Input.GetKey(KeyCode.Escape)) {
				if (_cScanInput == 1) {
					_axisPrimary[_cScanIndex] = "Mouse Up";
				}

				if (_cScanInput == 2) {
					_axisSecondary[_cScanIndex] = "Mouse Up";
				}

				_CheckingDuplicateStrings(_cScanIndex, _cScanInput);
				_cScanInput = 0;
			} else if (Input.GetAxis("Mouse Vertical") < -deadzone && !Input.GetKey(KeyCode.Escape)) {
				if (_cScanInput == 1) {
					_axisPrimary[_cScanIndex] = "Mouse Down";
				}

				if (_cScanInput == 2) {
					_axisSecondary[_cScanIndex] = "Mouse Down";
				}

				_CheckingDuplicateStrings(_cScanIndex, _cScanInput);
				_cScanInput = 0;
			}

		}

		// joystick axis scanning
		if (_allowJoystickAxis) {
			for (int i = 1; i < _numGamepads; i++) {
				for (int j = 1; j <= 10; j++) {
					string _joystring = _joyStrings[i, j];
					string _joystringPos = _joyStringsPos[i, j];
					string _joystringNeg = _joyStringsNeg[i, j];
					float axis = _GetCalibratedAxisInput(_joystring);
					if (_scanning && Mathf.Abs(axis) > deadzone && !Input.GetKey(KeyCode.Escape)) {
						if (_cScanInput == 1) {
							if (axis > deadzone) {
								_axisPrimary[_cScanIndex] = _joystringPos;
							} else if (axis < -deadzone) {
								_axisPrimary[_cScanIndex] = _joystringNeg;
							}

							_CheckingDuplicateStrings(_cScanIndex, _cScanInput);
							_cScanInput = 0;
							break;
						} else if (_cScanInput == 2) {
							if (axis > deadzone) {
								_axisSecondary[_cScanIndex] = _joystringPos;
							} else if (axis < -deadzone) {
								_axisSecondary[_cScanIndex] = _joystringNeg;
							}

							_CheckingDuplicateStrings(_cScanIndex, _cScanInput);
							_cScanInput = 0;
							break;
						}
					}
				}
			}
		}
	}
}
