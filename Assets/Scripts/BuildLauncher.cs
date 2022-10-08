﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VRE.Scripts.DataObjects;

public class BuildLauncher
{
    public static string build_script
        = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";

    public static string settings_asset
        = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";

    public static string profile_name = "Default";
    private static AddressableAssetSettings settings;

    static void getSettingsObject(string settingsAsset)
    {
        // This step is optional, you can also use the default settings:
        //settings = AddressableAssetSettingsDefaultObject.Settings;

        settings
            = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset)
                as AddressableAssetSettings;

        if (settings == null)
            Debug.LogError($"{settingsAsset} couldn't be found or isn't " +
                           $"a settings object.");
    }

    static void setProfile(string profile)
    {
        string profileId = settings.profileSettings.GetProfileId(profile);
        if (String.IsNullOrEmpty(profileId))
            Debug.LogWarning($"Couldn't find a profile named, {profile}, " +
                             $"using current profile instead.");
        else
            settings.activeProfileId = profileId;
    }

    static void setBuilder(IDataBuilder builder)
    {
        int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);

        if (index > 0)
            settings.ActivePlayerDataBuilderIndex = index;
        else
            Debug.LogWarning($"{builder} must be added to the " +
                             $"DataBuilders list before it can be made " +
                             $"active. Using last run builder instead.");
    }

    static bool buildAddressableContent()
    {
        AddressableAssetSettings
            .BuildPlayerContent(out AddressablesPlayerBuildResult result);
        bool success = string.IsNullOrEmpty(result.Error);

        if (!success)
        {
            Debug.LogError("Addressables build error encountered: " + result.Error);
        }

        return success;
    }

    [MenuItem("VRE/Build Custom Characters Mod")]
    public static async Task<bool> BuildAddressables()
    {
        var characterIds = new List<string>();
        //ie: Assets/Mods/SuccubusLily
        var modBuildPathInAssetsFolder = "";
        getSettingsObject(settings_asset);
        setProfile(profile_name);
        IDataBuilder builderScript
            = AssetDatabase.LoadAssetAtPath<ScriptableObject>(build_script) as IDataBuilder;

        var modName = "";

        foreach (var addressableAssetGroup in settings.groups)
        {
            if (addressableAssetGroup.IsDefaultGroup())
                modName = addressableAssetGroup.Name;
        }

        settings.profileSettings.SetValue(settings.activeProfileId, "modName", modName);

        foreach (var addressableAssetGroup in settings.groups)
        {
            addressableAssetGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming =
                BundledAssetGroupSchema.BundleNamingStyle.NoHash;
            addressableAssetGroup.GetSchema<BundledAssetGroupSchema>().IncludeInBuild =
                addressableAssetGroup.name == modName;

            //if eligible to build
            if (addressableAssetGroup.name == modName)
            {
                characterIds.AddRange(
                    addressableAssetGroup.entries.Select(addressableAssetEntry => addressableAssetEntry.address));
            }

            modBuildPathInAssetsFolder =
                addressableAssetGroup.GetSchema<BundledAssetGroupSchema>().BuildPath.GetValue(settings);
        }

        var isValid = await ValidateAddressables(characterIds);

        if (!isValid)
            return false;

        if (builderScript == null)
        {
            Debug.LogError(build_script + " couldn't be found or isn't a build script.");
            return false;
        }

        setBuilder(builderScript);

        buildAddressableContent();

        await buildJSONFiles(modBuildPathInAssetsFolder, characterIds);

        return true;
    }

    private static async Task<bool> ValidateAddressables(List<string> characterIds)
    {
        foreach (var characterId in characterIds)
        {
            if (characterId.Contains(" "))
            {
                EditorUtility.DisplayDialog("Error", $"Character Name can't contain Space in '{characterId}'", "OK");
                return false;
            }

            var go = await Addressables.LoadAssetAsync<GameObject>(characterId).Task;

            var goAnimator = go.GetComponent<Animator>();

            if (goAnimator == null)
            {
                EditorUtility.DisplayDialog("Error", $"Character doesn't have Animator in '{characterId}'", "OK");
                return false;
            }

            if (!goAnimator.isHuman)
            {
                EditorUtility.DisplayDialog("Error", $"Character Animator isn't humanoid in '{characterId}'", "OK");
                return false;
            }

            foreach (var skinnedMeshRenderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMeshRenderer.sharedMesh == null)
                {
                    EditorUtility.DisplayDialog("Error",
                        $"Skinned Mesh Renderer contains null mesh in '{characterId}: {skinnedMeshRenderer.name}'",
                        "OK");
                    return false;
                }

                if (!skinnedMeshRenderer.sharedMesh.isReadable)
                {
                    EditorUtility.DisplayDialog("Error",
                        $"Skinned Mesh Renderer isn't readable/writeable in '{characterId}: {skinnedMeshRenderer.name}'",
                        "OK");
                    return false;
                }
            }
        }

        return true;
    }

    static async Task buildJSONFiles(string directoryPath, List<string> characterIds)
    {
        foreach (var characterId in characterIds)
        {
            var characterGameObject = await Addressables.LoadAssetAsync<GameObject>(characterId).Task;

            var characterInfo = characterGameObject.GetComponent<CharacterInfo>();

            var characterDataObject = new CharacterDataObject()
            {
                id = characterId,
                characterRigAddressId = characterId,
                animationPresetId = characterInfo
                    ? characterInfo.animationPresetEnum.ToString()
                    : AnimationPresetEnum.Melee.ToString(),
                characterSoundPresetId = characterInfo
                    ? characterInfo.characterSoundPresetEnum.ToString()
                    : CharacterSoundPresetEnum.Female.ToString(),
                weaponPresetIds = characterInfo
                    ? characterInfo.weaponPresetEnums.Select(wpe => wpe.ToString()).ToArray()
                    : new string[] { }
            };

            var jsonContent = JsonConvert.SerializeObject(characterDataObject, Formatting.Indented);

            File.WriteAllText($"{directoryPath}/{characterId}.json", jsonContent);
        }
    }
}
#endif