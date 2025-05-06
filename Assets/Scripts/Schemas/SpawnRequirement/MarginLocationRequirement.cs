using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnRequirement/MarginLocation")]
public class MarginLocationRequirement : SpawnRequirement
{
    public int leftXMargin;
    public int rightXMargin;

    public int topYMargin;
    public int bottomYMargin;

    // Margin doesn't support consecutive spawns
    public override List<(int x, int y)> GetRandomConsecutiveNeighborLocations(RandomBoard board, int inputX, int inputY)
    {
        return new List<(int x, int y)>(0);
    }

    public override (int x, int y) GetRandomCoordinate(RandomBoard board)
    {
        int xReturnValue = Random.Range(0 + leftXMargin, board.width - rightXMargin);
        int yReturnValue = Random.Range(0 + bottomYMargin, board.height - topYMargin);

        if (xReturnValue < 0 || xReturnValue >= board.width)
        {
            xReturnValue = (int)(board.width / 2f);
        }
        if (yReturnValue < 0 || yReturnValue >= board.height)
        {
            yReturnValue = (int)(board.height / 2f);
        }

        if(!board.PeekUnoccupiedSpace(xReturnValue, yReturnValue))
        {
            return board.GetNextUnoccupiedSpaceWithMargin(leftXMargin, rightXMargin, topYMargin, bottomYMargin, xReturnValue, yReturnValue);
        }
        return (xReturnValue, yReturnValue);
    }
}
