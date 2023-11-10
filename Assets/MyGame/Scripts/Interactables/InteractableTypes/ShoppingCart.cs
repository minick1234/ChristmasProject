using System;
using System.Collections;
using System.Collections.Generic;
using EasyCharacterMovement;
using UnityEngine;
using UnityEngine.Events;

public class ShoppingCart : Interactable
{
    //Down below there is two ways to pass this event with the correct arguement for the object,
    //we can either do it using a delegate which focuses on what is being sent,
    //or we can use a custom class that is of type eventargs and pass whatever through there which is a bit mroe 
    //dynamic but its also more lengthy as we need to have that class created. The way i have uncommented as of now is the more industry standard way
    //and is considered the better option as it is type safe and can prevent issues down the line.
    
    //In short terms they are basically the same thing because a event handler is a delegate that takes in a Object parameter named sender, and a teventargs which is a object that extends eventargs,
    //so realistically they are the same thing and i could use the option i have commented out but for clarity sake, why remake something that already exists as it is the same shit anyway,
    //might as well use the eventhanlder one and have it just done for me and make the custom class to pass the correct params, and if im not passing params just use Eventargs.empty
    //its just more convienient even though the commented out way makes it so much faster in terms of passing info.
    

 //   public delegate void RandomDelegate(GameObject thing);
   // public event RandomDelegate ShoppingCartInteracted_Event;
    
     public event EventHandler<ShoppingCartInteractEventArgs> ShoppingCartInteracted_Event;

    private OurFirstPersonCharacter FPSCharacter;

    private FixedJoint shoppingCartFixedJoint;
    
    // Start is called before the first frame update
    void Start()
    {
        FPSCharacter = GameObject.FindObjectOfType<OurFirstPersonCharacter>();
        shoppingCartFixedJoint = gameObject.GetComponent<FixedJoint>();
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    public override void Interact()
    {
        ShoppingCartInteracted_Event +=
            FPSCharacter.OnShoppingCartInteracted;
        
        //Assign the event params for the player object
        ShoppingCartInteractEventArgs shoppingCartInteractEventArgs = new ShoppingCartInteractEventArgs();
        shoppingCartInteractEventArgs.ShoppingCartObject = this.gameObject;

        //Check if the event is null or unsubscribed
        if (ShoppingCartInteracted_Event != null)
        {
            //If it has been subscribed correctly and is not null we call the function
            ShoppingCartInteracted_Event(this, shoppingCartInteractEventArgs);
        }
        
        ShoppingCartInteracted_Event -=
            GameObject.FindObjectOfType<OurFirstPersonCharacter>().OnShoppingCartInteracted;

        if (ShoppingCartInteracted_Event == null)
        {
            Debug.Log("I have emptied this event");
        }
       
    }

    public void EnableTheJoint()
    {
        
    }

    public void AttachPlayerToShoppingCartJoint()
    {
        Transform characterRoot = FPSCharacter.gameObject.transform.Find("Root");

        if (characterRoot != null)
        {
            gameObject.transform.parent = characterRoot;
        }
        
        shoppingCartFixedJoint.connectedBody = FPSCharacter.GetComponent<Rigidbody>();
        shoppingCartFixedJoint.enableCollision = true;
    }

    public void DetachPlayerFromShoppingCartJoint()
    {
        shoppingCartFixedJoint.connectedBody = null;
        shoppingCartFixedJoint.enableCollision = false;
    }
    
    /*
    public override void Interact()
    {
        //Subscribe to the event so it knows what functions to call.
        ShoppingCartInteracted_Event +=
            FPSCharacter.OnShoppingCartInteracted;

        //Assign the event params for the player object
        ShoppingCartInteractEventArgs shoppingCartInteractEventArgs = new ShoppingCartInteractEventArgs();
        shoppingCartInteractEventArgs.ShoppingCartObject = this.gameObject;

        //Check if the event is null or unsubscribed
        if (ShoppingCartInteracted_Event != null)
        {
            //If it has been subscribed correctly and is not null we call the function
            ShoppingCartInteracted_Event(this.gameObject);
        }

        //Unsubscribe from the event so it is then set as null and cannot be bothered again if this is raised somewhere else
        ShoppingCartInteracted_Event -=
            GameObject.FindObjectOfType<OurFirstPersonCharacter>().OnShoppingCartInteracted;

    }
    */

    public override void FinishedInteracting()
    {
        throw new System.NotImplementedException();
    }
}

public class ShoppingCartInteractEventArgs : EventArgs
{
    public GameObject ShoppingCartObject { get; set; }
}

