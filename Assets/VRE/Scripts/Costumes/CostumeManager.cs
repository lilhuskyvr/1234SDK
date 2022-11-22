using System;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VRE.Scripts.Costumes
{
    public class CostumeManager: MonoBehaviour
    {
        private List<Costume> _costumes = new();
        public bool autoInit;

        private void Awake()
        {
            if (autoInit)
                Init();
        }

        public void Init()
        {
            _costumes = GetComponentsInChildren<Costume>().ToList();
        }
        
        private void Start()
        {
            if (_costumes.Count == 0)
                return;
            UnEquipAllCostumes(); 
            
            var costume = _costumes[Random.Range(0, _costumes.Count)];
            
            EquipCostume(costume);
        }

        public void EquipCostume(Costume costume)
        {
            costume.Equip();
        }

        [Button]
        public void UnEquipAllCostumes()
        {
            foreach (var costume in _costumes)
            {
                costume.UnEquip();
            }
        }
    }
}