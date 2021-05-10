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

        public bool isCharacterDataSet;

        public void Init(string name, int level, int xp, int gold)
        {
            nameText.text = name;
            levelText.text = "Level\n" + level.ToString();
            xpText.text = "XP\n" + xp.ToString();
            goldText.text = "Gold\n" + gold.ToString();

            isCharacterDataSet = true;
        }
    }
}

