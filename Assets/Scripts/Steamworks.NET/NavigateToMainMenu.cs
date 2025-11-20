using UnityEngine;

public class NavigateToMainMenu : MonoBehaviour
{
    public void ToMainMenu()
    {
        TransitionManager.Instance.DoTransition(TransitionManager.TransitionType.Goop, CheatManager.Instance.Restart);
    }
}
