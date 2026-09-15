using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownListener : MonoBehaviour
{

    public Toggle Vortrag;
    public Toggle Rueckwaerts;
    public Toggle Reihen;
    public Toggle Kopfrechnen;
    public Dropdown Drop;
    private bool toggleActive;
    private void Update()
    {
        if (Drop == null) return;

        bool interactable = Drop.value != 0;
        if (Vortrag != null) Vortrag.interactable = interactable;
        if (Rueckwaerts != null) Rueckwaerts.interactable = interactable;
        if (Reihen != null) Reihen.interactable = interactable;
        if (Kopfrechnen != null) Kopfrechnen.interactable = interactable;
    }

}
