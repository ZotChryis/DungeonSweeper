using System;
using Gameplay;
using Schemas;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ClassSelectionItem : MonoBehaviour
{
    [SerializeField] private Class.Id Class;
    [SerializeField] private Image Icon;
    [SerializeField] private Image LockedIcon;
    [SerializeField] private TMP_Text Name;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private Button Button;
    
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

        Button.onClick.AddListener(OnButtonClicked);
    }

    public Class.Id GetClassId()
    {
        return Class;
    }
    
    public void SetLocked(bool locked)
    {
        Button.interactable = !locked;
        LockedIcon.GameObject().SetActive(locked);
    }

    private void OnButtonClicked()
    {
        ServiceLocator.Instance.Player.TEMP_SetClass(Class);
        ServiceLocator.Instance.LevelManager.SetLevel(0);
        
        // When a class is selected, we will close the class selection screen AND the main menu screen
        // TODO: Do proper scene mangagement
        ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();
    }
}