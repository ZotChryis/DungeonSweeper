public abstract class SpawnRequirement : Schema
{
    public abstract bool IsValid(int xCoord, int yCoord);
}