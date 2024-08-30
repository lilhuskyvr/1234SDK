namespace VRE.Scripts.DataObjects
{
    public class OutfitItemDataObject : DataObject
    {
        public string id;
        public string[] manikinPartAddressIds = { };
        public float minQuality = 1f;
        public bool belongsToBody;
        public string[] coverSkinMeshRendererNames = { };
        public string[] hexColors = { };
        public bool tearable;
    }
}