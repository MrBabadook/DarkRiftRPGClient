using System;

namespace DarkRiftRPG
{

    public struct CharacterStats
    {
        public string Level;
        public string XP;
        public string Gold;
    }

    public class Character
    {
        event Action OnCharacterInfoUpdated;
        public string CharacterID { get; set; }
        public string CharacterName { get; set; }

        public CharacterStats stats;
        public string CharacterLevel { get; set; }
        public string CharacterXP { get; set; }
        public string CharacterGold { get; set; }

        public  void UpdatePlayerData(string id, string name, string level, string xp, string gold)
        {
            //comes from character data api calls
            CharacterID = id ?? CharacterID ?? "";
            CharacterName = name ?? CharacterName ?? "";

            //Comes from playfab stat api calls
            stats.Level = level ?? stats.Level ?? "";
            stats.XP = xp ?? stats.XP ?? "";
            stats.Gold = gold ?? stats.Gold ?? "";

            OnCharacterInfoUpdated?.Invoke();
        }

    }
}

