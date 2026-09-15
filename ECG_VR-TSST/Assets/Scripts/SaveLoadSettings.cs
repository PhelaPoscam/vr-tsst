using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadSettings : MonoBehaviour
{
    [SerializeField] private Toggle _firstIsSecond;
    public Dropdown pruefer;
    public Dropdown announcement;
    public Dropdown sprache;
    public Text savedText;
    public Sprite[] allSprites;

    /// <summary>
    /// Gathers every information from the options menu
    /// </summary>
    public void Save()
    {
        Dropdown[] dropdowns = new Dropdown[3];
        dropdowns[0] = pruefer;
        dropdowns[1] = announcement;
        dropdowns[2] = sprache;
        string[] firstRound = getFirstRound();
        string[] secondRound = getSecondRound();
        ConvertSettings(dropdowns, firstRound, secondRound, _firstIsSecond);
    }

    /// <summary>
    /// Loads the existing settings file
    /// </summary>
    public void Load()
    {
        string path = Application.dataPath + "/Settings/Settings.txt";
        string str = "";
        Setting setting = new Setting();
        try
        {
            if (File.Exists(path))
            {
                str = File.ReadAllText(path);
            }
            else
            {
                Debug.LogWarning("Settings file does not exist at: " + path);
            }
        }
        catch(System.Exception e)
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
            catch(System.Exception e)
            {
                Debug.LogError("Fehler beim deserialisieren der Settings: " + e);
            }
        }

        if (setting.firstRound == null) setting.firstRound = new string[0];
        if (setting.secondRound == null) setting.secondRound = new string[0];

        if (pruefer != null)
        {
            pruefer.value = setting.oneAuditor ? 0 : 1;
        }

        if (announcement != null)
        {
            announcement.value = setting.secondRoundAnnouncement ? 0 : 1;
        }

        if (sprache != null)
        {
            sprache.value = setting.language;
        }

        if (_firstIsSecond != null)
        {
            _firstIsSecond.isOn = setting.firstIsSecond;
        }

        for(int i = 1; i <= 2; i++)
        {
            for(int j = 1; j <= 5; j++)
            {
                GameObject slot = GameObject.FindGameObjectWithTag(i + "R" + j + "T");
                if (slot != null && slot.transform.childCount > 0)
                {
                    Image img = slot.transform.GetChild(0).GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(1, 1, 1, 0);
                        img.sprite = null;
                    }
                }
            }
        }

        if (setting.firstRound != null && allSprites != null)
        {
            for(int i = 0; i < setting.firstRound.Length; i++)
            {
                if(!string.IsNullOrEmpty(setting.firstRound[i]))
                {
                    for(int j = 0; j < allSprites.Length; j++)
                    {
                        if (allSprites[j] != null && setting.firstRound[i].Equals(allSprites[j].name))
                        {
                            GameObject slot = GameObject.FindGameObjectWithTag("1R" + (i + 1) + "T");
                            if (slot != null && slot.transform.childCount > 0)
                            {
                                Image img = slot.transform.GetChild(0).GetComponent<Image>();
                                if (img != null)
                                {
                                    img.color = new Color(1, 1, 1, 1); 
                                    img.sprite = allSprites[j];
                                    img.SetNativeSize();
                                }
                            }
                        }
                    }
                }
            }
        }

        if (setting.secondRound != null && allSprites != null)
        {
            for (int i = 0; i < setting.secondRound.Length; i++)
            {
                if (!string.IsNullOrEmpty(setting.secondRound[i]))
                {
                    for (int j = 0; j < allSprites.Length; j++)
                    {
                        if (allSprites[j] != null && setting.secondRound[i].Equals(allSprites[j].name))
                        {
                            GameObject slot = GameObject.FindGameObjectWithTag("2R" + (i + 1) + "T");
                            if (slot != null && slot.transform.childCount > 0)
                            {
                                Image img = slot.transform.GetChild(0).GetComponent<Image>();
                                if (img != null)
                                {
                                    img.color = new Color(1, 1, 1, 1);
                                    img.sprite = allSprites[j];
                                    img.SetNativeSize();
                                }
                            }
                        }
                    }
                }
            }
        }

        if (savedText != null)
        {
            savedText.text = "Settings Loaded!";
            savedText.gameObject.SetActive(true);
            StartCoroutine("Wait");
        }
    }

    //Get every task dropped onto the first round drop area
    private string[] getFirstRound()
    {
        string[] firstRound = new string[5];
        for(int i = 0; i < firstRound.Length; i++)
        {
            GameObject slot = GameObject.FindGameObjectWithTag("1R" + (i + 1) + "T");
            if (slot != null && slot.transform.childCount > 0)
            {
                Image img = slot.transform.GetChild(0).GetComponent<Image>();
                if (img != null && img.sprite != null)
                {
                    firstRound[i] = img.sprite.name;
                }
            }
        }
        //delete empty spaces between tasks
        List<string> temp = new List<string>();
        foreach(string s in firstRound)
        {
            if (!string.IsNullOrEmpty(s))
            {
                temp.Add(s);
            }
        }
        firstRound = temp.ToArray();
        return firstRound;
    }

    //Get every task dropped onto the second round drop area
    private string[] getSecondRound()
    {
        string[] secondRound = new string[5];
        for (int i = 0; i < secondRound.Length; i++)
        {
            GameObject slot = GameObject.FindGameObjectWithTag("2R" + (i + 1) + "T");
            if (slot != null && slot.transform.childCount > 0)
            {
                Image img = slot.transform.GetChild(0).GetComponent<Image>();
                if (img != null && img.sprite != null)
                {
                    secondRound[i] = img.sprite.name;
                }
            }
        }
        //delete empty spaces between tasks
        List<string> temp = new List<string>();
        foreach (string s in secondRound)
        {
            if (!string.IsNullOrEmpty(s))
            {
                temp.Add(s);
            }
        }
        secondRound = temp.ToArray();
        return secondRound;
    }

    /// <summary>
    /// Converts the gathered settings into a json objct and writes the object into a settingss file
    /// </summary>
    /// <param name="dropdowns"></param>
    /// <param name="firstRound"></param>
    /// <param name="secondRound"></param>
    private void ConvertSettings(Dropdown[] dropdowns, string[] firstRound, string[] secondRound, Toggle t)
    {
        bool oneAuditor = (dropdowns != null && dropdowns.Length > 0 && dropdowns[0] != null) ? dropdowns[0].value == 0 : false;
        bool secondRoundAnnouncement = (dropdowns != null && dropdowns.Length > 1 && dropdowns[1] != null) ? dropdowns[1].value == 0 : true;
        byte language = (dropdowns != null && dropdowns.Length > 2 && dropdowns[2] != null) ? (byte)dropdowns[2].value : (byte)0;

        if (firstRound == null) firstRound = new string[0];
        if (secondRound == null) secondRound = new string[0];

        if(firstRound.Length == 0 && secondRound.Length != 0)
        {
            firstRound = secondRound;
            secondRound = new string[0];
        }

        bool firstIsSecond = t != null && t.isOn;

        Setting currentSetting = new Setting(oneAuditor, secondRoundAnnouncement, firstRound, secondRound, language, firstIsSecond);
        string str = JsonUtility.ToJson(currentSetting);
        Directory.CreateDirectory(Application.dataPath + "/Settings");
        string path = Application.dataPath + "/Settings/Settings.txt";

        try
        {
            File.WriteAllText(path, str);
        }
        catch (IOException e)
        {
            Debug.Log("Fehler beim schreiben der Datei:" + e);
        }

        if (savedText != null)
        {
            savedText.text = "Settings Saved!";
            savedText.gameObject.SetActive(true);
            StartCoroutine("Wait");
        }
    }

    /// <summary>
    /// disables the "Settings Saved!" text after two seconds
    /// </summary>
    /// <returns></returns>
    IEnumerator Wait()
    {
        yield return new WaitForSecondsRealtime(2f);
        if (savedText != null)
        {
            savedText.gameObject.SetActive(false);
        }
    }
}
