using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO: Make a new fucking name
public class PlayerUIItem : MonoBehaviour
{
    public Image GhostFull;

    [SerializeField]
    private Image Full;

    [SerializeField]
    private Image Empty;

    [SerializeField]
    private TMP_Text Label;

    public void SetFull(bool full)
    {
        Full.enabled = full;
        Empty.enabled = !full;
    }

    public void SetGhostFull(bool ghostFull)
    {
        if (GhostFull)
        {
            GhostFull.enabled = ghostFull;
        }
    }

    public void SetLabelText(string text)
    {
        Label.SetText(text);
    }
}
