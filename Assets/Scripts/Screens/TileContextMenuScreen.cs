using Singletons;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
    public class TileContextMenuScreen : BaseScreen
    {
        [SerializeField] private RectTransform Content;
    
        [SerializeField] private GameObject[] Level0Objects;
        [SerializeField] private GameObject[] Level1Objects;
        [SerializeField] private GameObject[] Level2Objects;
        [SerializeField] private GameObject[] Level3Objects;

        private Tile ActiveTile;

        protected override void Awake()
        {
            base.Awake();

            ServiceLocator.Instance.Register(this);

            var buttons = GetComponentsInChildren<Button>(true).Where(b => b.gameObject.name.Contains("Option")).ToArray();
            for (int i = 0; i < buttons.Length; i++)
            {
                string buttonNumber = buttons[i].gameObject.name.Substring(7);
                int.TryParse(buttonNumber, out int number);
                buttons[i].onClick.AddListener(() =>
                {
                    OnOptionSelected(number);
                });
            }

            ServiceLocator.Instance.LevelManager.OnLevelChanged += OnLevelChanged;
        }

        private void OnDestroy()
        {
            ServiceLocator.Instance.LevelManager.OnLevelChanged -= OnLevelChanged;
        }
    
        private void OnLevelChanged(int level)
        {
            foreach (var level0Object in Level0Objects)
            {
                level0Object.SetActive(level >= 0);
            }
        
            foreach (var level1Object in Level1Objects)
            {
                level1Object.SetActive(level >= 1);
            }
            
            foreach (var level2Object in Level2Objects)
            {
                level2Object.SetActive(level >= 2);
            }
            
            foreach (var level3Object in Level3Objects)
            {
                level3Object.SetActive(level >= 3);
            }
        }

        protected override void OnCloseButtonClicked()
        {
            ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
            ActiveTile = null;
        }

        public void OnOptionSelected(int option)
        {
            ActiveTile.SetAnnotation(option);
            ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
            ActiveTile = null;
        }

        public void SetActiveTile(Tile tile)
        {
            ActiveTile = tile;
            Content.position = ((RectTransform)(tile.transform)).position;
            ClampToWindow();
        }

        protected override void OnShow()
        {
            base.OnShow();
            AudioManager.Instance.PlaySfx("ClickNeutral");
        }

        protected override void OnHide()
        {
            base.OnHide();
            AudioManager.Instance.PlaySfx("ClickNeutral");
        }
        
        // Clamp panel to area of parent
        void ClampToWindow()
        {
            var parentRectTransform = Content.parent.GetComponent<RectTransform>();
            Vector3 pos = Content.localPosition;

            Vector3 minPosition = parentRectTransform.rect.min - Content.rect.min;
            Vector3 maxPosition = parentRectTransform.rect.max - Content.rect.max;

            pos.x = Mathf.Clamp(Content.localPosition.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(Content.localPosition.y, minPosition.y, maxPosition.y);

            Content.localPosition = pos;
        }
    }
}
