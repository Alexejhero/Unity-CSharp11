using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace UnityPatcher
{
    internal static class Program
    {
        private static string _editorPath;
        private static string _assetsPath;

        private static bool _stillSilent = true;

        private static bool _elevated;

        public static void Main(string[] args)
        {
            string unityPath = null;
            string projectPath = null;

            if (args.Contains("-ns")) _stillSilent = false;
            if (args.Contains("-e")) _elevated = true;

            try
            {
                Execute(args, out unityPath, out projectPath);
            }
            catch (Exception e)
            {
                Logger.Error.WriteLines("",
                    "Program interrupted due to exception",
                    e.ToString());
            }

            bool canRelaunch = !string.IsNullOrEmpty(unityPath) && !string.IsNullOrEmpty(projectPath);

            if (_elevated && canRelaunch)
            {
                _stillSilent = false;
                Logger.Warning.WriteLines("",
                    "Unity will now be relaunched.",
                    "You might get a warning about launching Unity as administrator.",
                    "If that happens, click 'Restart as standard user'.");
            }

            if (!_stillSilent)
            {
                Logger.Info.WriteLines("",
                    "Press any key to exit...");
                Console.ReadKey();
            }

            if (unityPath != null && projectPath != null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = unityPath,
                    Arguments = $"-projectPath \"{projectPath}\""
                });
            }
        }

        private static void Execute(string[] args, out string unityPath, out string projectPath)
        {
            unityPath = projectPath = null;

            Logger.Message.WriteLines(
                "Unity 2022+ File Scoped Namespaces Patcher",
                "by Alexejhero",
                "");

            GetDllPath(args, out string dllPath);
            if (!CheckDllPath(dllPath, true)) return;

            bool patched = CheckPatch(dllPath, out AssemblyDefinition asm);

            if (!TryGetIntention(args, patched, out bool intentionToPatch)) return;

            GetUnityLaunchPath(args, out unityPath, out projectPath);

            if (!intentionToPatch) DoUnpatch(dllPath, asm);
            else DoPatch(dllPath, asm);
        }

        private static void GetDllPath(string[] args, out string dllPath)
        {
            string editorFolder;
            if (args.Length < 1)
            {
                _stillSilent = false;
                Logger.Info.WriteLines(
                    "Please enter the path to the Editor\\Data folder of the Unity version that you want to patch/unpatch.",
                    "This is the same as EditorApplication.applicationContentsPath in Unity.",
                    @"Example: C:\Program Files\Unity\Hub\Editor\2022.3.37f1\Editor\Data",
                    "");
                Logger.Info.Write("> ");
                editorFolder = Console.ReadLine();
                Logger.Info.WriteLines("");
            }
            else
            {
                editorFolder = args[0];
            }

            dllPath = PathCombine(editorFolder, "Tools", "Unity.SourceGenerators", "Unity.SourceGenerators.dll");
            _editorPath = editorFolder;
        }

        private static void GetUnityLaunchPath(string[] args, out string unityPath, out string projectPath)
        {
            if (args.Length < 3 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[2]))
            {
                unityPath = null;
                projectPath = null;
                return;
            }

            unityPath = PathCombine(args[0], "..", "Unity.exe");
            projectPath = PathCombine(args[2], "..");

            _assetsPath = args[2];
        }

        private static bool CheckDllPath(string dllPath, bool initial)
        {
            Logger.Debug.WriteLines("Checking path...");
            if (!File.Exists(dllPath))
            {
                _stillSilent = false;
                Logger.Error.WriteLines(
                    "ERROR!",
                    $"Unity.SourceGenerators.dll not found at path {dllPath}");
                if (initial)
                {
                    Logger.Warning.WriteLines(
                        "This could mean that you have entered an incorrect path, or that the Unity version you are trying to patch does not have this file.",
                        "Keep in mind Unity versions before 2022 (2021 and below) are not supported.");
                }
                return false;
            }

            return true;
        }

        private static bool TryGetIntention(string[] args, bool isAlreadyPatched, out bool intentionToPatch)
        {
            if (args.Contains("-p"))
            {
                intentionToPatch = true;
                return true;
            }

            if (args.Contains("-u"))
            {
                intentionToPatch = false;
                return true;
            }

            _stillSilent = false;

            if (isAlreadyPatched)
            {
                intentionToPatch = false;

                Logger.Info.WriteLines(
                    "",
                    "Unity seems to already be patched.",
                    "There is no reason to unpatch it, as it will not cause any problems or conflict with other projects.",
                    "However, if you still want to unpatch, you can do so.",
                    "");

                Logger.Info.Write("Do you want to unpatch? (y/N) ");
                return YesNoChoice(false);
            }
            else
            {
                intentionToPatch = true;

                Logger.Info.WriteLines(
                    "",
                    "Unity does NOT seem to be patched.",
                    "");

                Logger.Info.Write("Do you want to patch? (Y/n) ");
                return YesNoChoice(true);
            }
        }

        private static bool CheckPatch(string dllPath, out AssemblyDefinition asm)
        {
            Logger.Debug.WriteLines("Checking patch status...");
            asm = AssemblyDefinition.ReadAssembly(dllPath);
            return asm.MainModule.GetTypes().Any(t => t.Name == "CSharp11NamespacePatch");
        }

        private static void DoUnpatch(string dllPath, AssemblyDefinition asm)
        {
            asm?.Dispose();

            string backupPath = dllPath + ".bck";

            if (!File.Exists(backupPath))
            {
                _stillSilent = false;
                Logger.Error.WriteLines(
                    "",
                    $"Missing backup file at path {backupPath}",
                    "Unpatching not possible",
                    "If you want to unpatch, please redownload Unity");
                return;
            }

            try
            {
                File.Delete(dllPath);
                File.Move(backupPath, dllPath);

                Logger.Success.WriteLines("Backup restored successfully. Unity is now unpatched.");
            }
            catch (Exception e)
            {
                _stillSilent = false;

                Logger.Error.WriteLines("",
                    "Unpatch failed due to exception:",
                    e.ToString());
                Logger.Warning.WriteLines("",
                    "This might indicate missing permissions or a file lock.",
                    "If Unity is open, please close it and try again.",
                    "If Unity is installed in Program Files or another protected directory, you might need to run this program as administrator.");
                Logger.Info.WriteLines("",
                    "What do you want to do?",
                    "1. Retry",
                    "2. Retry as administrator",
                    "3. Perform unpatch manually",
                    "(Press any other key to exit)",
                    "");
                Logger.Info.Write("> ");

                ConsoleKeyInfo key = Console.ReadKey();
                Logger.Info.WriteLines("", "");

                if (key.Key == ConsoleKey.D1) DoUnpatch(dllPath, null);
                else if (key.Key == ConsoleKey.D2) RelaunchAsAdmin($@"""{_editorPath}"" -u ""{_assetsPath}"" -e {(!_stillSilent ? " -ns" : "")}");
                else if (key.Key == ConsoleKey.D3) DoManualUnpatch(dllPath);
            }
        }

        private static void DoManualUnpatch(string dllPath)
        {
            Logger.Message.WriteLines(
                "The Unity.SourceGenerators folder has been opened in your file explorer application.",
                "Please follow these steps for a manual unpatch:",
                "1. Delete Unity.SourceGenerators.dll",
                "2. Rename Unity.SourceGenerators.dll.bck to Unity.SourceGenerators.dll",
                "",
                "After this is done, press any key to verify if the unpatch was successful.",
                "");

            Process.Start("explorer.exe", $"/select,\"{Path.GetFullPath(dllPath)}\"");
            Console.ReadKey(true);

            if (!CheckDllPath(dllPath, false))
            {
                Logger.Info.WriteLines("",
                    "Keep in mind that if you're lost you can always redownload Unity to restore the original file");
                Logger.Info.Write("Do you want to retry verification? (Y/n) ");
                if (YesNoChoice(true)) DoManualUnpatch(dllPath);
                Logger.Info.WriteLines("");
                return;
            }

            if (CheckPatch(dllPath, out AssemblyDefinition asm))
            {
                asm.Dispose();
                Logger.Warning.WriteLines("",
                    "Assembly seems to still be patched.");
                Logger.Info.Write("Do you want to retry verification? (Y/n) ");
                if (YesNoChoice(true)) DoManualUnpatch(dllPath);
                Logger.Info.WriteLines("");
                return;
            }

            Logger.Success.WriteLines("",
                "Unpatch successful.",
                "");
        }

        private static void DoPatch(string dllPath, AssemblyDefinition asm)
        {
            Logger.Debug.WriteLines("Patching...");

            TypeDefinition type = asm.MainModule.GetType("Unity.MonoScriptGenerator.TypeNameHelper");
            MethodDefinition method = type?.Methods.First(m => m.Name == "GetTypeInformation");

            if (method == null || !method.HasBody)
            {
                Console.WriteLine("PATCH FAILED!");
                Console.WriteLine("Cannot find method body for Unity.MonoScriptGenerator.TypeNameHelper.GetTypeInformation");
                Console.WriteLine();
                Console.WriteLine("This should never happen in theory, but if it does, it might mean that you are using a newer, unsupported version of Unity.");
                Console.WriteLine("This package has only been tested with Unity versions 2022, 2023 and 6000.");
                Console.WriteLine();
                return;
            }

            Logger.Debug.WriteLines($"Found method {method.FullName}");

            const string typeToReplace = "Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax";
            int changes = 0;

            AssemblyNameReference reference = asm.MainModule.AssemblyReferences.First(a => a.Name == "Microsoft.CodeAnalysis.CSharp");
            TypeReference typeRef = new TypeReference("Microsoft.CodeAnalysis.CSharp.Syntax", "BaseNamespaceDeclarationSyntax", asm.MainModule, reference);

            TypeReference imported = asm.MainModule.ImportReference(typeRef);

            Logger.Debug.WriteLines($"Imported reference to {imported.FullName}");

            foreach (VariableDefinition local in method.Body.Variables)
            {
                if (local.VariableType.FullName == typeToReplace)
                {
                    Logger.Debug.WriteLines($"Updating local variable {local.Index}");
                    local.VariableType = imported;
                    changes++;
                }
            }

            foreach (Instruction instruction in method.Body.Instructions)
            {
                if (instruction.Operand is TypeReference t && t.FullName == typeToReplace)
                {
                    Logger.Debug.WriteLines($"Updating instruction {instruction} ...");
                    instruction.Operand = imported;
                    changes++;
                }
                else if (instruction.Operand is MemberReference member && member.DeclaringType != null && member.DeclaringType.FullName == typeToReplace)
                {
                    Logger.Debug.WriteLines($"Updating instruction {instruction} ...");
                    member.DeclaringType = imported;
                    changes++;
                }
            }

            if (asm.MainModule.GetType("CSharp11NamespacePatch") == null)
            {
                Logger.Debug.WriteLines("Creating signature type CSharp11NamespacePatch ...");
                TypeDefinition patchType = new TypeDefinition("", "CSharp11NamespacePatch", TypeAttributes.Class | TypeAttributes.Public, asm.MainModule.TypeSystem.Object);
                asm.MainModule.Types.Add(patchType);
                changes++;
            }

            Logger.Debug.WriteLines("",
                "Finished making changes");

            if (changes <= 0)
            {
                _stillSilent = false;
                Logger.Error.WriteLines("",
                    "No changes made. Assembly seems to be already patched somehow, even though previous patch checks failed.",
                    "This should never happen...");
                return;
            }

            Logger.Debug.WriteLines("Saving assembly in temporary folder...");

            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);

            string tempDll = PathCombine(tempFolder, "Unity.SourceGenerators.dll");
            asm.Write(tempDll);

            Logger.Debug.WriteLines("Backing up original assembly...");

            string tempBck = tempDll + ".bck";
            File.Copy(dllPath, tempBck);

            asm.Dispose();

            DoPatchSave(tempDll, tempBck, dllPath, dllPath + ".bck");
        }

        private static void DoPatchSave(string tempDll, string tempBck, string targetDll, string targetBck)
        {
            Logger.Debug.WriteLines("Replacing original assembly...");

            try
            {
                File.Copy(tempBck, targetBck, true);
                File.Copy(tempDll, targetDll, true);

                Logger.Success.WriteLines("Patch successful. Please restart Unity!");
            }
            catch (Exception e)
            {
                _stillSilent = false;

                Logger.Error.WriteLines("",
                    "Patch failed due to exception:",
                    e.ToString());
                Logger.Warning.WriteLines("",
                    "This might indicate missing permissions or a file lock.",
                    "If Unity is open, please close it and try again.",
                    "If Unity is installed in Program Files or another protected directory, you might need to run this program as administrator.");
                Logger.Info.WriteLines("",
                    "What do you want to do?",
                    "1. Retry",
                    "2. Retry as administrator",
                    "3. Perform patch manually",
                    "(Press any other key to exit)",
                    "");
                Logger.Info.Write("> ");

                ConsoleKeyInfo key = Console.ReadKey();
                Logger.Info.WriteLines("", "");

                if (key.Key == ConsoleKey.D1) DoPatchSave(tempDll, tempBck, targetDll, targetBck);
                else if (key.Key == ConsoleKey.D2) RelaunchAsAdmin($@"""{_editorPath}"" -p ""{_assetsPath}"" -e {(!_stillSilent ? " -ns" : "")}");
                else if (key.Key == ConsoleKey.D3) DoManualPatchSave(tempDll, targetDll);
            }
        }

        private static void DoManualPatchSave(string tempDll, string targetDll)
        {
            Logger.Message.WriteLines(
                "Two directories have been opened in your file explorer application.",
                "Please move the Unity.SourceGenerators.dll and Unity.SourceGenerators.dll.bck file from the temporary directory to the Unity.SourceGenerators directory, overriding any existing files.",
                "Important! If you don't move the .bck file as well, you will not be able to unpatch Unity after.",
                "",
                "After this is done, press any key to verify if the patch was successful.",
                "");

            Process.Start("explorer.exe", $"/select,\"{Path.GetFullPath(tempDll)}\"");
            Process.Start("explorer.exe", $"/select,\"{Path.GetFullPath(targetDll)}\"");
            Console.ReadKey(true);

            if (!CheckDllPath(targetDll, false))
            {
                Logger.Info.WriteLines("",
                    "Keep in mind that if you're lost you can always redownload Unity to restore the original file");
                Logger.Info.Write("Do you want to retry verification? (Y/n) ");
                if (YesNoChoice(true)) DoManualPatchSave(tempDll, targetDll);
                Logger.Info.WriteLines("");
                return;
            }

            if (!CheckPatch(targetDll, out AssemblyDefinition asm))
            {
                asm.Dispose();
                Logger.Info.WriteLines("",
                    "Assembly seems to NOT be patched.");
                Logger.Info.Write("Do you want to retry verification? (Y/n) ");
                if (YesNoChoice(true)) DoManualPatchSave(tempDll, targetDll);
                Logger.Info.WriteLines("");
                return;
            }

            Logger.Success.WriteLines("",
                "Patch successful.",
                "");
        }

        private static bool YesNoChoice(bool def)
        {
            ConsoleKey defKey = def ? ConsoleKey.Y : ConsoleKey.N;

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Y && key.Key != ConsoleKey.N && key.Key != ConsoleKey.Enter) continue;

                ConsoleKey pressedKey = key.Key;
                if (pressedKey == ConsoleKey.Enter) pressedKey = defKey;

                if (pressedKey == ConsoleKey.Y)
                {
                    Console.WriteLine("Y");
                    Console.WriteLine();
                    return true;
                }
                else
                {
                    Console.WriteLine("N");
                    Console.WriteLine();
                    return false;
                }
            }
        }

        private static void RelaunchAsAdmin(string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                // ReSharper disable once PossibleNullReferenceException
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Arguments = args,
                Verb = "runas"
            };

            Process.Start(startInfo);

            Logger.Message.WriteLines("",
                "Spawned separate process with administrator privileges");

            Environment.Exit(0);
        }

        private static string PathCombine(params string[] paths)
        {
            string result = paths[0];
            for (int i = 1; i < paths.Length; i++)
            {
                result = Path.Combine(result, paths[i]);
            }

            return result;
        }
    }
}
