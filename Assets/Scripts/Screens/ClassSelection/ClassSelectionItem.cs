using System;
using Gameplay;
using Schemas;
using Screens.ClassSelection;
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
    [SerializeField] private TMP_Text BlockedText;
    [SerializeField] private Button Button;

    private bool _locked = false;
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
        LockedIcon.gameObject.SetActive(locked);
        UnlockText.gameObject.SetActive(locked);

        _locked = locked;
    }

    public void SetBlocked(bool blocked)
    {
        // Locked supercedes blocked
        if (_locked)
        {
            BlockedText.gameObject.SetActive(false);
            return;
        }
        
        Button.interactable = !blocked;
        LockedIcon.gameObject.SetActive(blocked);
        BlockedText.gameObject.SetActive(blocked);
    }

    private void OnButtonClicked()
    {
        TransitionManager.Instance.DoTransition(TransitionManager.TransitionType.Goop, SetClassAndStartGame);
        ServiceLocator.Instance.AudioManager.PlaySfx("ClickGood");
    }

    private void SetClassAndStartGame()
    {
        // TODO: Cleanup this flow, but it works for now :shrug:
        // Commit to the selected challenge if needed
        ServiceLocator.Instance.ChallengeSystem.Commit();
        if (ServiceLocator.Instance.ChallengeSystem.CurrentChallenge != null)
        {
            ServiceLocator.Instance.LevelManager.StartingLevel = 0;
        }
        
        ServiceLocator.Instance.Player.ShopXp = StartingLevelStartingXp[ServiceLocator.Instance.LevelManager.StartingLevel];
        ServiceLocator.Instance.SaveSystem.WipeRun();
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = true;
        ServiceLocator.Instance.Player.TEMP_SetClass(Class);
        ServiceLocator.Instance.LevelManager.SetToStartingLevel();
        ServiceLocator.Instance.Grid.GenerateGrid();
        ServiceLocator.Instance.Player.ChangeBountyTarget();
        ServiceLocator.Instance.Player.ChangeMenuTarget();
        
        // When a class is selected, we will close the class selection screen AND the main menu screen
        // TODO: Do proper scene mangagement
        ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();
    }
}