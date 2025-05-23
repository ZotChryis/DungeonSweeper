using System;
using Gameplay;
using Schemas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ClassSelectionItem : MonoBehaviour
{
    [SerializeField] private Class.Id Class;
    [SerializeField] private Image Icon;
    [SerializeField] private TMP_Text Name;
    [SerializeField] private TMP_Text Description;

    private void Awake()
    {
        ClassSchema Schema = ServiceLocator.Instance.Schemas.ClassSchemas.Find(c => c.Id == Class);
        if (Schema == null)
        {
            return;
        }

        Icon.sprite = Schema.Sprite;
        Name.SetText(Schema.Name);
        Description.SetText(Schema.Description);
    }
}