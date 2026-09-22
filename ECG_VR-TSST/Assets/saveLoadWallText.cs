using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class saveLoadWallText : MonoBehaviour {

    /// <summary>
    /// Check if a File for the Waiting Room texts already exist. If thats not the case, make one, so that on each fresh boot in a new environment a text is available
    /// </summary>
    private void Awake()
    {
        Directory.CreateDirectory(Application.persistentDataPath + "/WallText");

        // Seed each language file independently - a missing one used to leave the
        // waiting room walls blank if only the other language had been written.
        SeedIfMissing(0, new WaitingRoomText("Bitte haben Sie noch einen Moment Geduld! Sie können gleich zu dem/den Prüfer(n) ins Büro, um mit den Aufgaben zu starten.", "Im später folgenden Gespräch werden sowohl Bild als auch Ton aufgezeichnet werden!", "Bitte stehen Sie auf.\r\nDer/Die Prüfer ist/sind jetzt für Sie bereit."));
        SeedIfMissing(1, new WaitingRoomText("Please be patient for a moment! You can go straight to the auditor(s) into the office to start with the tasks any moment.", "In the following conversation both picture and sound will be recorded!", "Please stand up.\r\nThe auditor(s) is/are now ready for you."));
    }

    private void SeedIfMissing(int language, WaitingRoomText defaults)
    {
        string path = Application.persistentDataPath + "/WallText/WallText" + language + ".txt";

        try
        {
            if (File.Exists(path) && !string.IsNullOrEmpty(File.ReadAllText(path))) return;
            File.WriteAllText(path, JsonUtility.ToJson(defaults));
        }
        catch (System.Exception e)
        {
            Debug.LogError("[saveLoadWallText] Could not seed wall text " + language + ": " + e);
        }
    }
    
}
