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

    private static AddressableAssetSettings settings;

    public static AddressableAssetSettings GetSettingsObject()
    {
        var settingsAsset = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";

        return AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset)
            as AddressableAssetSettings;
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

    [MenuItem("VRE/Build Default Addressable Group For StreamingAssets/Mods")]
    public static void BuildDefaultAddressableGroupAsMod()
    {
        BuildAddressables(new List<ModAsset>());
    }

    [MenuItem("VRE/Build Default Addressable Group For StreamingAssets/Default/Free")]
    public static void BuildDefaultAddressableGroupAsGamePart()
    {
        BuildAddressables(new List<ModAsset>(), "StreamingAssets/Default/Free");
    }

    [MenuItem("VRE/Build Default Addressable Group For StreamingAssets/Default/Full")]
    public static void BuildDefaultAddressableGroupAsGamePartFull()
    {
        BuildAddressables(new List<ModAsset>(), "StreamingAssets/Default/Full");
    }

    public static async Task<bool> BuildAddressables(List<ModAsset> modAssets, string profileName = "Default")
    {
        //ie: Assets/Mods/SuccubusLily
        var modBuildPathInAssetsFolder = "";

        settings = GetSettingsObject();

        setProfile(profileName);
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

                modBuildPathInAssetsFolder =
                    addressableAssetGroup.GetSchema<BundledAssetGroupSchema>().BuildPath.GetValue(settings);
            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);
            }
        }

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

        if (modAssets.Count > 0)
            await BuildJsonFiles(modBuildPathInAssetsFolder, modAssets);

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

    public static async Task BuildJsonFiles(string directoryPath, List<ModAsset> modAssets)
    {
        var characterDirectory = $"{directoryPath}/Characters";
        var outfitItemDirectory = $"{directoryPath}/OutfitItems";
        var outfitItemGroupDirectory = $"{directoryPath}/OutfitItemGroups";
        var outfitDirectory = $"{directoryPath}/Outfits";
        var weaponDirectory = $"{directoryPath}/Weapons";
        var hairDirectory = $"{directoryPath}/Hairs";
        var outfitPresetIds = new List<string>();
        var characterPresetIds = new List<string>();

        foreach (var modAsset in modAssets)
        {
            if (modAsset.info is CharacterInfo characterInfo)
            {
                var characterDataObject = new CharacterDataObject
                {
                    id = modAsset.addressableAddressId,
                    characterRigAddressId = modAsset.addressableAddressId,
                    animationPresetId =
                        characterInfo.animationPresetEnum.ToString(),
                    characterSoundPresetId = characterInfo.characterSoundPresetEnum.ToString(),
                    weaponPresetIds = characterInfo.weaponPresetIds.ToArray(),
                    outfitPresetIds = characterInfo.outfitPresetIds,
                    hairPresetIds = characterInfo.hairPresetIds,
                    isCore = characterInfo.isCore,
                    outfitItemParts = characterInfo.outfitItemParts.Select(part => part.name).ToArray()
                };

                characterPresetIds.Add(characterDataObject.id);
                outfitPresetIds.AddRange(characterInfo.outfitPresetIds);

                if (!Directory.Exists(characterDirectory)) Directory.CreateDirectory(characterDirectory);
                await File.WriteAllTextAsync($"{characterDirectory}/{modAsset.addressableAddressId}.json",
                    characterDataObject.ToJson());
                
                foreach (var characterInfoOutfitItemPart in characterInfo.outfitItemParts)
                {
                    var outfitItemDataObject = new OutfitItemDataObject
                    {
                        id = characterInfoOutfitItemPart.name,
                        manikinPartAddressIds = new[] { characterInfoOutfitItemPart.name },
                        minQuality = 1,
                        belongsToBody = false,
                        coverSkinMeshRendererNames = { },
                    };

                    if (!Directory.Exists(outfitItemDirectory)) Directory.CreateDirectory(outfitItemDirectory);
                    await File.WriteAllTextAsync($"{outfitItemDirectory}/{characterInfoOutfitItemPart.name}.json",
                        outfitItemDataObject.ToJson());
                }
                
            }

            if (modAsset.info is OutfitItemInfo outfitItemInfo)
            {
                Debug.Log("outfit item" + modAsset.addressableAddressId);

                if (outfitItemInfo.colors.Count == 0)
                {
                    var outfitItemDataObject = new OutfitItemDataObject
                    {
                        id = modAsset.addressableAddressId,
                        manikinPartAddressIds = new[] { modAsset.addressableAddressId },
                        minQuality = outfitItemInfo.minQuality,
                        belongsToBody = outfitItemInfo.belongsToBody,
                        coverSkinMeshRendererNames = outfitItemInfo.coverSkinMeshRendererNames,
                    };

                    if (!Directory.Exists(outfitItemDirectory)) Directory.CreateDirectory(outfitItemDirectory);
                    await File.WriteAllTextAsync($"{outfitItemDirectory}/{modAsset.addressableAddressId}.json",
                        outfitItemDataObject.ToJson());

                    if (modAsset.addressableAddressId.LastIndexOf("_Optional") ==
                        modAsset.addressableAddressId.Length - "_Optional".Length)
                    {
                        var optionalOutfitItemGroupDataObject = new OutfitItemGroupDataObject
                        {
                            id = modAsset.addressableAddressId,
                            outfitItemIds = new[]
                            {
                                "",
                                modAsset.addressableAddressId
                            }
                        };

                        if (!Directory.Exists(outfitItemGroupDirectory))
                            Directory.CreateDirectory(outfitItemGroupDirectory);
                        await File.WriteAllTextAsync(
                            $"{outfitItemGroupDirectory}/{optionalOutfitItemGroupDataObject.id}.json",
                            optionalOutfitItemGroupDataObject.ToJson());
                    }
                }
                else
                {
                    var outfitItemDataObject = new OutfitItemDataObject
                    {
                        id = modAsset.addressableAddressId,
                        manikinPartAddressIds = new[] { modAsset.addressableAddressId },
                        minQuality = outfitItemInfo.minQuality,
                        belongsToBody = outfitItemInfo.belongsToBody,
                        coverSkinMeshRendererNames = outfitItemInfo.coverSkinMeshRendererNames,
                        hexColors = outfitItemInfo.colors.ToArray()
                    };

                    if (!Directory.Exists(outfitItemDirectory)) Directory.CreateDirectory(outfitItemDirectory);
                    await File.WriteAllTextAsync($"{outfitItemDirectory}/{outfitItemDataObject.id}.json",
                        outfitItemDataObject.ToJson());

                    foreach (var color in outfitItemInfo.colors)
                    {
                        outfitItemDataObject.id = $"{modAsset.addressableAddressId}_{color}";
                        outfitItemDataObject.hexColors = new[] { color };

                        await File.WriteAllTextAsync($"{outfitItemDirectory}/{outfitItemDataObject.id}.json",
                            outfitItemDataObject.ToJson());
                    }

                    var coloredOutfitItemGroupDataObject = new OutfitItemGroupDataObject
                    {
                        id = modAsset.addressableAddressId,
                        outfitItemIds = outfitItemInfo.colors
                            .Select(color => $"{modAsset.addressableAddressId}_{color}").ToArray(),
                    };

                    if (!Directory.Exists(outfitItemGroupDirectory))
                        Directory.CreateDirectory(outfitItemGroupDirectory);
                    await File.WriteAllTextAsync($"{outfitItemGroupDirectory}/{modAsset.addressableAddressId}.json",
                        coloredOutfitItemGroupDataObject.ToJson());
                }
            }

            if (modAsset.info is WeaponInfo weaponInfo)
            {
                Debug.Log("weapon" + modAsset.addressableAddressId);
                var weaponDataObject = new WeaponDataObject
                {
                    id = modAsset.addressableAddressId,
                    weaponAddressId = modAsset.addressableAddressId,
                    displayInMenu = weaponInfo.displayInMenu
                };

                if (!Directory.Exists(weaponDirectory)) Directory.CreateDirectory(weaponDirectory);
                await File.WriteAllTextAsync($"{weaponDirectory}/{modAsset.addressableAddressId}.json",
                    weaponDataObject.ToJson());
            }
            
            if (modAsset.info is HairInfo hairInfo)
            {
                Debug.Log("hair" + modAsset.addressableAddressId);
                var hairDataObject = new HairDataObject
                {
                    id = modAsset.addressableAddressId,
                    hairAddressId = modAsset.addressableAddressId
                };

                if (!Directory.Exists(hairDirectory)) Directory.CreateDirectory(hairDirectory);
                await File.WriteAllTextAsync($"{hairDirectory}/{modAsset.addressableAddressId}.json",
                    hairDataObject.ToJson());
            }
        }

        foreach (var outfitPresetId in outfitPresetIds)
        {
            var outfitDataObject = new OutfitDataObject
            {
                id = outfitPresetId
            };

            if (!Directory.Exists(outfitDirectory)) Directory.CreateDirectory(outfitDirectory);
            await File.WriteAllTextAsync($"{outfitDirectory}/{outfitPresetId}.json",
                outfitDataObject.ToJson());
        }

        var outfitItemGroupParts = new List<string>
        {
            "Chests",
            "Capes",
            "Jackets",
            "Skirts",
            "Thighs",
            "Calves",
            "ForeArms",
            "Gloves",
            "Horns",
            "Glasses",
            "Headphones",
            "HairAccessories",
            "Shoes",
            "Bras",
            "Underwears",
            "Shoulders",
            "Mouths",
            "Necklaces",
            "Collars",
            "FullSuits",
            "Elbows",
            "Dresses",
            "AnkleArmors",
            "AnkleOrnaments",
            "Belts",
            "Bottoms",
            "EarRings",
            "Hairs",
            "KneeArmors",
            "LeftCapes",
            "LeftArms",
            "LeftForeArms",
            "LeftShoulders",
            "LeftThighs",
            "Masks",
            "LegStraps",
            "RightCapes",
            "RightArms",
            "RightForeArms",
            "RightShoulders",
            "RightThighs",
            "Tops"
        };

        foreach (var characterPresetId in characterPresetIds)
        {
            foreach (var outfitItemGroupPart in outfitItemGroupParts)
            {
                var outfitItemGroupDataObject = new OutfitItemGroupDataObject
                {
                    id = $"{characterPresetId}_{outfitItemGroupPart}",
                    name = outfitItemGroupPart,
                };

                if (!Directory.Exists(outfitItemGroupDirectory)) Directory.CreateDirectory(outfitItemGroupDirectory);

                var outfitItemGroupPath = $"{outfitItemGroupDirectory}/{outfitItemGroupDataObject.id}.json";

                if (!File.Exists(outfitItemGroupPath))
                {
                    await File.WriteAllTextAsync(outfitItemGroupPath,
                        outfitItemGroupDataObject.ToJson());
                }
            }
        }

        AssetDatabase.Refresh();
    }
}
#endif