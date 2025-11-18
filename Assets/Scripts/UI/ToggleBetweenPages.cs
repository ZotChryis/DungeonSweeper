using UnityEngine;

public class ToggleBetweenPages : MonoBehaviour
{
    public GameObject[] Page1;
    public GameObject[] Page2;

    public void ShowPage1()
    {
        foreach (var page in Page1)
        {
            page.SetActive(true);
        }
        foreach (var page in Page2)
        {
            page.SetActive(false);
        }
    }

    public void ShowPage2()
    {
        foreach (var page in Page1)
        {
            page.SetActive(false);
        }
        foreach (var page in Page2)
        {
            page.SetActive(true);
        }
    }
}