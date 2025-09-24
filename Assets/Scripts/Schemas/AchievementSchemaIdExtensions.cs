using Schemas;

public static class AchievementSchemaIdExtensions
{
    public static bool IsAchieved(this AchievementSchema.Id id)
    {
        return FBPP.GetBool("Achievement" + id, false);
    }
}
