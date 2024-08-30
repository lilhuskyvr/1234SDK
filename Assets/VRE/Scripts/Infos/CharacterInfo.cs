#if UNITY_EDITOR
using System.Collections.Generic;
using EasyButtons;
using UnityEditor;
using UnityEngine;

namespace VRE.Scripts.Infos
{
    public enum AnimationPresetEnum
    {
        Melee,
        Succubus,
        Fighter,
        Spear,
        TwoHandedSword
    }

    public enum CharacterSoundPresetEnum
    {
        Male,
        Female
    }

    public enum WeaponPresetEnum
    {
        SwordScout,
        SwordA1,
        SwordE,
        SwordF,
        SwordG,
        SwordI
    }

    public class CharacterInfo : Info
    {
        public AnimationPresetEnum animationPresetEnum;
        public CharacterSoundPresetEnum characterSoundPresetEnum;
        public List<string> weaponPresetIds = new();
        public string[] outfitPresetIds = { };
        public SkinnedMeshRenderer[] outfitItemParts;
        public string[] hairPresetIds = { };

        public bool isCore;

        [Button]
        public void SetWeaponPresets(WeaponPresetEnum weaponPresetEnum)
        {
            weaponPresetIds.Add(weaponPresetEnum.ToString());
            EditorUtility.SetDirty(gameObject);
            AssetDatabase.SaveAssets();
        }

        public void BeforeBuild()
        {
            if (outfitItemParts == null) return;
            foreach (var skinnedMeshRenderer in outfitItemParts)
            {
                skinnedMeshRenderer.gameObject.SetActive(false);
            }
        }
    }
}
#endif