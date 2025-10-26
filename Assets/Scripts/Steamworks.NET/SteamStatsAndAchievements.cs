#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
using Schemas;
using System.Linq;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

// This is a port of StatsAndAchievements.cpp from SpaceWar, the official Steamworks Example.
public class SteamStatsAndAchievements : MonoBehaviour
{
#if !DISABLESTEAMWORKS

    private Achievement_t[] m_Achievements = new Achievement_t[] {
        new Achievement_t(AchievementSchema.Id.Adventurer0.ToString(), "Novice Adventurer", "Complete the first dungeon level with the Adventurer."),
        new Achievement_t(AchievementSchema.Id.Adventurer1.ToString(), "Intermediate Adventurer", "Complete the second dungeon level with the Adventurer."),
        new Achievement_t(AchievementSchema.Id.Adventurer2.ToString(), "Advanced Adventurer", "Complete the third dungeon level with the Adventurer."),
        new Achievement_t(AchievementSchema.Id.Adventurer3.ToString(), "Master Adventurer", "Complete the fourth dungeon level with the Adventurer."),
        new Achievement_t(AchievementSchema.Id.Adventurer4.ToString(), "Ascendant Adventurer", "Complete the fifth dungeon level with the Adventurer."),

        new Achievement_t(AchievementSchema.Id.Warrior0.ToString(), "Novice Warrior", "Complete the first dungeon level with the Warrior."),
        new Achievement_t(AchievementSchema.Id.Warrior1.ToString(), "Intermediate Warrior", "Complete the second dungeon level with the Warrior."),
        new Achievement_t(AchievementSchema.Id.Warrior2.ToString(), "Advanced Warrior", "Complete the third dungeon level with the Warrior."),
        new Achievement_t(AchievementSchema.Id.Warrior3.ToString(), "Master Warrior", "Complete the fourth dungeon level with the Warrior."),
        new Achievement_t(AchievementSchema.Id.Warrior4.ToString(), "Ascendant Warrior", "Complete the fifth dungeon level with the Warrior."),

        new Achievement_t(AchievementSchema.Id.Ranger0.ToString(), "Novice Ranger", "Complete the first dungeon level with the Ranger."),
        new Achievement_t(AchievementSchema.Id.Ranger1.ToString(), "Intermediate Ranger", "Complete the second dungeon level with the Ranger."),
        new Achievement_t(AchievementSchema.Id.Ranger2.ToString(), "Advanced Ranger", "Complete the third dungeon level with the Ranger."),
        new Achievement_t(AchievementSchema.Id.Ranger3.ToString(), "Master Ranger", "Complete the fourth dungeon level with the Ranger."),
        new Achievement_t(AchievementSchema.Id.Ranger4.ToString(), "Ascendant Ranger", "Complete the fifth dungeon level with the Ranger."),

        new Achievement_t(AchievementSchema.Id.Wizard0.ToString(), "Novice Wizard", "Complete the first dungeon level with the Wizard."),
        new Achievement_t(AchievementSchema.Id.Wizard1.ToString(), "Intermediate Wizard", "Complete the second dungeon level with the Wizard."),
        new Achievement_t(AchievementSchema.Id.Wizard2.ToString(), "Advanced Wizard", "Complete the third dungeon level with the Wizard."),
        new Achievement_t(AchievementSchema.Id.Wizard3.ToString(), "Master Wizard", "Complete the fourth dungeon level with the Wizard."),
        new Achievement_t(AchievementSchema.Id.Wizard4.ToString(), "Ascendant Wizard", "Complete the fifth dungeon level with the Wizard."),

        new Achievement_t(AchievementSchema.Id.Bard0.ToString(), "Novice Bard", "Complete the first dungeon level with the Bard."),
        new Achievement_t(AchievementSchema.Id.Bard1.ToString(), "Intermediate Bard", "Complete the second dungeon level with the Bard."),
        new Achievement_t(AchievementSchema.Id.Bard2.ToString(), "Advanced Bard", "Complete the third dungeon level with the Bard."),
        new Achievement_t(AchievementSchema.Id.Bard3.ToString(), "Master Bard", "Complete the fourth dungeon level with the Bard."),
        new Achievement_t(AchievementSchema.Id.Bard4.ToString(), "Ascendant Bard", "Complete the fifth dungeon level with the Bard."),

        new Achievement_t(AchievementSchema.Id.FortuneTeller0.ToString(), "Novice Fortune Teller", "Complete the first dungeon level with the Fortune Teller."),
        new Achievement_t(AchievementSchema.Id.FortuneTeller1.ToString(), "Intermediate Fortune Teller", "Complete the second dungeon level with the Fortune Teller."),
        new Achievement_t(AchievementSchema.Id.FortuneTeller2.ToString(), "Advanced Fortune Teller", "Complete the third dungeon level with the Fortune Teller."),
        new Achievement_t(AchievementSchema.Id.FortuneTeller3.ToString(), "Master Fortune Teller", "Complete the fourth dungeon level with the Fortune Teller."),
        new Achievement_t(AchievementSchema.Id.FortuneTeller4.ToString(), "Ascendant Fortune Teller", "Complete the fifth dungeon level with the Fortune Teller."),

        new Achievement_t(AchievementSchema.Id.Miner0.ToString(), "Novice Miner", "Complete the first dungeon level with the Miner."),
        new Achievement_t(AchievementSchema.Id.Miner1.ToString(), "Intermediate Miner", "Complete the second dungeon level with the Miner."),
        new Achievement_t(AchievementSchema.Id.Miner2.ToString(), "Advanced Miner", "Complete the third dungeon level with the Miner."),
        new Achievement_t(AchievementSchema.Id.Miner3.ToString(), "Master Miner", "Complete the fourth dungeon level with the Miner."),
        new Achievement_t(AchievementSchema.Id.Miner4.ToString(), "Ascendant Miner", "Complete the fifth dungeon level with the Miner."),

        new Achievement_t(AchievementSchema.Id.Ritualist0.ToString(), "Novice Ritualist", "Complete the first dungeon level with the Ritualist."),
        new Achievement_t(AchievementSchema.Id.Ritualist1.ToString(), "Intermediate Ritualist", "Complete the second dungeon level with the Ritualist."),
        new Achievement_t(AchievementSchema.Id.Ritualist2.ToString(), "Advanced Ritualist", "Complete the third dungeon level with the Ritualist."),
        new Achievement_t(AchievementSchema.Id.Ritualist3.ToString(), "Master Ritualist", "Complete the fourth dungeon level with the Ritualist."),
        new Achievement_t(AchievementSchema.Id.Ritualist4.ToString(), "Ascendant Ritualist", "Complete the fifth dungeon level with the Ritualist."),

        new Achievement_t(AchievementSchema.Id.PacifistRat0.ToString(), "Novice Rat Pacifist", "Complete the first dungeon without defeating a single rat."),
        new Achievement_t(AchievementSchema.Id.PacifistRat1.ToString(), "Intermediate Rat Pacifist", "Complete the second dungeon without defeating a single rat."),
        new Achievement_t(AchievementSchema.Id.PacifistRat2.ToString(), "Advanced Rat Pacifist", "Complete the third dungeon without defeating a single rat."),
        new Achievement_t(AchievementSchema.Id.PacifistRat3.ToString(), "Master Rat Pacifist", "Complete the fourth dungeon without defeating a single rat."),
        new Achievement_t(AchievementSchema.Id.PacifistRat4.ToString(), "Ascendant Rat Pacifist", "Complete the fifth dungeon without defeating a single rat."),

        new Achievement_t(AchievementSchema.Id.Annihilation0.ToString(), "Novice Annihilation", "Complete the first dungeon level with an empty board."),
        new Achievement_t(AchievementSchema.Id.Annihilation1.ToString(), "Intermediate Annihilation", "Complete the second dungeon level with an empty board."),
        new Achievement_t(AchievementSchema.Id.Annihilation2.ToString(), "Advanced Annihilation", "Complete the third dungeon level with an empty board."),
        new Achievement_t(AchievementSchema.Id.Annihilation3.ToString(), "Master Annihilation", "Complete the fourth dungeon level with an empty board."),
        new Achievement_t(AchievementSchema.Id.Annihilation4.ToString(), "Ascendant Annihilation", "Complete the fifth dungeon level with an empty board."),

        new Achievement_t(AchievementSchema.Id.Priest0.ToString(), "Novice Priest", "Complete the first dungeon level with the Priest."),
        new Achievement_t(AchievementSchema.Id.Priest1.ToString(), "Intermediate Priest", "Complete the second dungeon level with the Priest."),
        new Achievement_t(AchievementSchema.Id.Priest2.ToString(), "Advanced Priest", "Complete the third dungeon level with the Priest."),
        new Achievement_t(AchievementSchema.Id.Priest3.ToString(), "Master Priest", "Complete the fourth dungeon level with the Priest."),
        new Achievement_t(AchievementSchema.Id.Priest4.ToString(), "Ascendant Priest", "Complete the fifth dungeon level with the Priest."),

        new Achievement_t(AchievementSchema.Id.Apothecary0.ToString(), "Novice Apothecary", "Complete the first dungeon level with the Apothecary."),
        new Achievement_t(AchievementSchema.Id.Apothecary1.ToString(), "Intermediate Apothecary", "Complete the second dungeon level with the Apothecary."),
        new Achievement_t(AchievementSchema.Id.Apothecary2.ToString(), "Advanced Apothecary", "Complete the third dungeon level with the Apothecary."),
        new Achievement_t(AchievementSchema.Id.Apothecary3.ToString(), "Master Apothecary", "Complete the fourth dungeon level with the Apothecary."),
        new Achievement_t(AchievementSchema.Id.Apothecary4.ToString(), "Ascendant Apothecary", "Complete the fifth dungeon level with the Apothecary."),

        new Achievement_t(AchievementSchema.Id.PacifistLovers0.ToString(), "Novice Matchmaker", "Complete the first dungeon level without defeating any Lovers."),
        new Achievement_t(AchievementSchema.Id.PacifistLovers1.ToString(), "Intermediate Matchmaker", "Complete the second dungeon level without defeating any Lovers."),
        new Achievement_t(AchievementSchema.Id.PacifistLovers2.ToString(), "Advanced Matchmaker", "Complete the third dungeon level without defeating any Lovers."),
        new Achievement_t(AchievementSchema.Id.PacifistLovers3.ToString(), "Master Matchmaker", "Complete the fourth dungeon level without defeating any Lovers."),
        new Achievement_t(AchievementSchema.Id.PacifistLovers4.ToString(), "Ascendant Matchmaker", "Complete the fifth dungeon level without defeating any Lovers."),

        new Achievement_t(AchievementSchema.Id.Merchant0.ToString(), "Novice Merchant", "Complete the first dungeon level with the Merchant."),
        new Achievement_t(AchievementSchema.Id.Merchant1.ToString(), "Intermediate Merchant", "Complete the second dungeon level with the Merchant."),
        new Achievement_t(AchievementSchema.Id.Merchant2.ToString(), "Advanced Merchant", "Complete the third dungeon level with the Merchant."),
        new Achievement_t(AchievementSchema.Id.Merchant3.ToString(), "Master Merchant", "Complete the fourth dungeon level with the Merchant."),
        new Achievement_t(AchievementSchema.Id.Merchant4.ToString(), "Ascendant Merchant", "Complete the fifth dungeon level with the Merchant."),

        new Achievement_t(AchievementSchema.Id.Gambler0.ToString(), "Novice Gambler", "Complete the first dungeon level with the Gambler."),
        new Achievement_t(AchievementSchema.Id.Gambler1.ToString(), "Intermediate Gambler", "Complete the second dungeon level with the Gambler."),
        new Achievement_t(AchievementSchema.Id.Gambler2.ToString(), "Advanced Gambler", "Complete the third dungeon level with the Gambler."),
        new Achievement_t(AchievementSchema.Id.Gambler3.ToString(), "Master Gambler", "Complete the fourth dungeon level with the Gambler."),
        new Achievement_t(AchievementSchema.Id.Gambler4.ToString(), "Ascendant Gambler", "Complete the fifth dungeon level with the Gambler."),

        new Achievement_t(AchievementSchema.Id.ItemAscetic0.ToString(), "Novice Ascetic", "Complete the third dungeon level with the Ascetic."),
        new Achievement_t(AchievementSchema.Id.ItemAscetic1.ToString(), "Intermediate Ascetic", "Complete the second dungeon level with the Ascetic."),
        new Achievement_t(AchievementSchema.Id.ItemAscetic2.ToString(), "Advanced Ascetic", "Complete the third dungeon level with the Ascetic."),
        new Achievement_t(AchievementSchema.Id.ItemAscetic3.ToString(), "Master Ascetic", "Complete the fourth dungeon level with the Ascetic."),
        new Achievement_t(AchievementSchema.Id.ItemAscetic4.ToString(), "Ascendant Ascetic", "Complete the fifth dungeon level with the Ascetic."),

        new Achievement_t(AchievementSchema.Id.Scribe0.ToString(), "Novice Scribe", "Complete the first dungeon level with the Scribe."),
        new Achievement_t(AchievementSchema.Id.Scribe1.ToString(), "Intermediate Scribe", "Complete the second dungeon level with the Scribe."),
        new Achievement_t(AchievementSchema.Id.Scribe2.ToString(), "Advanced Scribe", "Complete the third dungeon level with the Scribe."),
        new Achievement_t(AchievementSchema.Id.Scribe3.ToString(), "Master Scribe", "Complete the fourth dungeon level with the Scribe."),
        new Achievement_t(AchievementSchema.Id.Scribe4.ToString(), "Ascendant Scribe", "Complete the fifth dungeon level with the Scribe."),

        new Achievement_t(AchievementSchema.Id.Dryad0.ToString(), "Novice Dryad", "Complete the first dungeon level with the Dryad."),
        new Achievement_t(AchievementSchema.Id.Dryad1.ToString(), "Intermediate Dryad", "Complete the second dungeon level with the Dryad."),
        new Achievement_t(AchievementSchema.Id.Dryad2.ToString(), "Advanced Dryad", "Complete the third dungeon level with the Dryad."),
        new Achievement_t(AchievementSchema.Id.Dryad3.ToString(), "Master Dryad", "Complete the fourth dungeon level with the Dryad."),
        new Achievement_t(AchievementSchema.Id.Dryad4.ToString(), "Ascendant Dryad", "Complete the fifth dungeon level with the Dryad."),

        new Achievement_t(AchievementSchema.Id.Hardcore0.ToString(), "Hardcore", "Complete the first level without a Retry."),
        new Achievement_t(AchievementSchema.Id.Hardcore1.ToString(), "Hardcore+", "Complete the second level without a Retry."),
        new Achievement_t(AchievementSchema.Id.Hardcore2.ToString(), "Hardcore++", "Complete the third level without a Retry."),
        new Achievement_t(AchievementSchema.Id.Hardcore3.ToString(), "Hardcore+++", "Complete the fourth level without a Retry."),
        new Achievement_t(AchievementSchema.Id.Hardcore4.ToString(), "Hardcore++++", "Complete the fifth level without a Retry."),

        new Achievement_t(AchievementSchema.Id.DemonLord.ToString(), "DemonLord", "Defeat the Balrog."),
        new Achievement_t(AchievementSchema.Id.KillerSheep.ToString(), "Sheep Slayer", "Complete any dungeon after defeating 10 Sheep."),
        new Achievement_t(AchievementSchema.Id.KillerVampire.ToString(), "Vampire Slayer", "Complete any dungeon after defeating 10 Vampires."),
        new Achievement_t(AchievementSchema.Id.KillerWerewolf.ToString(), "Werewolf Slayer", "Complete any dungeon after defeating 4 Werewolf (not Humanoid)"),
    };

    // Our GameID
    private CGameID m_GameID;

    // Did we get the stats from Steam?
    private bool m_bRequestedStats;
    private bool m_bStatsValid;

    // Should we store stats this frame?
    private bool m_bStoreStats;

    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    protected Callback<UserStatsStored_t> m_UserStatsStored;
    protected Callback<UserAchievementStored_t> m_UserAchievementStored;

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
    }

    void OnEnable()
    {
        if (!SteamManager.Initialized)
            return;

        // Cache the GameID for use in the Callbacks
        m_GameID = new CGameID(SteamUtils.GetAppID());

        m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        // These need to be reset to get the stats upon an Assembly reload in the Editor.
        m_bRequestedStats = false;
        m_bStatsValid = false;
    }

    private void Update()
    {
        if (!SteamManager.Initialized)
            return;

        if (!m_bRequestedStats)
        {
            // Is Steam Loaded? if no, can't get stats, done
            if (!SteamManager.Initialized)
            {
                m_bRequestedStats = true;
                return;
            }
        }

        if (!m_bStatsValid)
            return;

        // Get info from sources

        //Store stats in the Steam database if necessary
        if (m_bStoreStats)
        {
            // already set any achievements in UnlockAchievement

            bool bSuccess = SteamUserStats.StoreStats();
            // If this failed, we never sent anything to the server, try
            // again later.
            m_bStoreStats = !bSuccess;
        }
    }

    /// <summary>
    /// Unlock achievement based off id. Id is AchievementSchema.Id ToString().
    /// </summary>
    /// <param name="achievementID"></param>
    public void UnlockAchievement(AchievementSchema.Id achievementID)
    {
        string achievementIdString = achievementID.ToString();
        Achievement_t achievement = m_Achievements.First(x => x.m_eAchievementID.Equals(achievementIdString, System.StringComparison.Ordinal));
        if (achievement != null)
        {
            UnlockAchievement(achievement);
            SteamUserStats.StoreStats();
        }
        else
        {
            Debug.LogWarning("Failed to find achievement with id: " + achievementID);
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Unlock this achievement
    //-----------------------------------------------------------------------------
    private void UnlockAchievement(Achievement_t achievement)
    {
        achievement.m_bAchieved = true;

        // the icon may change once it's unlocked
        //achievement.m_iIconImage = 0;

        // mark it down
        SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());

        // Store stats end of frame
        m_bStoreStats = true;
    }

    //-----------------------------------------------------------------------------
    // Purpose: We have stats data from Steam. It is authoritative, so update
    //			our data with those results now.
    //-----------------------------------------------------------------------------
    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (!SteamManager.Initialized)
            return;

        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("Received stats and achievements from Steam\n");

                m_bStatsValid = true;

                // load achievements
                foreach (Achievement_t ach in m_Achievements)
                {
                    bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
                    if (ret)
                    {
                        ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
                        ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
                    }
                    else
                    {
                        Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.m_eAchievementID + "\nIs it registered in the Steam Partner site?");
                    }
                }
            }
            else
            {
                Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Our stats data was stored!
    //-----------------------------------------------------------------------------
    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
            {
                // One or more stats we set broke a constraint. They've been reverted,
                // and we should re-iterate the values now to keep in sync.
                Debug.Log("StoreStats - some failed to validate");
                // Fake up a callback here so that we re-load the values.
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)m_GameID;
                OnUserStatsReceived(callback);
            }
            else
            {
                Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: An achievement was stored
    //-----------------------------------------------------------------------------
    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        // We may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (0 == pCallback.m_nMaxProgress)
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
            }
            else
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Display the user's stats and achievements
    //-----------------------------------------------------------------------------
    public void Render()
    {
        if (!SteamManager.Initialized)
        {
            GUILayout.Label("Steamworks not Initialized");
            return;
        }

        GUILayout.BeginArea(new Rect(Screen.width - 300, 0, 300, 800));
        foreach (Achievement_t ach in m_Achievements)
        {
            GUILayout.Label(ach.m_eAchievementID.ToString());
            GUILayout.Label(ach.m_strName + " - " + ach.m_strDescription);
            GUILayout.Label("Achieved: " + ach.m_bAchieved);
            GUILayout.Space(20);
        }

        // FOR TESTING PURPOSES ONLY!
        if (GUILayout.Button("RESET STATS AND ACHIEVEMENTS"))
        {
            SteamUserStats.ResetAllStats(true);
        }
        GUILayout.EndArea();
    }

    private class Achievement_t
    {
        public string m_eAchievementID;
        public string m_strName;
        public string m_strDescription;
        public bool m_bAchieved;

        /// <summary>
        /// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
        /// </summary>
        /// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
        /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
        /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
        public Achievement_t(string achievementID, string name, string desc)
        {
            m_eAchievementID = achievementID;
            m_strName = name;
            m_strDescription = desc;
            m_bAchieved = false;
        }
    }
#endif
}