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

    private void OnEnable()
    {
        if (Drop == null) return;
        Drop.onValueChanged.AddListener(UpdateInteractable);
        UpdateInteractable(Drop.value);
    }

    private void OnDisable()
    {
        if (Drop == null) return;
        Drop.onValueChanged.RemoveListener(UpdateInteractable);
    }

    private void UpdateInteractable(int value)
    {
        bool interactable = value != 0;
        if (Vortrag != null) Vortrag.interactable = interactable;
        if (Rueckwaerts != null) Rueckwaerts.interactable = interactable;
        if (Reihen != null) Reihen.interactable = interactable;
        if (Kopfrechnen != null) Kopfrechnen.interactable = interactable;
    }

}
