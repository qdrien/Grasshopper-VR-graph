using System.Collections;
using System.Collections.Generic;
using GHParser.Graph;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.SecondaryControllerGrabActions;

public class UngrabComponentHandler : MonoBehaviour {

	void Start () {
		VRTK_InteractableObject interactableObject = GetComponent<VRTK_InteractableObject>();
		if (interactableObject == null)
		{
			Debug.LogError("Need an InteractableObject script attached to this object.");
			return;
		}
		interactableObject.InteractableObjectUngrabbed += OnComponentUngrabbed;
		interactableObject.InteractableObjectGrabbed += OnComponentGrabbed;
	}

	private void OnComponentGrabbed(object sender, InteractableObjectEventArgs e)
	{
		GetComponent<InteractableVertex>().StartTrackingVelocity();
	}

	private void OnComponentUngrabbed(object sender, InteractableObjectEventArgs e)
	{
		Debug.Log("Component " + transform.name + " was ungrabbed");
		InteractableVertex interactableVertex = GetComponent<InteractableVertex>();
		float releaseVelocity = interactableVertex.StopTrackingVelocity();
		Debug.Log("Release velocity = " + releaseVelocity);
		if (releaseVelocity > .4f)
		{
			Debug.Log("Deleting the object " + gameObject.name);

			GetComponent<VRTK_InteractableObject>().enabled = false;
			GetComponent<VRTK_ChildOfControllerGrabAttach>().enabled = false;
			GetComponent<VRTK_SwapControllerGrabAction>().enabled = false;
			GetComponent<VRTK_InteractObjectHighlighter>().enabled = false;

			DeleteObjectAnimation deleteObjectAnimation = gameObject.AddComponent<DeleteObjectAnimation>();
			deleteObjectAnimation.AnimationTime = 2f;

			Material redMaterial = new Material(Shader.Find("Unlit/Color")){color = Color.red};

			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers)
			{
				r.material = redMaterial;
			}
			
			GHModelManager.Instance.RemoveEdges(interactableVertex.Vertex);
		}
		else
		{
			GHModelManager.Instance.RefreshEdges(interactableVertex.Vertex);
		}
	}
}
