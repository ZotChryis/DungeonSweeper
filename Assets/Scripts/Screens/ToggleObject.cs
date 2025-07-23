using UnityEngine;

public class ToggleObject : MonoBehaviour
{
    public int ClickCountRequired = 1;
    public GameObject target;

    private int Clicks = 0;
    
    public void TargetEnable()
    {
        target.SetActive(true);
    }

    public void TargetDisable()
    {
        target.SetActive(false);
    }

    public void ToggleTarget()
    {
        Clicks++;
        if (Clicks < ClickCountRequired)
        {
            return;
        }
        
        target.SetActive(!target.activeSelf);
    }
}
