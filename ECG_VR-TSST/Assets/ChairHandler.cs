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
        setting = SettingsLoader.Load();
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
