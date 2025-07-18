using System;
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
            if (level >= 0)
            {
                foreach (var level0Object in Level0Objects)
                {
                    level0Object.SetActive(true);
                }
            }
        
            if (level >= 1)
            {
                foreach (var level1Object in Level1Objects)
                {
                    level1Object.SetActive(true);
                }
            }
        
            if (level >= 2)
            {
                foreach (var level2Object in Level2Objects)
                {
                    level2Object.SetActive(true);
                }
            }
        
            if (level >= 3)
            {
                foreach (var level3Object in Level3Objects)
                {
                    level3Object.SetActive(true);
                }
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
            Content.position = new Vector3(Content.position.x, Mathf.Clamp(Content.position.y, 360, 910), Content.position.z);
        }
    }
}
