using UnityEngine;
using UnityEngine.Events;

public abstract class AbstractTask : MonoBehaviour
{

    [SerializeField]
    protected GenericButton[] _optionalButtons;
    protected GenericButton[] OptionalButtons
    {
        get { return _optionalButtons; }
    }

    [SerializeField]
    protected UnityEvent _finished;
    public UnityEvent Finished
    {
        get { return _finished; }
    }

    /// <summary>
    /// Sets up the object and its references
    /// </summary>
    protected abstract void InactiveSetup();

    /// <summary>
    /// Path of the current session's log file, read from the "GameController"-tagged
    /// GameManager. Returns null (and logs a warning once) if that object or its
    /// GameManager component is missing from the scene, instead of throwing.
    /// </summary>
    protected string GetLogPath()
    {
        GameObject gameManagerObject = GameObject.FindGameObjectWithTag("GameController");
        GameManager gameManager = gameManagerObject != null ? gameManagerObject.GetComponent<GameManager>() : null;
        if (gameManager == null)
        {
            Debug.LogWarning("[" + GetType().Name + "] No \"GameController\"-tagged GameManager found in the scene; log entry not written.");
            return null;
        }
        return gameManager.pathValue;
    }

    /// <summary>
    /// Length of a CharacterAction's clip for the given language, clamped the same way
    /// ProfessorRitter.PerformAction clamps it - so a clip missing for a language never
    /// throws IndexOutOfRangeException when used to drive a Timer.
    /// </summary>
    protected float ClipLength(CharacterAction action, byte language)
    {
        if (action == null || action.Clip == null || action.Clip.Length == 0)
            return 0f;
        int index = (language < action.Clip.Length) ? language : 0;
        AudioClip clip = action.Clip[index];
        return clip != null ? clip.length : 0f;
    }

    /// <summary>
    /// Call this right before the first Next call
    /// </summary>
    [ContextMenu("Init")]
    protected virtual void Init()
    {
        if (OptionalButtonSwitcher.Instance != null)
        {
            OptionalButtonSwitcher.Instance.EnableButtons(_optionalButtons);
        }
    }

    /// <summary>
    /// Changes object state
    /// </summary>
    /// <returns>true if Next() Operation was successful, false if Finish() should be called</returns>
    [ContextMenu("Next")]
    public abstract bool Next();

    /// <summary>
    /// Changes object state
    /// Overloaded to check the place this task is at to choose the character actions accordingly
    /// </summary>
    /// <returns>true if Next() Operation was successful, false if Finish() should be called</returns>
    [ContextMenu("Next")]
    public abstract bool Next(Round round ,bool secondRoundActive, bool secondRoundAvailable);

    /// <summary>
    /// Finishes an object
    /// </summary>
    [ContextMenu("Finish")]
    protected virtual void Finish()
    {
	    InactiveSetup();
	    if (OptionalButtonSwitcher.Instance != null)
        {
            OptionalButtonSwitcher.Instance.DisableAll();
        }
        if (_finished != null)
        {
            _finished.Invoke();
        }
    }

    /// <summary>
    ///Restarts the object
    /// </summary>
    [ContextMenu("Restart")]
    public virtual void Restart()
    {
        InactiveSetup();
        Init();
        Next();
    }

    /// <summary>
    ///Restarts the object
    /// </summary>
    [ContextMenu("Restart")]
    public virtual void Restart(Round round, bool secondRoundActive, bool secondRoundAvailable)
    {
        InactiveSetup();
        Init();
        Next(round, secondRoundActive, secondRoundAvailable);
    }

    protected void OnEnable()
    {
        InactiveSetup();
    }

    protected void OnDisable()
    {
        InactiveSetup();
    }

}
