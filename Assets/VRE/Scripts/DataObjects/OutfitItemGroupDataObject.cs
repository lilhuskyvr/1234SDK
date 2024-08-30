namespace VRE.Scripts.DataObjects
{
    public class OutfitItemGroupDataObject : DataObject
    {
        public string id;
        public string name;
        public string[] outfitItemIds = { };
        //if this is true, don't allow empty string
        public bool disallowEmptyString;
    }
}