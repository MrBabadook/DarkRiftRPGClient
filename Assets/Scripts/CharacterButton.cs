using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DarkRiftRPG
{
    public class CharacterButton : MonoBehaviour
    {
        public TMP_Text nameText;
        public TMP_Text levelText;
        public TMP_Text xpText;
        public TMP_Text goldText;

        public bool statsSet;

        public void Init(string name, string level, string xp, string gold)
        {
            nameText.text = name;
            levelText.text = "Level\n" + level;
            xpText.text = "XP\n" + xp;
            goldText.text = "Gold\n" + gold;

            statsSet = true;
        }
    }
}

