using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChairHandler : MonoBehaviour {

    public GameObject secondChair;
    public GameObject thirdChair;
    private Setting setting;

    /// <summary>
    /// get settings file from data folder and save as Settings Object
    /// </summary>
    public void Awake()
    {
        string path = Application.dataPath + "/Settings/Settings.txt";
        string str = null;
        setting = new Setting();

        try
        {
            if (File.Exists(path))
            {
                str = File.ReadAllText(path);
                Debug.Log("ausgelesen");
            }
            else
            {
                Debug.LogWarning("Settings file does not exist at: " + path);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Fehler beim auslesen der Datei:" + e);
        }

        if (!string.IsNullOrEmpty(str)) 
        {
            try
            {
                Setting loaded = JsonUtility.FromJson<Setting>(str);
                if (loaded != null) setting = loaded;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Fehler beim deserialisieren der Settings: " + e);
            }
        }

        setChairs(setting);
    }

    /// <summary>
    /// Get NPC count from settings object and set the chairs accordingly
    /// </summary>
    /// <param name="setting"></param>
    public void setChairs(Setting setting)
    {
        if (setting == null) return;
        if (secondChair != null) secondChair.SetActive(!setting.oneAuditor);
        if (thirdChair != null) thirdChair.SetActive(!setting.oneAuditor);
    }
}
