using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnRequirement/Percent")]
public class PercentLocationRequirement : NoCheckChildSpawnRequirement
{
    public float XPercent = 0.5f;

    public float YPercent = 0.5f;

    public override (int x, int y) GetRandomCoordinate(RandomBoard board)
    {
        return ((int)(board.width * XPercent), (int)(board.height * YPercent));
    }
}
