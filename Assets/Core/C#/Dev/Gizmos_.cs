using UnityEngine;
using System.Collections;
namespace EditorGizmos{
	public class Gizmos_ : MonoBehaviour {
		public enum _GizmoType{
			ray,
			line,
			wiresphere,
			sphere,
			wirecube,
			cube,
			icon
		}
		public _GizmoType gizmoType = _GizmoType.wirecube;
		public Color _lineColor;
		public Color _solidColor;
		public float sphereRadius = 5.0f;
		public float rayLength = 5.0f;
		public Vector3 cubeSize = new Vector3(1.0f,1.0f,1.0f);
		public Transform lineTarget;
		public string iconName = "icon.putextensionere";
		void OnDrawGizmos(){
			switch (gizmoType){
				case _GizmoType.ray : 
					Gizmos.color = _lineColor;
					Vector3 direction = transform.TransformDirection(Vector3.forward) * rayLength;
					Gizmos.DrawRay(transform.position, direction);
					break;
				case _GizmoType.line :
					if(lineTarget != null){
						Gizmos.color = _lineColor;
						Gizmos.DrawLine(transform.position, lineTarget.position);
					}
					break;
				case _GizmoType.wiresphere :
					Gizmos.color = _lineColor;
					Gizmos.DrawWireSphere(transform.position, sphereRadius);
					break;
				case _GizmoType.sphere :
					Gizmos.color = _solidColor;
					Gizmos.DrawSphere(transform.position, sphereRadius);
					break;
				case _GizmoType.wirecube :
					Gizmos.color = _lineColor;
					Gizmos.DrawWireCube(transform.position, cubeSize);
					break;
				case _GizmoType.cube :
					Gizmos.color = _solidColor;
					Gizmos.DrawCube(transform.position, cubeSize);
					break;
				case _GizmoType.icon :
					Gizmos.DrawIcon(transform.position, iconName, true);
					break;
			}
		}
	}
}

