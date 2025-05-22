using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectionScreen : Screen
{
    [SerializeField] 
    private LibraryItem LibraryItemPrefab;
}

[Serializable]
public class ClassSelectionEntry
{
    [SerializeField] private Player.Class Class;
    [SerializeField] private Image Icon;
    [SerializeField] private TMP_Text Name;
    [SerializeField] private TMP_Text Description;
}