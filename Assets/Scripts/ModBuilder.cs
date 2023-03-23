#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using VRE.Scripts.Infos;
using CharacterInfo = VRE.Scripts.Infos.CharacterInfo;

public class ModBuilder
{
    public static string GetModNameFromFilePath(string filePath)
    {
        return Directory.GetParent(filePath)?.Name;
    }

    public static string settingsAsset = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";

    public static AddressableAssetSettings getSettingsObject()
    {
        // This step is optional, you can also use the default settings:
        //settings = AddressableAssetSettingsDefaultObject.Settings;

        return AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset)
            as AddressableAssetSettings;
    }

    public static AddressableAssetGroup GetOrCreateAddressableGroup(string modName, AddressableAssetSettings settings)
    {
        var addressableGroup = settings.FindGroup(modName);
        if (ReferenceEquals(addressableGroup, null))
        {
            addressableGroup = settings.CreateGroup(modName, false, false, false, settings.DefaultGroup.Schemas);
        }

        return addressableGroup;
    }

    public static ModAsset AddAssetToAddressableGroup(string path, AddressableAssetGroup assetGroup,
        AddressableAssetSettings settings)
    {
        if (!ValidatePrefab(path, out var modAsset)) return null;

        var guid = AssetDatabase.AssetPathToGUID(path);

        var entry = settings.FindAssetEntry(guid);

        if (ReferenceEquals(entry, null))
        {
            entry = settings.CreateOrMoveEntry(guid, assetGroup);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entry, true);
        }

        var prefabGuid = AssetDatabase.AssetPathToGUID(path);
        var prefabEntry = settings.FindAssetEntry(prefabGuid);

        if (ReferenceEquals(prefabEntry, null)) return null;

        entry.SetAddress(modAsset.addressableAddressId, false);

        return modAsset;
    }

    private static bool ValidatePrefab(string path, out ModAsset modAsset)
    {
        modAsset = new ModAsset();
        var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var characterInfo = gameObject.GetComponent<CharacterInfo>();
        var outfitItemInfo = gameObject.GetComponent<OutfitItemInfo>();
        var weaponInfo = gameObject.GetComponent<WeaponInfo>();

        if (characterInfo != null && ValidateCharacterInfo(gameObject))
        {
            modAsset.addressableAddressId = Path.GetFileNameWithoutExtension(path);
            modAsset.info = characterInfo;
            return true;
        }

        if (outfitItemInfo != null)
        {
            modAsset.addressableAddressId = Path.GetFileNameWithoutExtension(path);
            modAsset.info = outfitItemInfo;
            return true;
        }
        
        if (weaponInfo != null)
        {
            modAsset.addressableAddressId = Path.GetFileNameWithoutExtension(path);
            modAsset.info = weaponInfo;
            return true;
        }

        return false;
    }

    private static bool ValidateCharacterInfo(GameObject go)
    {
        var goAnimator = go.GetComponent<Animator>();

        if (goAnimator == null)
        {
            EditorUtility.DisplayDialog("Error", $"Character doesn't have Animator in '{go.name}'",
                "OK");
            return false;
        }

        if (!goAnimator.isHuman)
        {
            EditorUtility.DisplayDialog("Error", $"Character Animator isn't humanoid in '{go.name}'",
                "OK");
            return false;
        }

        foreach (var skinnedMeshRenderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (skinnedMeshRenderer.sharedMesh == null)
            {
                EditorUtility.DisplayDialog("Error",
                    $"Skinned Mesh Renderer contains null mesh in '{go.name}: {skinnedMeshRenderer.name}'",
                    "OK");
                return false;
            }

            if (!skinnedMeshRenderer.sharedMesh.isReadable)
            {
                EditorUtility.DisplayDialog("Error",
                    $"Skinned Mesh Renderer isn't readable/writeable in '{go.name}: {skinnedMeshRenderer.name}'",
                    "OK");
                return false;
            }
        }

        return true;
    }

    [MenuItem("VRE/Build Mods")]
    public static async void BuildMods()
    {
        var inputDirectoryPath =
            EditorUtility.OpenFolderPanel("Choose Folder", Application.dataPath, "");

        if (ReferenceEquals(inputDirectoryPath, null)) return;

        var files = Directory.GetFiles(inputDirectoryPath, "*.prefab").Select(f => f.ToUnityRelativePath()).ToArray();

        if (files.Length == 0) return;

        var settings = getSettingsObject();

        if (ReferenceEquals(settings, null)) return;

        var modName = GetModNameFromFilePath(files[0]);

        var modAddressableGroup = GetOrCreateAddressableGroup(modName, settings);
        //set default
        settings.DefaultGroup = modAddressableGroup;

        var modAssets = new List<ModAsset>();

        foreach (var file in files)
        {
            var modAsset = AddAssetToAddressableGroup(file, modAddressableGroup, settings);
            if (modAsset == null) continue;
            modAssets.Add(modAsset);
        }

        var success = await BuildLauncher.BuildAddressables(modAssets);

        if (success) EditorUtility.DisplayDialog("Successful", $"Built mod '{modName}'", "OK");
    }
}
#endif