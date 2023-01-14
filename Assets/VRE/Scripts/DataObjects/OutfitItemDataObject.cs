using System.Reflection;

namespace VRE.Scripts.DataObjects
{
    [Obfuscation(Exclude = true)]
    public class OutfitItemDataObject
    {
        public string id;
        public string[] manikinPartAddressIds = { };
        public float minQuality = 0.2f;
    }
}