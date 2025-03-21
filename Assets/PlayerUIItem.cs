using UnityEngine;
using UnityEngine.UI;

// TODO: Make a new fucking name
public class PlayerUIItem : MonoBehaviour
{
    [SerializeField]
    private Image Full;

    [SerializeField]
    private Image Empty;

    public void SetFull(bool full)
    {
        Full.enabled = full;
        Empty.enabled = !full;
    }
}
