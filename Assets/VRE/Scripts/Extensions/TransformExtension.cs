using System.Linq;
using UnityEngine;

namespace VRE.Scripts.Extensions
{
    public static class TransformExtension
    {
        public static Transform[] GetChildTransformsWithChildCount(this Transform transform, int childCount)
        {
            return transform.GetComponentsInChildren<Transform>().Where(t => t.childCount == childCount).ToArray();
        }

        public static Transform[] GetChildTransforms(this Transform transform)
        {
            return transform.GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray();
        }

        public static bool HasChildren(this Transform transform)
        {
            return transform.GetComponentsInChildren<Transform>().Length > 1;
        }

        public static Transform CreateChildTransform(this Transform transform, string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(transform);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;

            return child;
        }

        public static void SetParentOrigin(this Transform transform, Transform parent)
        {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}