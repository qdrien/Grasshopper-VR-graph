using System;
using System.Collections.Generic;
using UnityEngine;

namespace GHParser.Utils
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineCollider : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        private Vector3[] _oldPositions;

        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            Vector3[] positions = new Vector3[_lineRenderer.positionCount];
            UpdateColliders(positions);
        }

        public void Update()
        {
            Vector3[] positions = new Vector3[_lineRenderer.positionCount];
            _lineRenderer.GetPositions(positions);
         
            if (_oldPositions.Length != _lineRenderer.positionCount)
            {
                UpdateColliders(positions);
                return;
            }
            
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] != _oldPositions[i])
                {
                    UpdateColliders(positions);
                    return;
                }
            }
        }
        
        private void UpdateColliders(Vector3[] positions)
        {
            //Debug.LogError("Updating colliders for " + gameObject.name);
            _oldPositions = positions;

            for (int i = 0; i < positions.Length - 1; i++)
            {
                Vector3 start = positions[i];
                Vector3 end = positions[i + 1];
                GameObject colliderContainer = new GameObject("Collider");
                CapsuleCollider capsuleCollider = colliderContainer.AddComponent<CapsuleCollider>();
                colliderContainer.transform.parent = transform;
                colliderContainer.transform.position = Vector3.Lerp(start, end, .5f);
                capsuleCollider.radius = _lineRenderer.startWidth;
                if(!Mathf.Approximately(_lineRenderer.startWidth, _lineRenderer.endWidth))
                {
                    Debug.LogWarning("Different start/end width for the same line are not supported");
                }

                capsuleCollider.direction = 2;
                capsuleCollider.height = Vector3.Distance(start, end);
                Vector3 direction = (end - start).normalized;
                Quaternion lookRotation = Quaternion.identity;
                if (direction != Vector3.zero) 
                {
                    //LookRotation would call Debug.Log() and cause perf issues with a zero vector
                    //since there is no (or an infinite amount of) direction(s) in that case
                    lookRotation = Quaternion.LookRotation(direction);
                }

                colliderContainer.transform.rotation = lookRotation;
            }
            
        }
    }
}