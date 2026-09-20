using TMPro;
using UnityEngine;
using System.Collections.Generic;


public class CaveDialog : MonoBehaviour
{
    public GameObject dialogBox;
    public TextMeshProUGUI DialogText;
    // public string dialog;
    public bool playerInRange;

    public List<string> possibleDialogs = new List<string>
    {
        "The gate does not yield to force, but to the silent offering of a lunar fruit.",
        "The twin locks of the Earth-Gate require a metal forged under a storm.",
        "The earth is breathing. Deep inside, the Ancient Watcher stirs, annoyed by the light.",
        "The air tastes of ozone. There are rumors of glowing, unstable slimes that have made this place their home.",
        "This crypt is reserved for heroes who can handle more than three slimes. You should go home.",
        "Warning: Beyond this point lies doom, despair, and a level of peril far exceeding your current skill set. Turn back, weakling.",
        "The door is stuck! It only opens for those carrying a perfectly ripe plum. No substitutes accepted.",
        "The puzzle requires the mind of a tactical genius and the patience of a saint. Given your track record, this is where your journey ends."
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
