using System.Collections;
using System.Collections.Generic;
using GHParser.Graph;
using UnityEngine;
using VRTK;

public class UngrabComponentHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (GetComponent<VRTK_InteractableObject>() == null)
		{
			Debug.LogError("Need an InteractableObject script attached to this object.");
			return;
		}
		GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed += new InteractableObjectEventHandler(OnComponentUngrabbed);
	}

	private void OnComponentUngrabbed(object sender, InteractableObjectEventArgs e)
	{
		Debug.Log("Component " + transform.name + " was ungrabbed");
		InteractableVertex interactableVertex = GetComponent<InteractableVertex>();
		GHModelManager.Instance.RefreshEdges(interactableVertex.Vertex);
	}
}
