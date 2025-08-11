using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO: Make a new fucking name
public class PlayerUIItem : MonoBehaviour
{
    [SerializeField]
    private Image Half;
    
    [SerializeField]
    private Image GhostFull;

    [SerializeField]
    private Image Full;

    [SerializeField]
    private Image Empty;

    [SerializeField]
    private TMP_Text Label;

    public void SetHalf(bool isHalf)
    {
        Half.enabled = isHalf;
        
        Full.enabled = !isHalf;
        Empty.enabled = !isHalf;
        GhostFull.enabled = !isHalf;
        Label.enabled = !isHalf;
    }
    
    public void SetFull(bool full)
    {
        Full.enabled = full;
        Empty.enabled = !full;
    }

    public void SetGhostFull(bool ghostFull)
    {
        GhostFull.enabled = ghostFull;
    }

    public void SetLabelText(string text)
    {
        Label.enabled = true;
        Label.SetText(text);
    }
}
