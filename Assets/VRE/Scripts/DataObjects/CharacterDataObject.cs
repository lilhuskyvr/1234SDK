namespace VRE.Scripts.DataObjects
{
    public class CharacterDataObject: DataObject
    {
        public string id;
        public string animationPresetId;
        public string characterSoundPresetId;
        public string[] weaponPresetIds = {};
        public string[] outfitPresetIds = {};
        public string[] outfitItemParts = { };
        public string[] hairPresetIds = {};
        public string characterRigAddressId;
        public bool isNSFW;
        public bool isCore;
        public float scale = 1;
        public float moveSpeedInMps = 3f;
        public bool canBeDismembered = true;
    }
}