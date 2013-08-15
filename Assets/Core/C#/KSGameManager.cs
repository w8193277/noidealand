/*================================================================
  Scene manager class, manages assets, etc.
  Currently empty and will do nothing.
*/
using UnityEngine;
using System.Collections;
namespace Managers{
	public class KSGameManager : MonoBehaviour {
		public bool _hideCursor;
		public KeyCode menuKey;
	    void LateUpdate() {
	    	if(!_hideCursor){
	    		return;
	    	}
	    	if(_hideCursor){
	    		Screen.lockCursor = true;
	    		if(Input.GetKeyDown(menuKey)){
					Screen.lockCursor = true;
				}
				if(Input.GetKeyDown(menuKey)){
					Screen.showCursor = true;
				}
	    	}
	    }
	}
}