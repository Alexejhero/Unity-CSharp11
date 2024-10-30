## Unity C#11
Unity package based on [Unity Roslyn Updater](https://github.com/DaZombieKiller/UnityRoslynUpdater) to enable using C#11 features (**including [file-scoped namespaces](#file-scoped-namespaces)**) in Unity 2022.

Designed only to work on Windows. It would be good to get this working on other platforms but since I only use Windows, that's what I've made this for. (PR welcome 🙂)

#### Installation and Use
Install from this git URL (see [How to install from a git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html)):
```
https://github.com/Alexejhero/Unity-CSharp11.git?path=Assets/UnityCSharp11
```
After installing, [regenerate your C# project files](https://docs.unity3d.com/Manual/VisualStudioIntegration.html) - your project will now target C#11.

### Features
Most C# 10 and C# 11 features will work. Please refer to the [Language Support](https://github.com/DaZombieKiller/UnityRoslynUpdater?tab=readme-ov-file#c-11) section of Unity Roslyn Updater's README to see which features are not available. Additionally, the Roslyn version that Unity uses **does not** support _file-local types_, so that will also be unavailable. **C#12 features are not supported!**

#### File-scoped Namespaces
By default, Unity 2022 fails to detect `MonoBehaviour` or `ScriptableObject` classes from files which use [file-scoped namespaces](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/file-scoped-namespaces).

This package includes a patcher that modifies your local Unity Editor installation to fix this issue. Any other team members will also need to patch their editors to work with file-scoped namespaces.

A window will display at launch prompting you to patch the editor. After closing it, you can manually patch or unpatch again in `Preferences > C# 11`.

The patching process is reversible, so you may roll back at any time.

### Compatibility
#### [Hot Reload](https://hotreload.net)
If you have Hot Reload installed, you will also need to install the package from this git URL:
```
https://github.com/Alexejhero/Unity-CSharp11.git?path=Assets/UnityCSharp11.HotReload
```
Hot Reload generates their own csproj files when compiling changes, and this package ensures they are also processed to support C#11.
#### [CsprojModifier](https://github.com/Cysharp/CsprojModifier)
This package automatically sets up csproj properties and compiler arguments for C#11, so you can remove any LangVersion modifications set in CsprojModifier.

### Motivation
This project was created as an alternative to [Unity Roslyn Updater](https://github.com/DaZombieKiller/UnityRoslynUpdater). Distributing the patcher as a Unity package makes it easy to install and use for all team members.

Unity Roslyn Updater replaces the editor's bundled Roslyn compiler (which, as of Unity 2022, supports up to C#11) with the latest version of the .NET SDK you have installed on your computer (if any). This allows Unity Roslyn Updater to support C#12 and beyond, but does not scale to multi-person teams due to the large possible variety of user environments - e.g. a developer using features that require a recent SDK would force artists to install that SDK to be able to compile the project.

For this reason, our package does not replace the compiler, so that all team members have access to the same language features.
