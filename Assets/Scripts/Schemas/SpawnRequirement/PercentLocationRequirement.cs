using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnRequirement/Percent")]
public class PercentLocationRequirement : NoCheckChildSpawnRequirement
{
    public float XPercent = 0.5f;

    public float YPercent = 0.5f;

    public int margin = 0;

    public override (int x, int y) GetRandomCoordinate(RandomBoard board)
    {
        int xMargin = Random.Range(-margin, margin + 1);
        int yMargin = Random.Range(-margin, margin + 1);
        (int retX, int retY) = ((int)(board.width * XPercent) + xMargin, (int)(board.height * YPercent) + yMargin);
        if(!board.PeekUnoccupiedSpace(retX, retY))
        {
            return board.GetNextUnoccupiedSpace(retX, retY);
        }
        return (retX, retY);
    }
}
