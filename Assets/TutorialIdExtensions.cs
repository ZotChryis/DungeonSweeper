using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TutorialManager;

public static class TutorialIdExtensions
{
    /// <summary>
    /// Gets the string used as a key saved to FBPP settings.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetTutorialKey(this TutorialId id)
    {
        return "Tutorial" + id.ToString();
    }
}
