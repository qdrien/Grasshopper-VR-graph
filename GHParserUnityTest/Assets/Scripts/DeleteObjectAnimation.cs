using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class DeleteObjectAnimation : MonoBehaviour
{

	public float AnimationTime;

	private float _elapsed;

	private Vector3 _initialScale;

	void Start ()
	{
		_elapsed = 0;
		_initialScale = transform.localScale;
	}
	
	void Update ()
	{
		_elapsed += Time.deltaTime;
		
		if(_elapsed > AnimationTime) 
			Destroy(gameObject);

		transform.localScale = _initialScale * (1 - _elapsed / AnimationTime);
		transform.eulerAngles = Vector3.one * _elapsed;
	}
}
