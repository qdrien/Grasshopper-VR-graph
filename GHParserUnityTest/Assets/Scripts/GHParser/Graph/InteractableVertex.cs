using System;
using System.Linq;
using UnityEngine;

namespace GHParser.Graph
{
    public class InteractableVertex : MonoBehaviour
    {
        private bool _trackVelocity;
        private Vector3 _previousPosition;
        private int _roundTripIndex;
        private int _velocityMemorySize = 10;
        private float[] _lastVelocities;
        
        private int _directionRoundTripIndex;
        private int _directionMemorySize = 10;
        private Vector3[] _lastDirections;

        public Vertex Vertex { get; set; }

        private void Start()
        {
            if (Vertex == null)
            {
                Debug.LogError("This InteractableVertex does not point to an actual Vertex");
            }

            _lastVelocities = new float[_velocityMemorySize];
            _lastDirections = new Vector3[_directionMemorySize];
        }

        private void Update()
        {
            if (_trackVelocity)
            {
                _lastVelocities[_roundTripIndex] = Vector3.Distance(transform.position, _previousPosition) / Time.deltaTime;
                _roundTripIndex++;
                if (_roundTripIndex >= _velocityMemorySize) _roundTripIndex = 0;

                _lastDirections[_directionRoundTripIndex] = transform.position - _previousPosition;
                _directionRoundTripIndex++;
                if (_directionRoundTripIndex >= _directionMemorySize) _directionRoundTripIndex = 0;
                
                _previousPosition = transform.position;
            }
        }

        public void StartTrackingVelocity()
        {
            _trackVelocity = true;
        }

        public float StopTrackingVelocity()
        {
            Vector3 sum = Vector3.zero;
            foreach (Vector3 vector in _lastDirections)
            {
                sum += vector;
            }
            
            Vector3 averageDirection = sum/_directionMemorySize;

            _trackVelocity = false;

            //safeguard based on the assumption that placing a component on the table is a downward movement
            if (averageDirection.y < 0) return 0f; 
            
            return _lastVelocities.Average();
        }
    }
}