using UnityEngine;

namespace VRE.Scripts.Infos
{
    public enum AnimationPresetEnum
    {
        Melee,
        Succubus,
        Fighter
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
        public WeaponPresetEnum[] weaponPresetEnums = { };
        public string[] outfitPresetIds = { };
        public bool isNSFW;
        [Tooltip("This rig is a base rig and will only be used to create new character. It can't be spawned in battle")]
        public bool isBaseRig;
        public bool isCore;
    }
}