using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using UnityEngine;
using VRE.Scripts.WeaponComponents;

namespace VRE.Scripts.Infos
{
    public enum AnimationPresetEnum
    {
        Melee,
        Succubus,
        Fighter,
        Spear
    }

    public enum CharacterSoundPresetEnum
    {
        Female
    }

    public enum WeaponPresetEnum
    {
        SwordScout
    }

    public class CharacterInfo : Info
    {
        public AnimationPresetEnum animationPresetEnum;
        public CharacterSoundPresetEnum characterSoundPresetEnum;
        public List<string> weaponPresetIds = new();
        public string[] outfitPresetIds = { };
        public bool isNSFW;
        [Tooltip("This rig is a base rig and will only be used to create new character. It can't be spawned in battle")]
        public bool isBaseRig;
        public bool isCore;
        
        [Button]
        public void SetWeaponPresets(WeaponPresetEnum weaponPresetEnum)
        {
            weaponPresetIds.Add(weaponPresetEnum.ToString());
        }
    }
}