using UnityEngine;

namespace VRE.Scripts.Infos
{

    public class OutfitItemInfo : MonoBehaviour
    {
        public AnimationPresetEnum animationPresetEnum;
        public CharacterSoundPresetEnum characterSoundPresetEnum;
        public WeaponPresetEnum[] weaponPresetEnums = { };
        public bool isNSFW;
        [Tooltip("This rig is a base rig and will only be used to create new character. It can't be spawned in battle")]
        public bool isBaseRig;
    }
}