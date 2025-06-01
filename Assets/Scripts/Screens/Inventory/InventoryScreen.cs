using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScreen : BaseScreen
{
    [SerializeField] 
    private InventoryItem ItemPrefab;

    [SerializeField] 
    private Transform ItemListRoot;
}

[Serializable]
public class InventoryItem
{
    [SerializeField] private Image Icon;
    [SerializeField] private TMP_Text Name;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private TMP_Text Quantity; // If consumable
}