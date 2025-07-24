using UI;
using UnityEngine;

public class CheatToggler : MonoBehaviour
{
    public DSButton Button;
    public ToggleObject Toggle;

    private void Start()
    {
        Button.OnConfirmed += OnConfirmed;
    }

    private void OnConfirmed()
    {
        Toggle.ToggleTarget();
    }
}
