using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class InteractionManager : NetworkBehaviour
{
    public GameObject ItemParent;
    public GameObject Item;
    //public LineRenderer lr;
    public GameObject Camera;

    private Interactable currentlySelected = null;
    private int currentIndex = 0;


    // COLORS
    static Color basic = new Color(180,180,180);
    static Color highlighted = new Color(255, 255, 255);


    [Client]
    private void Start()
    {
        if (!netIdentity.isClient) return;
        InputManager.current.onScroll += onScroll;
        InputManager.current.onInteract += Interact;
    }

    [Client]
    private void Update()
    {
        if (!netIdentity.isClient) return;
        getInteraction();
        //getAllInteractions();
    }

    private void onScroll(bool tf)
    {
        // true for up false for down
        Debug.Log(tf);
        if(currentlySelected != null)
        {
            if(tf == true)
            {
                // SCROLLING UP
                currentIndex = Mathf.Min(currentIndex + 1, currentlySelected.Interactions.Length - 1);
            }
            else
            {   
                // SCROLLING DOWN
                currentIndex = Mathf.Max(currentIndex - 1, 0);
            }

            getAllInteractions();
        }
    }

    [Client]
    private void getInteraction()
    {
        Collider[] hitColliders = Physics.OverlapSphere(Camera.transform.position + (Camera.transform.forward * 0.75f), 1f);

        Interactable n_interact = null;

        foreach (var hitCollider in hitColliders)
        {
            Interactable interactble = hitCollider.transform.root.GetComponent<Interactable>();
            if(interactble != null && n_interact == null)
            {
                n_interact = interactble;
            }
        }

        if(currentlySelected != n_interact)
        {
            currentlySelected = n_interact;
            currentIndex = 0;
            getAllInteractions();
        }
    }

    [Client]
    private void getAllInteractions()
    {
        //Remove all items we currently have
        if(ItemParent.transform.childCount > 0)
        {
            for(int i = 0; i < ItemParent.transform.childCount; i++)
            {
                Destroy(ItemParent.transform.GetChild(i).gameObject);
            }
        }

        //adds items to the UI
        if (currentlySelected != null && currentlySelected.Interactions.Length > 0)
        {
            for (int i = 0; i < currentlySelected.Interactions.Length; i++)
            {
                var action = currentlySelected.Interactions[i];
                GameObject newItem = Instantiate(Item, ItemParent.transform) as GameObject;

                if (i == currentIndex)
                {
                    newItem.GetComponent<Image>().color = highlighted;
                    newItem.transform.Find("InteractionText").GetComponent<Text>().text = "> " + currentlySelected.Interactions[i].getName() + " <";
                }
                else
                {
                    newItem.GetComponent<Image>().color = basic;
                    newItem.transform.Find("InteractionText").GetComponent<Text>().text = currentlySelected.Interactions[i].getName();
                }

                //Interactions.Add(action);
                //Items.Add(newItem);
            }
        }
    }

    /*
    private void updateLine()
    {
        if(currentlySelected != null)
        {
            lr.gameObject.SetActive(true);

            Vector3 pos2 = currentlySelected.InteractionPoint.position;
            lr.SetPosition(1, new Vector3(pos2.x,pos2.y,0));
        }
        else
        {
            lr.gameObject.SetActive(false);
        }
    }
    */

    void Interact(bool b)
    {
        if (b && currentlySelected != null)
        {
            currentlySelected.Interact(currentIndex);
            getAllInteractions();
        }
    }

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(Camera.transform.position + (Camera.transform.forward * 0.75f), 1f);
    }
    */
}
