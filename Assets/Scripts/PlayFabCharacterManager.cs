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
using PlayFab.Json;
using PlayFab.DataModels;

namespace DarkRiftRPG
{
    //Should manage calls between playfab api and characters retreieved from playfab
    public class PlayFabCharacterManager : MonoBehaviour
    {
        public static PlayFabCharacterManager Instance;

        public List<GameObject> CharacterSlots = new List<GameObject>();

        public GameObject NewCharacterCreatePanel;

        [SerializeField]
        List<CharacterData> playerCharacters = new List<CharacterData>();

        [SerializeField]
        List<CharacterButton> characterButtons;

        public CharacterData currentSelectedCharacter;

        public TMP_Text InfoText;
        int characterResultCount = 0;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(this);
        }
        public void Init()
        {
            ConnectionManager.Instance.Client.MessageReceived += OnMessage;
            characterButtons = FindObjectsOfType<CharacterButton>().ToList();

            foreach (CharacterButton characterButton in characterButtons)
            {
                characterButton.gameObject.SetActive(false);
            }
            PlayFabLoginManager.Instance.OpenLoginPanel();
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

            InfoText.text = "New Character Create Successfull!";

            TryRetrievePlayerCharacters();
        }

        public void TryRetrievePlayerCharacters()
        {
            ClearPlayerCharacterList(); //Clear current list of existing characters
            ResetCharacterButtons();
            ShowCharacterButtons();
            //Create a request that will return all characters for a given player
            ListUsersCharactersRequest listUsersCharactersRequest = new ListUsersCharactersRequest
            {
                PlayFabId = ConnectionManager.Instance.LocalPlayerPlayFabID
            };

            //All characters contained in single result
            PlayFabClientAPI.GetAllUsersCharacters(listUsersCharactersRequest,
            result =>
            {
                characterResultCount = result.Characters.Count; // how many came back?

                foreach (CharacterResult characterResult in result.Characters)
                {
                    GetDataForCharacter(characterResult);
                }
            },
            error =>
            {
                Debug.Log("Error: " + error.ErrorMessage);
            });
        }

        private void GetDataForCharacter(CharacterResult characterResult)
        {
            var characterEntityKey = new PlayFab.DataModels.EntityKey { Id = characterResult.CharacterId, Type = "character" };

            GetObjectsRequest getCharaterObjectDataRequest = new GetObjectsRequest
            {
                Entity = characterEntityKey,
                EscapeObject = true
            };

            PlayFabDataAPI.GetObjects(getCharaterObjectDataRequest,
            result =>
            {
                Debug.Log("result.Objects[\"CharacterData\"].EscapedDataObject: " + result.Objects["CharacterData"].EscapedDataObject);
               
                CharacterData characterData = PlayFabSimpleJson.DeserializeObject<CharacterData>(result.Objects["CharacterData"].EscapedDataObject);
                playerCharacters.Add(characterData);
                Debug.Log($"playerCharacters added to, count is {playerCharacters.Count}");

                SetCharacterButtonData(characterData);
                ShowCharacterButtons();

            },
            error =>
            {
                InfoText.text = "Error getting character stats";
                Debug.Log(error.ErrorMessage);
            });

           
        }

        private void SetCharacterButtonData(CharacterData characterData)
        {
            foreach (var slot in CharacterSlots)
            {
                CharacterSlot characterSlot = slot.GetComponent<CharacterSlot>();
                CharacterButton characterButton = characterSlot.CharacterSelectBtn.GetComponent<CharacterButton>();
                if (!characterButton.isCharacterDataSet)
                {
                    characterButton.Init(characterData.CharacterName, characterData.CharacterLevel, characterData.CharacterXP, characterData.CharacterGold);
                    characterButton.GetComponent<Button>().onClick.AddListener(() => TryJoinGameAsCharacter(characterData));
                    break;
                }
          
            }
        }

        private void ShowCharacterButtons()
        {

            if (CharacterSlots.Count == 0)
            {
                foreach (var slot in CharacterSlots)
                {
                    slot.GetComponent<CharacterSlot>().EmptySlotBtn.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (GameObject slot in CharacterSlots)
                {
                    CharacterSlot characterSlot = slot.GetComponent<CharacterSlot>();
                    CharacterButton characterButton = characterSlot.CharacterSelectBtn.GetComponent<CharacterButton>();

                    if (characterButton.isCharacterDataSet)
                    {
                        characterSlot.CharacterSelectBtn.SetActive(true);
                        characterSlot.EmptySlotBtn.SetActive(false);
                    }
                    else
                    {
                        characterSlot.EmptySlotBtn.gameObject.SetActive(true);
                        characterSlot.CharacterSelectBtn.SetActive(false);
                    }
                }
            }
        }

        private void ResetCharacterButtons()
        {
            foreach (var slot in CharacterSlots)
            {
                slot.GetComponent<CharacterSlot>().CharacterSelectBtn.GetComponent<CharacterButton>().isCharacterDataSet = false;
            }
        }
        private void ClearPlayerCharacterList()
        {
            playerCharacters.Clear();
        }

        private void TryJoinGameAsCharacter(CharacterData character)
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
            nameInput.text = string.Empty;
        }

        private void OnDestroy()
        {
            ConnectionManager.Instance.Client.MessageReceived -= OnMessage;
        }
    }
}

