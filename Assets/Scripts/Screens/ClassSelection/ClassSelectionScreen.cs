using System;
using Schemas;

namespace Screens.ClassSelection
{
    public class ClassSelectionScreen : BaseScreen
    {
        protected void Start()
        {
            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted += RefreshItems;
            RefreshItems(null);
        }

        private void OnDestroy()
        {
            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted -= RefreshItems;
        }

        private void RefreshItems(AchievementSchema _)
        {
            var unlockedClasses = ServiceLocator.Instance.AchievementSystem.GetUnlockedClasses();
            var items = GetComponentsInChildren<ClassSelectionItem>(includeInactive: true);
            foreach (var classSelectionItem in items)
            {
                classSelectionItem.SetLocked(!unlockedClasses.Contains(classSelectionItem.GetClassId()));
            }
        }
    }
}