using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : Interactable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public override void Interact()
    {
        Debug.Log("I am " + GetNameOfInteractable());
        Debug.Log("I am of type: " + this.GetType());
    }

    public override void FinishedInteracting()
    {
        throw new System.NotImplementedException();
    }
    
}
