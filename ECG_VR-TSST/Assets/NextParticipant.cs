using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextParticipant : MonoBehaviour{

    [SerializeField]
    private InputField inputField;


    /// <summary>
    /// Function for the Next button in the main Menu
    /// Increments the participant number by one
    /// </summary>
    public void OnButtonClicked()
    {
        if (inputField == null) return;
        string text = inputField.text;
        if (int.TryParse(text, out int number))
        {
            inputField.text = (number + 1).ToString();
        }
        else
        {
            inputField.text = "1";
        }
    }
}
