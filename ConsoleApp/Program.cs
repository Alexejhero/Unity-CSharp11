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
        private static string EditorFolderForRelaunch { get; set; }

        public static void Main(string[] args)
        {
            try
            {
                Execute(args);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("Program interrupted due to exception");
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void Execute(string[] args)
        {
            Console.WriteLine("Unity 2022+ File Scoped Namespaces Patcher");
            Console.WriteLine("by Alexejhero");
            Console.WriteLine();

            GetDllPath(args, out string dllPath);
            if (!CheckDllPath(dllPath, true)) return;

            Console.WriteLine("Checking patch status...");
            bool patched = CheckPatch(dllPath, out AssemblyDefinition asm);

            if (!TryGetIntention(args, patched, out bool intentionToPatch)) return;

            if (!intentionToPatch) DoUnpatch(dllPath, asm);
            else DoPatch(dllPath, asm);
        }

        private static void GetDllPath(string[] args, out string dllPath)
        {
            string editorFolder;
            if (args.Length < 1)
            {
                Console.WriteLine("Please enter the path to the Editor\\Data folder of the Unity version that you want to patch/unpatch.");
                Console.WriteLine("This is the same as EditorApplication.applicationContentsPath in Unity.");
                Console.WriteLine("Example: C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.37f1\\Editor\\Data");
                Console.WriteLine();
                Console.Write("> ");
                editorFolder = Console.ReadLine();
                Console.WriteLine();
            }
            else
            {
                editorFolder = args[0];
            }

            dllPath = PathCombine(editorFolder, "Tools", "Unity.SourceGenerators", "Unity.SourceGenerators.dll");
            EditorFolderForRelaunch = editorFolder;
        }

        private static bool CheckDllPath(string dllPath, bool initial)
        {
            Console.WriteLine("Checking path...");
            if (!File.Exists(dllPath))
            {
                Console.WriteLine("ERROR!");
                Console.WriteLine($"Unity.SourceGenerators.dll not found at path {dllPath}");
                if (initial)
                {
                    Console.WriteLine("This could mean that you have entered an incorrect path, or that the Unity version you are trying to patch does not have this file.");
                    Console.WriteLine("Keep in mind Unity versions before 2022 (2021 and below) are not supported.");
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

            if (isAlreadyPatched)
            {
                intentionToPatch = false;

                Console.WriteLine();
                Console.WriteLine("Unity seems to already be patched.");
                Console.WriteLine("There is no reason to unpatch it, as it will not cause any problems or conflict with other projects.");
                Console.WriteLine("However, if you still want to unpatch, you can do so.");
                Console.WriteLine();

                Console.Write("Do you want to unpatch? (y/N) ");
                return YesNoChoice(false);
            }
            else
            {
                intentionToPatch = true;

                Console.WriteLine();
                Console.WriteLine("Unity does NOT seem to be patched.");

                Console.WriteLine();

                Console.Write("Do you want to patch? (Y/n) ");
                return YesNoChoice(true);
            }
        }

        private static bool CheckPatch(string dllPath, out AssemblyDefinition asm)
        {
            asm = AssemblyDefinition.ReadAssembly(dllPath);
            return asm.MainModule.GetTypes().Any(t => t.Name == "CSharp11NamespacePatch");
        }

        private static void DoUnpatch(string dllPath, AssemblyDefinition asm)
        {
            asm?.Dispose();

            string backupPath = dllPath + ".bck";

            if (!File.Exists(backupPath))
            {
                Console.WriteLine($"Missing backup file at path {backupPath}");
                Console.WriteLine("Unpatching not possible");
                Console.WriteLine("If you want to unpatch, please redownload Unity");
                return;
            }

            try
            {
                File.Delete(dllPath);
                File.Move(backupPath, dllPath);

                Console.WriteLine("Backup restored successfully. Unity is now unpatched.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unpatch failed due to exception:");
                Console.WriteLine(e.ToString());
                Console.WriteLine();
                Console.WriteLine("This might indicate missing permissions or a file lock.");
                Console.WriteLine("If Unity is open, please close it and try again.");
                Console.WriteLine("If Unity is installed in Program Files or another protected directory, you might need to run this program as administrator.");
                Console.WriteLine();
                Console.WriteLine("What do you want to do?");
                Console.WriteLine("1. Retry");
                Console.WriteLine("2. Retry as administrator");
                Console.WriteLine("3. Perform unpatch manually");
                Console.WriteLine("(Press any other key to exit)");
                Console.WriteLine();
                Console.Write("> ");
                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();
                Console.WriteLine();
                if (key.Key == ConsoleKey.D1) DoUnpatch(dllPath, null);
                else if (key.Key == ConsoleKey.D2) RelaunchAsAdmin($@"""{EditorFolderForRelaunch}"" -u");
                else if (key.Key == ConsoleKey.D3) DoManualUnpatch(dllPath);
            }
        }

        private static void DoManualUnpatch(string dllPath)
        {
            Console.WriteLine("The Unity.SourceGenerators folder has been opened in your file explorer application.");
            Console.WriteLine("Please follow these steps for a manual unpatch:");
            Console.WriteLine("1. Delete Unity.SourceGenerators.dll");
            Console.WriteLine("2. Rename Unity.SourceGenerators.dll.bck to Unity.SourceGenerators.dll");
            Console.WriteLine();
            Console.WriteLine("After this is done, press any key to verify if the unpatch was successful.");
            Console.WriteLine();

            Process.Start("explorer.exe", "/select,\"" + dllPath + "\"");
            Console.ReadKey();

            if (!CheckDllPath(dllPath, false))
            {
                Console.WriteLine();
                Console.WriteLine("Keep in mind that if you're lost you can always redownload Unity to restore the original file");
                Console.Write("Do you want to retry verification? (Y/n) ");
                if (YesNoChoice(true)) DoManualUnpatch(dllPath);
                Console.WriteLine();
                return;
            }

            if (CheckPatch(dllPath, out AssemblyDefinition asm))
            {
                asm.Dispose();
                Console.WriteLine();
                Console.WriteLine("Assembly seems to still be patched.");
                Console.Write("Do you want to retry verification? (Y/n) ");
                if (YesNoChoice(true)) DoManualUnpatch(dllPath);
                Console.WriteLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Unpatch successful.");
            Console.WriteLine();
        }

        private static void DoPatch(string dllPath, AssemblyDefinition asm)
        {
            Console.WriteLine("Patching...");

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

            Console.WriteLine($"Found method {method.FullName}");

            const string typeToReplace = "Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax";
            int changes = 0;

            AssemblyNameReference reference = asm.MainModule.AssemblyReferences.First(a => a.Name == "Microsoft.CodeAnalysis.CSharp");
            TypeReference typeRef = new TypeReference("Microsoft.CodeAnalysis.CSharp.Syntax", "BaseNamespaceDeclarationSyntax", asm.MainModule, reference);

            TypeReference imported = asm.MainModule.ImportReference(typeRef);

            Console.WriteLine($"Imported reference to {imported.FullName}");

            foreach (VariableDefinition local in method.Body.Variables)
            {
                if (local.VariableType.FullName == typeToReplace)
                {
                    Console.WriteLine($"Updating local variable {local.Index}");
                    local.VariableType = imported;
                    changes++;
                }
            }

            foreach (Instruction instruction in method.Body.Instructions)
            {
                if (instruction.Operand is TypeReference t && t.FullName == typeToReplace)
                {
                    Console.WriteLine($"Updating instruction {instruction} ...");
                    instruction.Operand = imported;
                    changes++;
                }
                else if (instruction.Operand is MemberReference member && member.DeclaringType != null && member.DeclaringType.FullName == typeToReplace)
                {
                    Console.WriteLine($"Updating instruction {instruction} ...");
                    member.DeclaringType = imported;
                    changes++;
                }
            }

            if (asm.MainModule.GetType("CSharp11NamespacePatch") == null)
            {
                Console.WriteLine("Creating signature type CSharp11NamespacePatch ...");
                TypeDefinition patchType = new TypeDefinition("", "CSharp11NamespacePatch", TypeAttributes.Class | TypeAttributes.Public, asm.MainModule.TypeSystem.Object);
                asm.MainModule.Types.Add(patchType);
                changes++;
            }

            Console.WriteLine();
            Console.WriteLine("Finished making changes");

            if (changes <= 0)
            {
                Console.WriteLine();
                Console.WriteLine("No changes made. Assembly seems to be already patched somehow, even though previous patch checks failed.");
                Console.WriteLine("This should never happen...");
                return;
            }

            Console.WriteLine("Saving assembly in temporary folder...");

            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);

            string tempDll = PathCombine(tempFolder, "Unity.SourceGenerators.dll");
            asm.Write(tempDll);

            Console.WriteLine("Backing up original assembly...");

            string tempBck = tempDll + ".bck";
            File.Copy(dllPath, tempBck);

            asm.Dispose();

            DoPatchSave(tempDll, tempBck, dllPath, dllPath + ".bck");
        }

        private static void DoPatchSave(string tempDll, string tempBck, string targetDll, string targetBck)
        {
            Console.WriteLine("Replacing original assembly...");

            try
            {
                File.Copy(tempBck, targetBck, true);
                File.Copy(tempDll, targetDll, true);

                Console.WriteLine("Patch successful. Please restart Unity!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Patch failed due to exception:");
                Console.WriteLine(e.ToString());
                Console.WriteLine();
                Console.WriteLine("This might indicate missing permissions or a file lock.");
                Console.WriteLine("If Unity is open, please close it and try again.");
                Console.WriteLine("If Unity is installed in Program Files or another protected directory, you might need to run this program as administrator.");
                Console.WriteLine();
                Console.WriteLine("What do you want to do?");
                Console.WriteLine("1. Retry");
                Console.WriteLine("2. Retry as administrator");
                Console.WriteLine("3. Perform patch manually");
                Console.WriteLine("(Press any other key to exit)");
                Console.WriteLine();
                Console.Write("> ");
                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();
                Console.WriteLine();
                if (key.Key == ConsoleKey.D1) DoPatchSave(tempDll, tempBck, targetDll, targetBck);
                else if (key.Key == ConsoleKey.D2) RelaunchAsAdmin($@"""{EditorFolderForRelaunch}"" -p");
                else if (key.Key == ConsoleKey.D3) DoManualPatchSave(tempDll, targetDll);
            }
        }

        private static void DoManualPatchSave(string tempDll, string targetDll)
        {
            Console.WriteLine("Two directories have been opened in your file explorer application.");
            Console.WriteLine("Please move the Unity.SourceGenerators.dll and Unity.SourceGenerators.dll.bck file from the temporary directory to the Unity.SourceGenerators directory, overriding any existing files.");
            Console.WriteLine();
            Console.WriteLine("After this is done, press any key to verify if the patch was successful.");
            Console.WriteLine();

            Process.Start("explorer.exe", "/select,\"" + tempDll + "\"");
            Process.Start("explorer.exe", "/select,\"" + targetDll + "\"");
            Console.ReadKey();

            if (!CheckDllPath(targetDll, false))
            {
                Console.WriteLine();
                Console.WriteLine("Keep in mind that if you're lost you can always redownload Unity to restore the original file");
                Console.Write("Do you want to retry verification? (Y/n) ");
                if (YesNoChoice(true)) DoManualPatchSave(tempDll, targetDll);
                Console.WriteLine();
                return;
            }

            if (!CheckPatch(targetDll, out AssemblyDefinition asm))
            {
                asm.Dispose();
                Console.WriteLine();
                Console.WriteLine("Assembly seems to NOT be patched.");
                Console.Write("Do you want to retry verification? (Y/n) ");
                if (YesNoChoice(true)) DoManualPatchSave(tempDll, targetDll);
                Console.WriteLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Patch successful.");
            Console.WriteLine();
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

            Console.WriteLine();
            Console.WriteLine("Spawned separate process with administrator privileges");
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
