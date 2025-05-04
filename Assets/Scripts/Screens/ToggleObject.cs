using UnityEngine;

public class ToggleObject : MonoBehaviour
{
    public GameObject target;

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
        target.SetActive(!target.activeSelf);
    }
}
