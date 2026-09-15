using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "ChracterAction", menuName = "CharacterAction")]
public class CharacterAction : ScriptableObject
{

    [SerializeField]
    private AudioClip[] _clip;
    public AudioClip[] Clip
    {
        get
        {
            return _clip;
        }
    }

    [SerializeField]
    private string _animationTrigger;
    public string AnimationTrigger
    {
        get
        {
            return _animationTrigger;
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_animationTrigger) && _clip != null && _clip.Length > 0)
        {
            Setting s = LoadSettings();
            int lang = (s != null && s.language < _clip.Length) ? (int)s.language : 0;
            if (_clip[lang] != null)
            {
                _animationTrigger = _clip[lang].name;
            }
        }
        if (string.IsNullOrEmpty(_animationTrigger) || _clip == null)
        {
            Debug.LogError("[" + this.GetType() + "] A value is null or empy");
        }
        name = _animationTrigger;
    }

    private Setting LoadSettings()
    {
        string path = Application.dataPath + "/Settings/Settings.txt";
        Setting setting = new Setting();

        try
        {
            if (File.Exists(path))
            {
                string str = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(str))
                {
                    Setting loaded = JsonUtility.FromJson<Setting>(str);
                    if (loaded != null) setting = loaded;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Fehler beim auslesen der Datei:" + e);
        }

        return setting;
    }

}