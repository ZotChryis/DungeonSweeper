using System;
using Schemas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Challenges
{
    public class ChallengeItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject checkbox;
        [SerializeField] private Button button;

        private ChallengeSchema _schema;
        private Action _onButtonClickedCallback;

        public void SetData(ChallengeSchema schema, Action callback)
        {
            _schema = schema;
            _onButtonClickedCallback = callback;

            label.SetText(_schema.Title);
            checkbox.SetActive(ServiceLocator.Instance.ChallengeSystem.IsCompleted(schema));
        }
        
        private void Start()
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        
        private void OnButtonClicked()
        {
            _onButtonClickedCallback?.Invoke();
        }
    }
}