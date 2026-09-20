using TMPro;
using UnityEngine;
using System.Collections.Generic;


public class HouseDialog : MonoBehaviour
{
    public GameObject dialogBox;
    public TextMeshProUGUI DialogText;
    // public string dialog;
    public bool playerInRange;

    public List<string> possibleDialogs = new List<string>
    {
        "The trader is always late opening the stall lately. He spends too much time staring at that creepy old statue.",
        "Last night, I swear I heard the earth groaning. It sounded like it came from that old cave entrance.",
        "Don't stray too far past the fence line. The wild boars have been very aggressive this season.",
        "Did you know this path used to be a bustling highway? Now, only a few travelers dare to use it.",
        "This is private property! Are you blind, or just naturally dense? Get out of my sight.",
        "Another tourist in a leather vest. You lot are all the same. Go bother the bats in the cave.",
        "Move! You're blocking the sun and my pet moss needs its vitamin D. Some hero you are.",
        "I was about to take a nap! Who do you think you are, waking up the entire neighborhood with your heavy footfalls?"
    };

    private string currentDialog;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && playerInRange)
        {
            if (dialogBox.activeInHierarchy)
            {
                dialogBox.SetActive(false);
            }
            else
            {
                dialogBox.SetActive(true);
                DialogText.text = currentDialog;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            PickRandomDialog();
            dialogBox.SetActive(true);
            DialogText.text = currentDialog;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (dialogBox != null)
                dialogBox.SetActive(false);
        }
    }

    void PickRandomDialog()
    {
        if (possibleDialogs.Count > 0)
        {
            int index = Random.Range(0, possibleDialogs.Count);
            currentDialog = possibleDialogs[index];
        }
        else
        {
            currentDialog = "Hello there.";
        }
    }
}
