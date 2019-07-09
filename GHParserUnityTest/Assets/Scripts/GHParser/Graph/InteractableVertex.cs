using UnityEngine;

namespace GHParser.Graph
{
    public class InteractableVertex : MonoBehaviour
    {
        public Vertex Vertex { get; set; }

        private void Start()
        {
            if (Vertex == null)
            {
                Debug.LogError("This InteractableVertex does not point to an actual Vertex");
            }
        }
    }
}