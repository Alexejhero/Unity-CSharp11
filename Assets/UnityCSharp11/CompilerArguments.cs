using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace UnityCSharp11
{
    public static class CompilerArguments
    {
        public static void SetCompilerArgument(string key, string value, NamedBuildTarget? target = null)
        {
            if (!key.StartsWith('/'))
                key = '/' + key;
            target ??= NamedBuildTarget.Standalone;

            string[] compilerArgs = PlayerSettings.GetAdditionalCompilerArguments(target.Value);
            int existingIdx = Array.FindIndex(compilerArgs, arg => arg.StartsWith(key));
            string fullString = $"{key}:{value}";
            if (existingIdx >= 0)
            {
                if (compilerArgs[existingIdx] == fullString)
                    return;
                Debug.LogWarning($"Compiler argument {key} already set ({compilerArgs[existingIdx]}), overwriting ({fullString})");
                compilerArgs[existingIdx] = fullString;
            }
            else
            {
                compilerArgs = compilerArgs.Append(fullString).ToArray();
            }

            PlayerSettings.SetAdditionalCompilerArguments(target.Value, compilerArgs);
        }
    }
}
