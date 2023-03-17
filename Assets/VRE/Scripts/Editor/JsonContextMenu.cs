using System.Collections.Generic;
using System.IO;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using VRE.Scripts.DataObjects;

namespace VRE.Scripts.Editor
{
    public class JsonContextMenu
    {
        [MenuItem("Assets/Create/OutfitItemDataObject.json", false, 1)]
        public static void CreateOutfitItemJson()
        {
            var outfitItemIds = new List<string>();
            var dirPath = "";
            foreach (var gameObject in Selection.gameObjects)
            {
                var fullPath = AssetDatabase.GetAssetPath(gameObject).AssetPathToFullPath();
                dirPath = $"{Path.GetDirectoryName(fullPath)}/JSON";
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                var outfitItemId = gameObject.name;
                
                var outfitItemDataObject = new OutfitItemDataObject
                {
                    id = outfitItemId,
                    belongsToBody = false,
                    manikinPartAddressIds = new[] { outfitItemId },
                    minQuality = 1
                };
                
                outfitItemIds.Add(outfitItemId);

                File.WriteAllText($"{dirPath}/{outfitItemId}.json", outfitItemDataObject.ToJson());
            }
            
            var outfitDataObject = new OutfitDataObject()
            {
                id = "outfitItem",
                outfitItemIds =  outfitItemIds.ToArray(),
            };
            
            File.WriteAllText($"{dirPath}/outfit.json", outfitDataObject.ToJson());
            
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/Create/MapDataObject.json", false, 1)]
        public static void CreateMapJson()
        {
            foreach (var gameObject in Selection.objects)
            {
                Debug.Log(gameObject.name);
                var fullPath = AssetDatabase.GetAssetPath(gameObject).AssetPathToFullPath();
                var dirPath = $"{Path.GetDirectoryName(fullPath)}/Maps";
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                var mapId = gameObject.name;
                
                var outfitItemDataObject = new MapDataObject
                {
                    id = mapId,
                    mapAddressId = mapId,
                    mapThumbnailAddressId = $"{mapId}Thumbnail"
                };

                File.WriteAllText($"{dirPath}/{mapId}.json", outfitItemDataObject.ToJson());
            }
            
            AssetDatabase.Refresh();
        }
    }
}