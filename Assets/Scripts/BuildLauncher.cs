#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using System;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

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

    [MenuItem("VRE/VRE Builder")]
    public static bool BuildAddressables()
    {
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
        }

        if (builderScript == null)
        {
            Debug.LogError(build_script + " couldn't be found or isn't a build script.");
            return false;
        }

        setBuilder(builderScript);

        return buildAddressableContent();
    }
}
#endif