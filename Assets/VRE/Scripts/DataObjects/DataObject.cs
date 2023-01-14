using Newtonsoft.Json;

namespace VRE.Scripts.DataObjects
{
    public class DataObject
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
        }
    }
}