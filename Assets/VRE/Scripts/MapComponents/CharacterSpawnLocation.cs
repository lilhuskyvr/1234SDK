using UnityEngine;

namespace VRE.Scripts.MapComponents
{
    public class CharacterSpawnLocation : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position + Vector3.up, new Vector3(0.5f, 2, 0.5f));
        }
    }
}