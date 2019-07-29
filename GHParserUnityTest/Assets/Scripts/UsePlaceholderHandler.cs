using System;
using System.Collections;
using System.Collections.Generic;
using GHParser.Graph;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.SecondaryControllerGrabActions;

public class UsePlaceholderHandler : MonoBehaviour {

	void Start () {
		VRTK_InteractableObject interactableObject = GetComponent<VRTK_InteractableObject>();
		if (interactableObject == null)
		{
			Debug.LogError("Need an InteractableObject script attached to this object.");
			return;
		}
		interactableObject.InteractableObjectUsed += OnComponentUsed;
	}

	private void OnComponentUsed(object sender, InteractableObjectEventArgs e)
	{
		Debug.LogWarning(transform.name + " used.");
		gameObject.SetActive(false);
		Invoke(nameof(Activate), 2f);
		GHModelManager.Instance.AttachComponent(GetComponentInChildren<Text>().text, new Guid(transform.name));
	}

	private void Activate()
	{
		gameObject.SetActive(true);
	}
}
