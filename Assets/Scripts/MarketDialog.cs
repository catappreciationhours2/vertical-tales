using TMPro;
using UnityEngine;
using System.Collections.Generic;


public class MarketDialog : MonoBehaviour
{
    public GameObject dialogBox;
    public TextMeshProUGUI DialogText;
    // public string dialog;
    public bool playerInRange;

    public List<string> possibleDialogs = new List<string>
    {
        "The price of Lumiberries has tripled! The best picking spots are now too close to the Black Forest boundary.",
        "My delivery of Iron Ore is late. The guards stopped all trade routes near the Capital. Something big is happening.",
        "Try these Moon Apples. They say they only grow where the 'Ancient Ones' once walked.",
        "I need to find a new source of granite. The stone from the old quarry near the ruins is getting too fragile to carve.",
        "You look like you found your sword in a ditch. Everything here costs more than your entire life savings, beat it.",
        "No, I don't take swamp currency. Come back when you've accomplished something other than dirtying up my storefront.",
        "This is a Golden Potato—only 800 gold. It was grown in soil blessed by three goddesses. You wouldn't appreciate it.",
        "Everything is cash only! I don't accept 'favor credits' or trade for 'future dungeon loot.' Now buy a banana, or leave."
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
