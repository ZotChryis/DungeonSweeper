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
    [SerializeField] private TMP_Text UnlockText;
    [SerializeField] private Button Button;

    private readonly int[] StartingLevelStartingXp = new int[] { 0, 10, 25, 45, 55, 55 };

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
        UnlockText.SetText(Schema.GetUnlockText());

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
        UnlockText.gameObject.SetActive(locked);
    }

    private void OnButtonClicked()
    {
        TransitionManager.Instance.DoTransition(TransitionManager.TransitionType.Goop, SetClassAndStartGame);
        ServiceLocator.Instance.AudioManager.PlaySfx("ClickGood");
    }

    private void SetClassAndStartGame()
    {
        ServiceLocator.Instance.Player.ShopXp = StartingLevelStartingXp[ServiceLocator.Instance.LevelManager.StartingLevel];
        ServiceLocator.Instance.SaveSystem.Wipe();
        
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = true;
        ServiceLocator.Instance.Player.TEMP_SetClass(Class);
        ServiceLocator.Instance.LevelManager.SetToStartingLevel();
        ServiceLocator.Instance.Grid.GenerateGrid();
        ServiceLocator.Instance.Player.ChangeBountyTarget();
        
        // When a class is selected, we will close the class selection screen AND the main menu screen
        // TODO: Do proper scene mangagement
        ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();
    }
}