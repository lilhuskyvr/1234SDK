#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

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

    public static void AddAssetToAddressableGroup(string path, AddressableAssetGroup assetGroup,
        AddressableAssetSettings settings)
    {
        if (!ValidatePrefab(path)) return;

        var guid = AssetDatabase.AssetPathToGUID(path);

        var entry = settings.FindAssetEntry(guid);

        if (ReferenceEquals(entry, null))
        {
            entry = settings.CreateOrMoveEntry(guid, assetGroup);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entry, true);
        }

        var prefabGuid = AssetDatabase.AssetPathToGUID(path);
        var prefabEntry = settings.FindAssetEntry(prefabGuid);

        if (ReferenceEquals(prefabEntry, null)) return;

        entry.SetAddress(Path.GetFileNameWithoutExtension(path), false);
    }

    private static bool ValidatePrefab(string path)
    {
        var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        var characterInfo = gameObject.GetComponent<CharacterInfo>();

        if (ReferenceEquals(characterInfo, null)) return false;

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

        foreach (var file in files)
        {
            AddAssetToAddressableGroup(file, modAddressableGroup, settings);
        }

        var success = await BuildLauncher.BuildAddressables();

        if (success) EditorUtility.DisplayDialog("Successful", $"Built mod '{modName}'", "OK");
    }
}
#endif