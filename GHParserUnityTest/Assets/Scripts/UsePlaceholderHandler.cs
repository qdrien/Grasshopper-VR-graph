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
		GHModelManager.Instance.AttachTemplateComponent(new Guid(transform.name), e.interactingObject, GetComponentInChildren<Text>().text);
		gameObject.SetActive(false);
		Invoke(nameof(Activate), 2f);
	}

	private void Activate()
	{
		gameObject.SetActive(true);
	}
}
