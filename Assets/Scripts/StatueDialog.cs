using TMPro;
using UnityEngine;
using System.Collections.Generic;


public class StatueDialog : MonoBehaviour
{
    public GameObject dialogBox;
    public TextMeshProUGUI DialogText;
    // public string dialog;
    public bool playerInRange;

    public List<string> possibleDialogs = new List<string>
    {
        "The plaque has worn away, but the farmers say this is Saint Elara, who calmed the earth's fury.",
        "The hero depicted here was once a guardian of the 'Seven Keys.' Only six remain accounted for.",
        "When the Veil of Sleep lifts, the stone will weep, and the forgotten gate shall answer.",
        "The tears of stone mark the beginning of the end. Do not disturb what lies below.",
        "Sir Reginald the Mediocre ^-^ He never beat the first dungeon but was very good at tidying up.",
        "Oh, look. Another destined hero. Don't trip over the grass on your way to saving the world, kid.",
        "The stone shall weep when a hero wears too much green. Avoid wearing a hat.",
        "Only those with a flawless dental record may pass. You look like you chew rocks, so move on."
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
