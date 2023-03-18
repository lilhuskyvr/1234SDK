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
                outfitItemIds = outfitItemIds.ToArray(),
            };

            File.WriteAllText($"{dirPath}/outfit.json", outfitDataObject.ToJson());

            AssetDatabase.Refresh();
        }


        [MenuItem("Assets/Create/MapDataObject.json", false, 1)]
        public static void CreateMapJson()
        {
            foreach (var gameObject in Selection.objects)
            {
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

        public static void CopyFilesRecursive(string sourceDirectory, string targetDirectory)
        {
            foreach (var directoryPath in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(directoryPath.Replace(sourceDirectory, targetDirectory));
            }

            foreach (var filePath in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(filePath, filePath.Replace(sourceDirectory, targetDirectory), true);
            }
        }

        [MenuItem("Assets/Copy To Game Mods", false, 1)]
        public static void CopyToGameMods()
        {
            foreach (var gameObject in Selection.objects)
            {
                var sourcePath = AssetDatabase.GetAssetPath(gameObject).AssetPathToFullPath();
                if (!Directory.Exists(sourcePath)) continue;
                var destinationPath =
                    $"C:/1234/1234URP/1234URP/Build/1234_Data/StreamingAssets/Mods/{Path.GetFileName(sourcePath)}";

                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                CopyFilesRecursive(sourcePath, destinationPath);

                EditorUtility.DisplayDialog("Successful!", $"Copied directory: {sourcePath}", "OK");
            }
        }
    }
}