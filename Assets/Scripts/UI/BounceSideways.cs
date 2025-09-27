using DG.Tweening;
using UnityEngine;

public class BounceSideways : MonoBehaviour
{
    private void Start()
    {
        Vector3 startingPosition = transform.localPosition;
        Vector3[] path = new Vector3[2] { startingPosition + new Vector3(-10, 0), startingPosition + new Vector3(10, 0, 0) };
        transform.DOLocalPath(path, 0.8f, PathType.Linear).SetLoops(-1);
    }
}
