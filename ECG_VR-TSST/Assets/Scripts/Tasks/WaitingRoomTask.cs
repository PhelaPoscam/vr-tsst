using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class WaitingRoomTask : AbstractTask
{

    private enum State
    {
        Inactive,
        Initialized,
        InitalMessage,
        SurveillanceMessage,
        ReadyMessage,
        Finished,
    }

    [SerializeField]
    private float _timeBetweenMessages = 300;

    [SerializeField]
    private float _timeAfterSecondMessage = 10;

    [SerializeField]
    private AudioSource _readySound;

    public Text _initalText;

    public Text _surveillanceText;

    public Text _readyText;

    private State _state;

    /// <summary>
    /// Setup all important values for this task
    /// </summary>
    [ContextMenu("Inactive Setup")]
    protected override void InactiveSetup()
    {
        if (_readyText != null) _readyText.enabled = false;
        if (_surveillanceText != null) _surveillanceText.enabled = false;
        if (_initalText != null) _initalText.enabled = false;
        _state = State.Inactive;
    }

    /// <summary>
    /// Initialize task by initializing all important values
    /// </summary>
    [ContextMenu("Init")]
    protected override void Init()
    {
        base.Init();
        _state = State.Initialized;
    }

    /// <summary>
    /// Main Logic of task. Each call will go to the next state if the state has changed between calls
    /// </summary>
    /// <returns></returns>

    public override bool Next(Round round, bool secondRoundActive, bool secondRoundAvailable)
    {
        return Next();
    }

    /// <summary>
    /// Overloaded Next()-method to use in tasks that require the auditor to talk 
    /// </summary>
    /// <param name="round"></param>
    /// <param name="secondRoundActive"></param>
    /// <param name="secondRoundAvailable"></param>
    /// <returns></returns>
    [ContextMenu("Next")]
    public override bool Next()
    {
        WriteLogEntry();
        Debug.Log("[WaitingRoomTask] " + _state);
        switch (_state)
        {
            case State.Inactive:
                Init();
                return Next();
            case State.Initialized:
                _state = State.InitalMessage;
                _initalText.enabled = true;
                getWallTexts();
                Timer.Instance.StartTimer(_timeBetweenMessages, Timer.Mode.Automatic);
                return true;
            case State.InitalMessage:
                _state = State.SurveillanceMessage;
                _initalText.enabled = false;
                _surveillanceText.enabled = true;
	            Timer.Instance.StartTimer(_timeAfterSecondMessage, Timer.Mode.Automatic);
                return true;
            case State.SurveillanceMessage:
                _state = State.ReadyMessage;
                _surveillanceText.enabled = false;
                _readyText.enabled = true;
                _readySound.Play();
                Timer.Instance.StartTimer(_timeAfterSecondMessage, Timer.Mode.Automatic);
                return true;
            case State.ReadyMessage:
                _state = State.Finished;
                return Next();
            case State.Finished:
                Finish();
                return false;
            default:
                Debug.LogError("[WaitingRoomManager] Invalid state.");
                return false;
        }
    }

    /// <summary>
    /// Call Finish from base class
    /// </summary>
    [ContextMenu("Finish")]
    protected override void Finish()
    {
        base.Finish();
    }

    /// <summary>
    /// Call restart from main class
    /// </summary>
    [ContextMenu("Restart")]
    public override void Restart()
    {
        base.Restart();
    }

    /// <summary>
    /// Writes log entry for current task
    /// </summary>
    void WriteLogEntry()
    {
        string path = GetLogPath();
        if (path == null) return;

        string textToWrite = "Current task: \"WaitingRoomTask\"; Current state of Task: \"" + _state.ToString() + "\" at " + System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "\r\n";

        try
        {
            File.AppendAllText(path, textToWrite);
        }
        catch (IOException e)
        {
            Debug.Log("Fehler beim schreiben der Datei:" + e);
        }
    }

    /// <summary>
    /// Get all three wall txt shown in the waiting room from the file holding the texts with the currently selected language
    /// </summary>
    private void getWallTexts()
    {
        string str = "";
        byte language = LoadSettings().language;
        string path = Application.persistentDataPath + "/WallText/WallText" + language + ".txt";

        WaitingRoomText waitText = new WaitingRoomText();

        try
        {
            if (File.Exists(path)) str = File.ReadAllText(path);
            else Debug.LogWarning("[WaitingRoomTask] No wall text file at: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[WaitingRoomTask] Could not read wall text: " + e);
        }

        if (!string.IsNullOrEmpty(str))
        {
            WaitingRoomText loaded = JsonUtility.FromJson<WaitingRoomText>(str);
            if (loaded != null) waitText = loaded;
        }

        if (_initalText != null) _initalText.text = waitText.initialText ?? "";
        if (_surveillanceText != null) _surveillanceText.text = waitText.surveillanceText ?? "";
        if (_readyText != null) _readyText.text = waitText.readyText ?? "";
    }

    /// <summary>
    /// Load sssettings from file
    /// </summary>
    /// <returns></returns>
    private Setting LoadSettings()
    {
        return SettingsLoader.Load();
    }

}