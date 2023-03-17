using UnityEngine;

namespace VRE.Scripts.MapComponents
{
    public class MapCanvasGroup : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position + 2 * Vector3.up, new Vector3(8, 2));
        }
    }
}