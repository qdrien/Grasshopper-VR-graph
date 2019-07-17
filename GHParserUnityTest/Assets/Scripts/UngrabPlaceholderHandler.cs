using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class UngrabPlaceholderHandler : MonoBehaviour {

	void Start () {
		
		VRTK_InteractableObject interactableObject = GetComponent<VRTK_InteractableObject>();
		if (interactableObject == null)
		{
			Debug.LogError("Need an InteractableObject script attached to this object.");
			return;
		}
		interactableObject.InteractableObjectUngrabbed += OnPlaceholderUngrabbed;
	}

	private void OnPlaceholderUngrabbed(object sender, InteractableObjectEventArgs e)
	{
		//TODO: should probably clone the object here
		//TODO: attach it to the drawingsurface
		
		VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_ObjectAutoGrab>().enabled = false;
	}
}
