using System;
using UnityEngine;

public class TempController : MonoBehaviour
{
    public GameObject target;
    
    private void OnEnable()
    {
        TouchInputHandler.Instance.OnObjectSelected += Select;
        TouchInputHandler.Instance.OnSelectionCleared += Unselect;
    }

    private void Select(TouchInteractor obj)
    {
        target = obj.gameObject;
    }

    private void Unselect()
    {
        target = null;
    }
}
