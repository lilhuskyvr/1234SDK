using System.Reflection;

namespace VRE.Scripts.DataObjects
{
    public class OutfitItemDataObject : DataObject
    {
        public string id;
        public string[] manikinPartAddressIds = { };
        public float minQuality = 0.2f;
    }
}