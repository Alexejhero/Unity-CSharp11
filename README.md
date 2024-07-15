## Unity C#11
Unity package based on [Unity Roslyn Updater](https://github.com/DaZombieKiller/UnityRoslynUpdater) to enable using C#11 features in Unity 2022.

Designed only to work on Windows.

#### Installation and Use
Install from this git URL: `https://github.com/Alexejhero/Unity-CSharp11.git?path=Assets/UnityCSharp11` (see [How to install from a git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html))

After installing, [Regenerate your C# project files](https://docs.unity3d.com/Manual/VisualStudioIntegration.html) - your project now targets C#11.

Please refer to the [Language Support](https://github.com/DaZombieKiller/UnityRoslynUpdater?tab=readme-ov-file#c-11) section of Unity Roslyn Updater's README to see which features of C#11 will work.

#### File-scoped Namespaces
By default, Unity 2022 can no longer properly detect `MonoBehaviour`s if you use [file-scoped namespaces](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/file-scoped-namespaces).

This package includes a patcher that will **modify your local Unity Editor installation** to fix this issue.

Any other team members will also need to patch their editors to work with file-scoped namespaces.

The patching process is reversible, so you may roll back at any time.

### Compatibility

#### [Hot Reload](https://hotreload.net)
- If you have Hot Reload installed, you will also need to install from this git URL: `https://github.com/Alexejhero/Unity-CSharp11.git?path=Assets/UnityCSharp11.HotReload`

#### [CsprojModifier](https://github.com/Cysharp/CsprojModifier)
- This project automatically sets up csproj elements and compiler arguments for C#11, so you can remove any LangVersion modifications set in CsprojModifier.
