using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIItem : MonoBehaviour
{
    [SerializeField] 
    private GameObject Container;
    
    [SerializeField]
    private Image Half;
    
    [SerializeField]
    private Image Shield;

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
        Shield.enabled = !isHalf;
        Label.enabled = !isHalf;
    }
    
    public void SetFull(bool full)
    {
        Full.enabled = full;
        Empty.enabled = !full;
    }

    public void SetShield(bool shield)
    {
        Shield.enabled = shield;
    }

    public void SetLabelText(string text)
    {
        Label.enabled = true;
        Label.SetText(text);
    }

    public void Animate(float duration, float delay)
    {
        Container.transform.DOScale(Vector3.one, duration).From(Vector3.zero).SetDelay(delay);
    }
}
