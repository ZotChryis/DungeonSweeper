using Gameplay;
using Schemas;
using Singletons;
using UnityEngine;

public class BountyBoardHudRerollButton : MonoBehaviour
{
    [SerializeField] private string ConfirmationTitle;
    [SerializeField] private string ConfirmationMessage;

    private ItemInstance bountyBoard = null;

    private ItemInstance GetBountyBoard()
    {
        if (bountyBoard != null)
        {
            return bountyBoard;
        }
        bountyBoard = ServiceLocator.Instance.Player.Inventory.GetFirstItem(ItemSchema.Id.BountyBoard);
        return bountyBoard;
    }

    public void AttemptToActivateBountyBoard()
    {
        if (GetBountyBoard().CanBeUsed())
        {
            ServiceLocator.Instance.OverlayScreenManager.RequestConfirmationScreen(() =>
            {
                ActivateBountyBoardItem();
            },
                ConfirmationTitle,
                string.Format(ConfirmationMessage, GetBountyBoard().CurrentCharges)
            );
            AudioManager.Instance.PlaySfx("ClickGood");
        }
        else
        {
            AudioManager.Instance.PlaySfx("Error");
        }
    }

    public void ActivateBountyBoardItem()
    {
        ServiceLocator.Instance.Player.Inventory.UseItem(GetBountyBoard());
    }
}
