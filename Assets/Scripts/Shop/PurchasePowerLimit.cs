using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

/// <summary>
/// As we purchase powers we increment our stock down.
/// </summary>
public class PurchasePowerLimit : MonoBehaviour
{
    [Tooltip("Max number of times you can purchase this.")]
    public int purchaseLimit = 0;

    [Tooltip("Number of times the player has already purchased this.")]
    public int currentPurchaseAmount = 0;

    [Tooltip("Amount of shop XP this power costs.")]
    public int costToPurchasePower = 1;

    [Tooltip("Text label to update as we run out of stock.")]
    public TextMeshProUGUI purchaseLimitText;

    [Tooltip("Text label displaying cost in shop XP.")]
    public TextMeshProUGUI costText;

    [SerializeField]
    private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

    private Button m_Button;

    private void Start()
    {
        m_Button = GetComponent<Button>();
        costText.text = costToPurchasePower.ToString() + "$";

        if (purchaseLimit <= 0)
        {
            // disable yourself...
            purchaseLimitText.enabled = false;
        }
        else
        {
            m_Button.onClick.AddListener(OnPurchasePower);
        }
        UpdatePurchaseLimitText();
    }

    private void OnPurchasePower()
    {
        // check if we can afford.
        if (ServiceLocator.Instance.Player.ShopXp >= costToPurchasePower)
        {
            ServiceLocator.Instance.Player.ShopXp -= costToPurchasePower;
            currentPurchaseAmount++;
            UpdatePurchaseLimitText();

            m_OnClick.Invoke();

            if (currentPurchaseAmount >= purchaseLimit)
            {
                // we are out of stock
                OnOutOfStock();
            }
        }
        else
        {
            // play the not enough money FX
        }
    }

    private void UpdatePurchaseLimitText()
    {
        if (purchaseLimitText && purchaseLimitText.enabled)
        {
            purchaseLimitText.text = (purchaseLimit - currentPurchaseAmount).ToString() + "/" + purchaseLimit;
        }
    }

    private void OnOutOfStock()
    {
        m_Button.interactable = false;
    }
}
