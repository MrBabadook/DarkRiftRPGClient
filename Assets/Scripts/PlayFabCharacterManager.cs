using UnityEngine;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DarkRiftRPG
{
    //Should manage calls between playfab api and characters retreieved from playfab
    public class PlayFabCharacterManager : MonoBehaviour
    {
        public List<CharacterResult> Characters = new List<CharacterResult>();

        public static PlayFabCharacterManager Instance;

        public GameObject PlayerCharacterSlot1;
        public GameObject PlayerCharacterSlot2;
        public GameObject NewCharacterCreatePanel;

        List<Character> playerCharacters = new List<Character>();

        public Character currentSelectedCharacter;

        public TMP_Text InfoText;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        public void Init()
        {
            ConnectionManager.Instance.Client.MessageReceived += OnMessage;
        }
        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            using (Message m = e.GetMessage())
            {
                switch ((Tags)m.Tag)
                {
                    case Tags.RegisterNewCharacterResponse:
                        OnRegisterNewCharacterResponse(m.Deserialize<RegisterNewCharacterResponseData>());
                        break;

                }
            }
        }

        private void OnRegisterNewCharacterResponse(RegisterNewCharacterResponseData data)
        {
            if (!data.RegisteredSuccessfully)
            {
                InfoText.text = "Error Creating New Character.";
                return;
            }

            InfoText.text = "New Character Created Successfull! Retrieving Character.";
            TryRetrievePlayerCharacters();
        }

        public void TryRetrievePlayerCharacters()
        {
            //Create a request that will return all characters for a given player
            ListUsersCharactersRequest listUsersCharactersRequest = new ListUsersCharactersRequest
            {
                PlayFabId = ConnectionManager.Instance.LocalPlayerPlayFabID
            };

            //All characters contained in single result
            PlayFabClientAPI.GetAllUsersCharacters(listUsersCharactersRequest,
            result =>
            {
                playerCharacters.Clear(); // empty out existing list of characters

                foreach (CharacterResult characterResult in result.Characters)
                {
                    //Create new character and set ID and Name
                    Character characterData = new Character();
                    characterData.CharacterID = characterResult.CharacterId;
                    characterData.CharacterName = characterResult.CharacterName;

                    playerCharacters.Add(characterData);
                }

                GetStatsForPlayerCharacters(playerCharacters);
                ShowCharacterSlotButtons(playerCharacters.Count);

            },
            error =>
            {
                Debug.Log("Error: " + error.ErrorMessage);
            });
        }

        private void GetStatsForPlayerCharacters(List<Character> characters)
        {
            foreach (Character playerCharacter in characters)
            {
                GetCharacterStatisticsRequest characterStatsReq = new GetCharacterStatisticsRequest
                {
                    CharacterId = playerCharacter.CharacterID
                };

                PlayFabClientAPI.GetCharacterStatistics(characterStatsReq,
                result =>
                {
                    //Get that character
                    Character character = playerCharacters.Find(x => x.CharacterID == characterStatsReq.CharacterId);
                    //ID and Name already set and are not stats - set level, xp, and gold
                    SetStatsForCharacter(result, character);

                    SetCharacterButtonInfo(character);
                },
                error =>
                {
                    InfoText.text = "Error getting character stats";
                    Debug.Log(error.ErrorMessage);
                });
            };

            ShowCharacterSlotButtons(playerCharacters.Count);
        }

        private static void SetStatsForCharacter(GetCharacterStatisticsResult result, Character character)
        {
            character.CharacterLevel = result.CharacterStatistics["Level"].ToString();
            character.CharacterXP = result.CharacterStatistics["XP"].ToString();
            character.CharacterGold = result.CharacterStatistics["Gold"].ToString();
        }

        private void ShowCharacterSlotButtons(int characterCount)
        {
            PlayerCharacterSlot1.transform.GetChild(0).gameObject.SetActive(false);
            PlayerCharacterSlot1.transform.GetChild(1).gameObject.SetActive(false);
            PlayerCharacterSlot2.transform.GetChild(0).gameObject.SetActive(false);
            PlayerCharacterSlot2.transform.GetChild(1).gameObject.SetActive(false);

            switch (characterCount)
            {
                case 0:
                    //No characters
                    PlayerCharacterSlot1.transform.GetChild(0).gameObject.SetActive(true);
                    PlayerCharacterSlot2.transform.GetChild(0).gameObject.SetActive(true);
                    break;
                case 1:
                    //One character
                    PlayerCharacterSlot1.transform.GetChild(1).gameObject.SetActive(true);
                    PlayerCharacterSlot2.transform.GetChild(0).gameObject.SetActive(true);
                    break;
                case 2:
                    //Two characters
                    PlayerCharacterSlot1.transform.GetChild(1).gameObject.SetActive(true);
                    PlayerCharacterSlot2.transform.GetChild(1).gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        private void SetCharacterButtonInfo(Character character)
        {
            CharacterButton[] CharacterButtons = FindObjectsOfType<CharacterButton>();

            for (int i = 0; i < CharacterButtons.Length; i++)
             {
                if (!CharacterButtons[i].statsSet)
                {
                    CharacterButtons[i].Init(character.CharacterName, character.CharacterLevel, character.CharacterXP, character.CharacterGold);
                    CharacterButtons[i].gameObject.GetComponent<Button>().onClick.AddListener(() => { TryJoinGameAsCharacter(character); });
                    return;
                }
            }
            
        }

        private void TryJoinGameAsCharacter(Character character)
        {
            currentSelectedCharacter = character;
            SceneManager.LoadScene("Game");
        }

        public void OpenCreateNewCharacterPrompt()
        {
            NewCharacterCreatePanel.SetActive(true);
        }

        public void CloseCreateNewCharacterPrompt()
        {
            NewCharacterCreatePanel.SetActive(false);
        }

        public void OnTryCreateCharacter(TMP_InputField nameInput)
        {
            string desiredCharacterName = nameInput.text;

            if (string.IsNullOrEmpty(desiredCharacterName))
            {
                InfoText.text = "Name cannot be empty";
            }
            else if (desiredCharacterName.Length < 3)
            {
                InfoText.text = "Name cannot be less then 3 character";
            }

            ConnectionManager.Instance.OnTryCreateNewCharacter(desiredCharacterName);

            CloseCreateNewCharacterPrompt();
        }

        private void OnDestroy()
        {
            ConnectionManager.Instance.Client.MessageReceived -= OnMessage;
        }
    }
}

