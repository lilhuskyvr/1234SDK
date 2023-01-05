using System.Collections.Generic;
using EasyButtons;
using UnityEngine;

namespace VRE.Scripts.Costumes
{
    public class Costume : MonoBehaviour
    {
        public List<Transform> parts = new();

        public void Equip()
        {
            foreach (var part in parts)
            {
                part.gameObject.SetActive(true);
            }
        }

        public void UnEquip()
        {
            foreach (var part in parts)
            {
                part.gameObject.SetActive(false);
            }
        }

        [Button]
        public void Preview()
        {
            gameObject.GetComponentInParent<CostumeManager>().UnEquipAllCostumes();
            Equip();
        }

        [Button]
        public void UnPreview()
        {
            UnEquip();
        }

        [Button]
        public void Duplicate()
        {
            var clonedCostume = gameObject.AddComponent<Costume>();

            clonedCostume.parts = new List<Transform>(parts);
        }
    }
}