using GHParser.GHElements;
using UnityEngine;

namespace GHParser.Graph
{
    public class InteractableGroup : MonoBehaviour
    {
        public Group Group { get; set; }

        private void Start()
        {
            if (Group == null)
            {
                Debug.LogError("This InteractableGroup does not point to an actual Group");
            }
        }
    }
}