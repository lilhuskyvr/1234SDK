using EasyButtons;
using UnityEngine;
using VRE.Scripts.DataObjects;

public enum AnimationPresetEnum
{
    Melee,
    Succubus
}

public enum CharacterSoundPresetEnum
{
    Female
}

public enum WeaponPresetEnum
{
    SwordScout
}

public class CharacterInfo : MonoBehaviour
{
    public AnimationPresetEnum animationPresetEnum;
    public CharacterSoundPresetEnum characterSoundPresetEnum;
    public WeaponPresetEnum[] weaponPresetEnums = { };
}