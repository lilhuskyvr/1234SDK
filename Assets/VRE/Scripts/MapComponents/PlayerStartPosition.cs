using System;
using UnityEngine;

namespace VRE.Scripts.MapComponents
{
    public class PlayerStartPosition : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(transform.position + Vector3.up, new Vector3(0.5f, 2, 0.5f));
        }
    }
}