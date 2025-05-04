using TMPro;
using UnityEngine;

public class PlayerShopXpLabel : MonoBehaviour
{
    private TextMeshProUGUI m_textLabel;

    private void Start()
    {
        m_textLabel = GetComponent<TextMeshProUGUI>();
        ServiceLocator.Instance.Player.OnShopXpChanged += UpdateShopXpLabel;
        UpdateShopXpLabel();
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.Player.OnShopXpChanged -= UpdateShopXpLabel;
    }

    private void UpdateShopXpLabel()
    {
        m_textLabel.text = ServiceLocator.Instance.Player.ShopXp.ToString() + "$";
    }
}
