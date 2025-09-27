using Unity.VisualScripting;
using UnityEngine;

public class FocusBox : MonoBehaviour
{
    [SerializeField] private RectTransform LeftBg;
    [SerializeField] private RectTransform RightBg;
    [SerializeField] private RectTransform TopBg;
    [SerializeField] private RectTransform BottomBg;

    [SerializeField] private GameObject LeftArrow;
    [SerializeField] private GameObject RightArrow;

    private RectTransform myRectTransform;

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
    }

    public void SetPositionAndShow(Vector3 localPosition, RectTransform targetRectTransform, bool useRightArrow)
    {
        gameObject.SetActive(true);

        transform.localPosition = localPosition;

        Debug.Log("Setting focus with screen width: " + Screen.width + " and pos: " + localPosition);
        RightArrow.SetActive(useRightArrow);
        LeftArrow.SetActive(!useRightArrow);

        myRectTransform.sizeDelta = targetRectTransform.sizeDelta;
        TopBg.sizeDelta = new Vector2(targetRectTransform.sizeDelta.x, 5000);
        BottomBg.sizeDelta = new Vector2(targetRectTransform.sizeDelta.x, 5000);
    }
}
