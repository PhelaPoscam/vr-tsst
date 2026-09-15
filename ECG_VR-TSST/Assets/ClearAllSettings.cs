using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearAllSettings : MonoBehaviour {

    [SerializeField]
    private Dropdown numberOfNPCs;

    [SerializeField]
    private Dropdown language;

    [SerializeField]
    private Dropdown secondRoundAnnouncement;

    [SerializeField]
    private Image[] allTasks;

    [SerializeField]
    private Toggle t;

    /// <summary>
    /// Set back every UI Element to its standard state
    /// </summary>
    public void onButtonClicked()
    {
        if (numberOfNPCs != null) numberOfNPCs.value = 0;
        if (language != null) language.value = 0;
        if (secondRoundAnnouncement != null) secondRoundAnnouncement.value = 0;
        if (t != null) t.isOn = false;
        if (allTasks != null)
        {
            foreach(Image i in allTasks)
            {
                if (i != null)
                {
                    i.color = new Color(1, 1, 1, 0);
                    i.sprite = null;
                }
            }
        }
    }
}
