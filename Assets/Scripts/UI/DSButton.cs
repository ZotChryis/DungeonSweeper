using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    // DS -> DungeonSweeper
    // Extended button class to wrap our own logic. Needed this for the animated text stuff
    // We must wrap and not extend because Unity has its own special Inspector drawer which I don't want to mess with
    public class DSButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button Button;
        [SerializeField] private bool AnimateTextOnHover;

        private TextAnimation[] TextAnimations;

        private void Start()
        {
            TextAnimations = GetComponentsInChildren<TextAnimation>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!AnimateTextOnHover)
            {
                return;
            }
            
            foreach (var anim in TextAnimations)
            {
                anim.Enabled = true;
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!AnimateTextOnHover)
            {
                return;
            }
            
            foreach (var anim in TextAnimations)
            {
                anim.Enabled = false;
                anim.Reset();
            }
        }
        
    }
}