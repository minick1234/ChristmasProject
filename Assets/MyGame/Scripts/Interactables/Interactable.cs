using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    #region Interactable Fields

    public string interactableName;

    #endregion

    #region Interactable Abstract Methods

    public abstract void Interact();
    public abstract void FinishedInteracting();

    #endregion

    #region Interactable Methods

    public string GetNameOfInteractable()
    {
        return interactableName;
    }

    public void SetNameOfInteractable(string name)
    {
        interactableName = name;
    }

    #endregion
}