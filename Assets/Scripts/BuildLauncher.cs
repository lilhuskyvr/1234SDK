#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VRE.Scripts.DataObjects;
using VRE.Scripts.Infos;
using CharacterInfo = VRE.Scripts.Infos.CharacterInfo;

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

    static bool setBuilder(IDataBuilder builder)
    {
        int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);

        if (index > 0)
        {
            settings.ActivePlayerDataBuilderIndex = index;
            return true;
        }
        else
        {
            Debug.LogWarning($"{builder} must be added to the " +
                             $"DataBuilders list before it can be made " +
                             $"active. Using last run builder instead.");

            return false;
        }
    }

    static bool buildAddressableContent()
    {
        AddressableAssetSettings
            .BuildPlayerContent(out AddressablesPlayerBuildResult result);
        bool success = string.IsNullOrEmpty(result.Error);

        if (!success)
        {
            EditorUtility.DisplayDialog("Failure!", "Addressables build error encountered: " + result.Error, "OK");
        }

        return success;
    }

    [MenuItem("VRE/Build Default Addressable Group")]
    public static async Task<bool> BuildAddressables()
    {
        var addressableAddressIds = new List<string>();
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
            try
            {
                addressableAssetGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming =
                    BundledAssetGroupSchema.BundleNamingStyle.NoHash;
                addressableAssetGroup.GetSchema<BundledAssetGroupSchema>().IncludeInBuild =
                    addressableAssetGroup.name == modName;

                //if eligible to build
                if (addressableAssetGroup.name == modName)
                {
                    addressableAddressIds.AddRange(
                        addressableAssetGroup.entries.Select(addressableAssetEntry => addressableAssetEntry.address));
                }

                modBuildPathInAssetsFolder =
                    addressableAssetGroup.GetSchema<BundledAssetGroupSchema>().BuildPath.GetValue(settings);
            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);
            }
        }

        // var isValid = await ValidateAddressables(addressableAddressIds);

        if (builderScript == null)
        {
            EditorUtility.DisplayDialog("Failure!", build_script + " couldn't be found or isn't a build script.", "OK");
            return false;
        }

        if (!setBuilder(builderScript))
        {
            EditorUtility.DisplayDialog("Failure!", "Fail to setup builderScript", "OK");
            return false;
        }

        buildAddressableContent();

        EditorUtility.DisplayDialog("Successful!", "Built addressable content", "OK");

        await BuildJsonFiles(modBuildPathInAssetsFolder, addressableAddressIds);

        return true;
    }

    private static async Task<bool> ValidateAddressables(List<string> addressableAddressIds)
    {
        foreach (var addressableAddressId in addressableAddressIds)
        {
            if (addressableAddressId.Contains(" "))
            {
                EditorUtility.DisplayDialog("Error", $"Character Name can't contain Space in '{addressableAddressId}'",
                    "OK");
                return false;
            }  

            var go = await Addressables.LoadAssetAsync<GameObject>(addressableAddressId).Task;

            var characterInfo = go.GetComponent<CharacterInfo>();

            // not a character
            if (characterInfo == null)
            {
                continue;
            }

            var goAnimator = go.GetComponent<Animator>();

            if (goAnimator == null)
            {
                EditorUtility.DisplayDialog("Error", $"Character doesn't have Animator in '{addressableAddressId}'",
                    "OK");
                return false;
            }

            if (!goAnimator.isHuman)
            {
                EditorUtility.DisplayDialog("Error", $"Character Animator isn't humanoid in '{addressableAddressId}'",
                    "OK");
                return false;
            }

            foreach (var skinnedMeshRenderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMeshRenderer.sharedMesh == null)
                {
                    EditorUtility.DisplayDialog("Error",
                        $"Skinned Mesh Renderer contains null mesh in '{addressableAddressId}: {skinnedMeshRenderer.name}'",
                        "OK");
                    return false;
                }

                if (!skinnedMeshRenderer.sharedMesh.isReadable)
                {
                    EditorUtility.DisplayDialog("Error",
                        $"Skinned Mesh Renderer isn't readable/writeable in '{addressableAddressId}: {skinnedMeshRenderer.name}'",
                        "OK");
                    return false;
                }
            }
        }

        return true;
    }

    public static async Task BuildJsonFiles(string directoryPath, List<string> addressableAddressIds)
    {
        var characterDirectory = $"{directoryPath}/Characters";
        var outfitItemDirectory = $"{directoryPath}/OutfitItems";

        foreach (var addressableAddressId in addressableAddressIds)
        {
            var characterGameObject = await Addressables.LoadAssetAsync<GameObject>(addressableAddressId).Task;

            var characterInfo = characterGameObject.GetComponent<CharacterInfo>();
            var outfitItemInfo = characterGameObject.GetComponent<OutfitItemInfo>();

            if (!ReferenceEquals(characterInfo, null))
            {
                var characterDataObject = new CharacterDataObject
                {
                    id = addressableAddressId,
                    characterRigAddressId = addressableAddressId,
                    animationPresetId = characterInfo
                        ? characterInfo.animationPresetEnum.ToString()
                        : AnimationPresetEnum.Melee.ToString(),
                    characterSoundPresetId = characterInfo
                        ? characterInfo.characterSoundPresetEnum.ToString()
                        : CharacterSoundPresetEnum.Female.ToString(),
                    weaponPresetIds = characterInfo
                        ? characterInfo.weaponPresetEnums.Select(wpe => wpe.ToString()).ToArray()
                        : new string[] { },
                    isNSFW = characterInfo && characterInfo.isNSFW
                };

                if (!Directory.Exists(characterDirectory)) Directory.CreateDirectory(characterDirectory);
                await File.WriteAllTextAsync($"{characterDirectory}/{addressableAddressId}.json", characterDataObject.ToJson());
            }

            if (!ReferenceEquals(outfitItemInfo, null))
            {
                Debug.Log("outfit item" + addressableAddressId);
                var outfitItemDataObject = new OutfitItemDataObject
                {
                    id = addressableAddressId,
                    manikinPartAddressIds = new[] { addressableAddressId },
                    minQuality = outfitItemInfo.minQuality,
                    belongsToBody = outfitItemInfo.belongsToBody
                };

                if (!Directory.Exists(outfitItemDirectory)) Directory.CreateDirectory(outfitItemDirectory);
                await File.WriteAllTextAsync($"{outfitItemDirectory}/{addressableAddressId}.json", outfitItemDataObject.ToJson());
            }
        }

        AssetDatabase.Refresh();
    }
}
#endif