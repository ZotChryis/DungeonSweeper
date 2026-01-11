using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AchievementTabButton : MonoBehaviour
    {
        [SerializeField] private Image backgrond;
        [SerializeField] private Image check;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private Sprite activeSprite;

        public void SetAchivedState(bool achieved)
        {
            check.enabled = achieved;
        }
        
        public void SetSelected(bool active)
        {
            backgrond.sprite = active ? activeSprite : inactiveSprite;
        }
    }
}
