using System;
using System.IO;
using UnityEditor;

namespace UnityCSharp11.Settings;

public static class Config
{
    private const string IGNORE_FILE = "Library/com.alexejhero.unitycsharp11/ignore";

    private static bool _showDialog = true;

    public static bool ShowPatchDialog
    {
        get => _showDialog;
        set
        {
            if (ShowPatchDialog == value) return;
            _showDialog = value;

            if (_showDialog) FileUtil.DeleteFileOrDirectory(IGNORE_FILE);
            else File.WriteAllText(IGNORE_FILE, string.Empty);

            ShowPatchDialogChanged?.Invoke(value);
        }
    }

    public static event Action<bool> ShowPatchDialogChanged;

    [InitializeOnLoadMethod]
    private static void LoadConfig()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(IGNORE_FILE)!);
        _showDialog = !File.Exists(IGNORE_FILE);

        if (PatchChecker.IsPatched()) ShowPatchDialog = true;
    }
}
