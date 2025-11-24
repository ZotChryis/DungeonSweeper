using Gameplay;
using Schemas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Achievements
{
    public class AchievementItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text Title;
        [SerializeField] private TMP_Text Description;
        [SerializeField] private TMP_Text Reward;
        [SerializeField] private TMP_Text Date;
        [SerializeField] private GameObject Check;
        [SerializeField] private Image AchievementIcon;
        [SerializeField] private Material GrayscaleMaterial;

        public void SetSchema(AchievementSchema schema)
        {
            Title.SetText(schema.Title);
            Description.SetText(schema.Description);
            Reward.SetText(schema.Reward);
            AchievementIcon.sprite = schema.AchievementIcon;

            bool achieved = schema.AchievementId.IsAchieved();
            if (achieved)
            {
                AchievementIcon.material = null;
            }
            else
            {
                AchievementIcon.material = GrayscaleMaterial;
            }
            string dateAchieved = FBPP.GetString("Achievement" + schema.AchievementId, "");

            Check.SetActive(achieved);

            Date.gameObject.SetActive(achieved);
            Date.SetText("Achieved: " + dateAchieved);
        }
    }
}
