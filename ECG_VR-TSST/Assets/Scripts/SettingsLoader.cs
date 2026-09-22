using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Single place the settings file is read from. Task scripts call this many
/// times per state transition, so the parsed result is cached and only re-read
/// when the file on disk actually changes.
/// </summary>
public static class SettingsLoader
{
    public static string Path
    {
        get { return Application.persistentDataPath + "/Settings/Settings.txt"; }
    }

    private static Setting s_cached;
    private static DateTime s_cachedWriteTime;

    public static Setting Load()
    {
        string path = Path;

        DateTime writeTime;
        try
        {
            writeTime = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
        }
        catch (IOException)
        {
            writeTime = DateTime.MinValue;
        }

        if (s_cached != null && writeTime == s_cachedWriteTime) return s_cached;

        s_cached = ReadFrom(path);
        s_cachedWriteTime = writeTime;
        return s_cached;
    }

    private static Setting ReadFrom(string path)
    {
        Setting setting = null;

        try
        {
            if (File.Exists(path))
            {
                string str = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(str)) setting = JsonUtility.FromJson<Setting>(str);
            }
            else
            {
                Debug.LogWarning("[SettingsLoader] No settings file at: " + path + " - using defaults. Configure a round in the Settings menu and save it.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[SettingsLoader] Could not read settings: " + e);
        }

        if (setting == null) setting = new Setting();
        if (setting.firstRound == null) setting.firstRound = new string[0];
        if (setting.secondRound == null) setting.secondRound = new string[0];
        return setting;
    }
}
