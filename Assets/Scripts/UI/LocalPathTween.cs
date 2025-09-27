using DG.Tweening;
using UnityEngine;

public class LocalPathTween : MonoBehaviour
{
    [Tooltip("Local path relative to localPosition")]
    public Vector3[] path;
    public int loopCount = 1;
    public float duration = 1f;
    public PathType pathType = PathType.Linear;

    private void Start()
    {
        Vector3 startingPosition = transform.localPosition;

        Vector3[] localPath = new Vector3[path.Length];

        for (int i = 0; i < path.Length; ++i)
        {
            localPath[i] = path[i] + startingPosition;
        }

        transform.DOLocalPath(localPath, duration, pathType).SetLoops(loopCount);
    }
}
