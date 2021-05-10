using System;
using UnityEngine;

namespace DarkRiftRPG
{
    public class CharacterData
    {
        public string PlayFabID { get; set; }
        public string CharacterID { get; set; }
        public string CharacterName { get; set; }
        public int CharacterLevel { get; set; }
        public int CharacterXP { get; set; }
        public int CharacterGold { get; set; }

        public Vector3 WorldPosition { get; set; } 
        
        public bool IsInitialCharacterData { get; set; }


        public CharacterData(string playfabID, string characterID, string name, int level, int xp, int gold, Vector3 position)
        {
            PlayFabID = playfabID;
            CharacterID = characterID;
            CharacterName = name;
            CharacterLevel = level;
            CharacterXP = xp;
            CharacterGold = gold;
            WorldPosition = position;
        }
        public CharacterData() { }

    }
}

