using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Screen : MonoBehaviour
{
    [SerializeField]
    private GameObject Container;

    [SerializeField]
    private Button CloseButton;

    private void Awake()
    {
        CloseButton?.onClick.AddListener(OnCloseButtonClicked);
    }

    private void OnCloseButtonClicked()
    {
        SceneManager.LoadScene("Game");
    }

    // TODO: Add animations and shit
    public void Show()
    {
        Container.SetActive(true);
    }

    public void Hide()
    {
        Container.SetActive(false);
    }
}
