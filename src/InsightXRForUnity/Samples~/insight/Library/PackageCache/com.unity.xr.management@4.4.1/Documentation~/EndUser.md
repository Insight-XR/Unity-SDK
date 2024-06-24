---
uid: xr-plug-in-management-end-user
---
# Advanced configuration for plug-in management

To use the XR Plug-in Manager to manage XR plug-ins and settings, refer to the following documentation in the Unity Manual:

| Topic | Description |
| :---  | :---        |
| [Choose XR provider plug-ins](xref:xr-configure-providers) | How to configure your project to use specific XR provider plug-ins, such as OpenXR. |
| [XR Plug-in Management settings](xref:xr-plugin-management) | Reference information describing the XR Plug-in Management settings. |

The topics on this page describe more advanced uses of the XR Plug-in Manager and its API. Refer to this information when:

* You want to control when XR provider plug-ins are loaded and initialized
* You want to decide at runtime which XR provider plug-in to use
* You want to set the order in which the XR provider plug-in manager attempts to load plug-ins
* You want to assign values to XR provider plug-in settings at build or runtime using a script

<a id="automatic-xr-loading"></a>
<a id="load-xr-plug-ins"></a>
## Load XR plug-ins

By default, XR Plug-in Management initializes automatically and starts your XR environment when the application loads. At runtime, this happens immediately before the first Scene loads. In Play mode, this happens immediately after the first Scene loads, but before `Start` is called on your GameObjects. In both scenarios, XR should be set up before calling the MonoBehaviour [Start](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html) method, so you should be able to query the state of XR in the `Start` method of your GameObjects.

Alternately, you can wait until after the `Start` event completes and manually initialize XR.

> [!IMPORTANT]
> Do not start manual initialization of XR until after the Unity `Start` event completes. The Unity graphics system must be completely initialized before you can manually initialize XR, which isn't guaranteed until after `Start` is finished. (Automatic XR initialization take places before graphics initialization, but there's no way to invoke manual XR initalization early enough.) 

If you want to start XR on a per-Scene basis (for example, to start in 2D and transition into VR), follow these steps:

1. Access the **Project Settings** window (menu: **Edit** &gt; **Project Settings**).
2. Select the **XR Plug-in Management** tab on the left.
3. Disable the **Initialize on start** option for each platform you support.
4. At runtime, call the following methods on `XRGeneralSettings.Instance.Manager` to add/create, remove, and reorder the Loaders from your scripts:

|Method|Description|
|---|---|
|`InitializeLoader(Async)`|Sets up the XR environment to run manually. Should be called on or after [Start](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html) has finished to avoid conflicts with graphics initialization sequence.|
|`StartSubsystems`|Starts XR and puts your application into XR mode.|
|`StopSubsystems`|Stops XR and takes your application out of XR mode. You can call `StartSubsystems` again to go back into XR mode.|
|`DeinitializeLoader`|Shuts down XR and removes it entirely. You must call `InitializeLoader(Async)` before you can run XR again.|

To handle pause state changes in the Editor, subscribe to the [`EditorApplication.pauseStateChanged`](https://docs.unity3d.com/ScriptReference/EditorApplication-pauseStateChanged.html) API, then stop and start the subsystems according to the new pause state that the `pauseStateChange` delegate method returns.

The following code shows an example of how to manually control XR using XR Plug-in Management:

```csharp
using System.Collections;
using UnityEngine;

using UnityEngine.XR.Management;

public class ManualXRControl
{
    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }

    void StopXR()
    {
        Debug.Log("Stopping XR...");

        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }
}
```

<a id="managing-xr-loader-lifecycles-manually"></a>
<a id="manage-xr-loader-lifecycles"></a>
## Manage XR loader lifecycles

The previous section showed how to manage the entire XR system lifecycle. If you require more granular control, you can manage an individual loader's lifecycle instead.

### API

You can use the following methods in your script to control the lifecycle of XR manually:

|**Method**|**Description**|
|---|---|
|`XRLoader.Initialize`|Sets up the XR environment to run manually and initializes all subsystems for the XR loader.|
|`XRLoader.Start`|Starts XR and requests the XR loader to start all initialized subsystems.|
|`XRLoader.Stop`|Stops XR and requests the XR loader to stop all initialized subsystems. You can call `StartSubsystems` again to go back into XR mode.|
|`XRLoader.Deinitialize`|Shuts down the XR loader and de-initializes all initialized subsystems. You must call `XRLoader.Initialize` before you can use the loader again.|

The following code example demonstrates how to manage individual loaders at runtime.

> [!IMPORTANT]
> The following example circumvents XR Management Lifecycle control. When you take this route, you must manually manage the lifecycle of all the loaders initialized in this manner. APIs that expect to use XR Plug-In Management to acquire subsystems from a loader will not function properly when manually handling loader lifecycles.
>
> If you need a specific loader initialized but want that loader to still be managed by XR Plug-In Management, look into the '[Modifying the Loader List](#example-modifying-the-loader-list)' section on how to do that.

### Example

```csharp
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class RuntimeXRLoaderManager : MonoBehaviour
{
    XRLoader m_SelectedXRLoader;

    void StartXR(int loaderIndex)
    {
        // Once a loader has been selected, prevent the RuntimeXRLoaderManager from
        // losing access to the selected loader
        if (m_SelectedXRLoader == null)
        {
            m_SelectedXRLoader = XRGeneralSettings.Instance.Manager.activeLoaders[loaderIndex];
        }
        StartCoroutine(StartXRCoroutine());
    }

    IEnumerator StartXRCoroutine()
    {
        Debug.Log("Init XR loader");

        var initSuccess = m_SelectedXRLoader.Initialize();
        if (!initSuccess)
        {
            Debug.LogError("Error initializing selected loader.");
        }
        else
        {
            yield return null;
            Debug.Log("Start XR loader");
            var startSuccess = m_SelectedXRLoader.Start();
            if (!startSuccess)
            {
                yield return null;
                Debug.LogError("Error starting selected loader.");
                m_SelectedXRLoader.Deinitialize();
            }
        }
    }

    void StopXR()
    {
        Debug.Log("Stopping XR Loader...");
        m_SelectedXRLoader.Stop();
        m_SelectedXRLoader.Deinitialize();
        m_SelectedXRLoader = null;
        Debug.Log("XR Loader stopped completely.");
    }
}
```

## Use XR Plug-In Management to initialize a specific loader

Sometimes, you may want to include multiple loaders in a build and have them fall through in a specific order. By default, XR Plug-In Management will attempt to initialize the loader in alphabetical order based on the loaders' name. If this isn't adequate you can modify the loader list in Edit Mode, Play Mode, and in a built player with some caveats.

1) In Edit Mode, you may modify the loaders list without restriction.

2) You may reorder or remove loaders from the loader list at runtime. A new loader that wasn't known at startup can't be added to the loader list at runtime. Any attempt to add an unknown loader to the list at runtime will fail.

    This means that you are able to do the following during runtime:

    - Remove loaders from the list of loaders.
    - Re-add loaders that were previously removed.
    - Reorder the list of loaders.

3) Any operation on the XR Plug-in Manager UI will reset the ordering to the original alphabetical ordering.

<a id="example-modifying-the-loader-list"></a>
### Example: Modify the active loader list at runtime

If you wish to reorder the set of loaders so XR Plug-In Management attempts to initialize a specific loader first you could do the following at runtime:

```csharp
    var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTarget.Standalone);
    var settingsManager = generalSettings.Manager;

    // Get a readonly reference to the current set of loaders.
    var readonlyCurrentLoaders = settingsManager.activeLoaders;

    // Modifying the loader list order
    var reorderedLoadersList = new List<XRLoader>();
    foreach (var loader in readonlyCurrentLoaders)
    {
#if UNITY_ANDROID
        if (loader is XRFooLoader)
        {
            // Insert 'Foo' Loaders at head of list
            reorderedLoaderList.Insert(loader, 0);
        }
        else if (loader is XRBarLoader)
        {
            // Insert 'Bar' Loaders at back of list
            reorderedLoaderList.Insert(loader, reorderedLoaderList.Count);
        }
#else // !UNITY_ANDROID
        if (loader is XRBarLoader)
        {
            // Insert 'Bar' Loaders at head of list
            reorderedLoaderList.Insert(loader, 0);
        }
        else if (loader is XRFooLoader)
        {
            // Insert 'Foo' Loaders at back of list
            reorderedLoaderList.Insert(loader, reorderedLoaderList.Count);
        }
#endif // end UNITY_ANDROID
    }

    // Would only fail if the new list contains a loader that was
    // not in the original list.
    if (!settingsManager.TrySetLoaders(reorderedLoadersList))
        Debug.Log("Failed to set the reordered loader list! Refer to the documentation for additional information!");

```

### Example: general modifications of the loader list

You can also modify the loader list in other more general ways. The following example shows how to use `TryAdd`, `TryRemove`, and `TrySet` in a variety of ways.

```csharp
    var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTarget.Standalone);
    var settingsManager = generalSettings.Manager;

    // Get example loaders as XRLoaders
    var fooLoader = new FooLoader() as XRLoader;
    var barLoader = new BarLoader() as XRLoader;

    // Adding new loaders
    // Append the new FooLoader
    if (!settingsManager.TryAddLoader(fooLoader))
        Debug.Log("Adding new Foo Loader failed! Refer to the documentation for additional information!");

    // Insert the new BarLoader at the start of the list
    if (!settingsManager.TryAddLoader(barLoader, 0))
        Debug.Log("Adding new Bar Loader failed! Refer to the documentation for additional information!");

    // Removing loaders
    if (!settingsManager.TryRemoveLoader(fooLoader))
        Debug.Log("Failed to remove the fooLoader! Refer to the documentation for additional information!");

    // Modifying the loader list order
    var readonlyCurrentLoaders = settingsManager.activeLoaders;

    // Copy the returned read only list
    var currentLoaders = new List<XRLoader>(readonlyCurrentLoaders);

    // Reverse the list
    currentLoaders.Reverse();

    if (!settingsManager.TrySetLoaders(currentLoaders))
        Debug.Log("Failed to set the reordered loader list! Refer to the documentation for additional information!");
```

You would most likely place this script in a custom pre-process build script, but that isn't required. Regardless of the script's location, you should do this as a setup step before you start a build as XR Plug-in Manager will serialize this list as part of the build execution.

<a id="customizing build and runtime settings"></a>
## Customize build and runtime settings

Any package that needs build or runtime settings should provide a settings data type for use. This data type appears in the **Project Settings** window, underneath a top level **XR** node.

You can use scripts to configure the settings for a specific plug-in, or change the active and inactive plug-ins per build target.

### Example: Access custom settings

> [!NOTE]
> The code in this example doesn't install any plug-ins for you. Make sure your plug-ins are installed and available before you try this script.

```csharp
    var metadata = XRPackageMetadataStore.GetMetadataForPackage(my_pacakge_id);
    assets = AssetDatabase.FindAssets($"t:{metadata.settingsType}");
    var assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);

    // Settings access is type specific. You will need information from your plug-in documentation
    // to know how to get at specific instances and properties.

    // You must know the type of the settings you are accessing.
    var directInstance  = AssetDatabase.LoadAssetAtPath(assetPath, typeof(full.typename.for.pluginsettings));

    // You must know the access method for getting build target specific settings data.
    var buildTargetSettings = directInstance.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);

    // Do something with settings...

    // Mark instance dirty and save any changes.
    EditorUtility.SetDirty(directInstance);
    AssetDatabase.SaveAssets();
```

### Example: Configure plug-ins per build target

> [!NOTE]
> The code in this example doesn't install any plug-ins for you. Make sure your plug-ins are installed and available before you try this script.

To add a plug-in to the set of assigned plug-ins for a build target:

```csharp
    var buildTargetSettings = XRGeneralSettingsPerBuildTarget.SettingsForBuildTarget(BuildTarget.Standalone);
    var pluginsSettings = buildTargetSettings.AssignedSettings;
    var didAssign = XRPackageMetadataStore.AssignLoader(pluginsSettings, "full.typename.for.pluginloader", BuildTargetGroup.Standalone);

    if (!didAssign)
    {
        // Report error or do something here.
        ...
    }
```

To remove a plug-in from the set of assigned plug-ins for a build target:

```csharp
    var buildTargetSettings = XRGeneralSettingsPerBuildTarget.SettingsForBuildTarget(BuildTarget.Standalone);
    var pluginsSettings = buildTargetSettings.AssignedSettings;
    var didRemove = XRPackageMetadataStore.RemoveLoader(pluginsSettings, "full.typename.for.pluginloader", BuildTargetGroup.Standalone);

    if (!didRemove)
    {
        // Report error or do something here.
        ...
    }
```
