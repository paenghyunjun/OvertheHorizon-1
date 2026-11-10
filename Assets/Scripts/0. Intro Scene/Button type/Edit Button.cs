using UnityEngine;
using UnityEngine.EventSystems;

public class EditButton : UIStateFeedback
{
    [SerializeField]private GameObject editPanel;
    bool editButtonClicked = false;
    public void OnClickEdit()
    {
        if (!editButtonClicked)
        {
            editButtonClicked = true;
            editPanel.SetActive(true);
        }
        else
        {
            editButtonClicked = false;
            editPanel.SetActive(false);
        }
    }
}