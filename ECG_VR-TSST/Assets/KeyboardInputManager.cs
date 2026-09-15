using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardInputManager : MonoBehaviour {

    [SerializeField]
    private Button weiter;
    [SerializeField]
    private Button erneut;
    [SerializeField]
    private Button zeitNichtUm;
    [SerializeField]
    private Button nichtVerstanden;
    [SerializeField]
    private Button lautUndDeutlich;
    [SerializeField]
    private Button vonVorne;
    [SerializeField]
    private Button falsch;
    [SerializeField]
    private Button richtig;
    [SerializeField]
    private Button ergebnis;

    /// <summary>
    /// Listen for the number keys one through nine and initiate button presses accordingly
    /// </summary>
    public void Update()
    {
        CheckButton(KeyCode.Alpha1, weiter);
        CheckButton(KeyCode.Alpha2, erneut);
        CheckButton(KeyCode.Alpha3, zeitNichtUm);
        CheckButton(KeyCode.Alpha4, nichtVerstanden);
        CheckButton(KeyCode.Alpha5, lautUndDeutlich);
        CheckButton(KeyCode.Alpha6, vonVorne);
        CheckButton(KeyCode.Alpha7, falsch);
        CheckButton(KeyCode.Alpha8, richtig);
        CheckButton(KeyCode.Alpha9, ergebnis);
    }

    /// <summary>
    /// Triggers the button if it is assigned and its key was pressed this frame
    /// </summary>
    /// <param name="key"></param>
    /// <param name="button"></param>
    private void CheckButton(KeyCode key, Button button)
    {
        if (button != null && Input.GetKeyDown(key))
        {
            button.onClick.Invoke();
        }
    }
}
