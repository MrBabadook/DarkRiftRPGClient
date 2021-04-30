using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;
using System;

namespace DarkRiftRPG
{
    public class PlayFabLoginManager : MonoBehaviour
    {
        [Header("Screens")]
        public GameObject LoginPanel;
        public GameObject RegisterPanel;
        public GameObject CharacterSelectPanel;

        [Header("Login Screen")]
        public TMP_InputField LoginEmailField;
        public TMP_InputField LoginPasswordField;
        public Button LoginBtn;
        public Button RegisterBtn;

        [Header("Register Screen")]
        public TMP_InputField RegisterEmailField;
        public TMP_InputField RegisterDisplayNameField;
        public TMP_InputField RegisterPasswordwordField;
        public Button RegisterAccountBtn;
        public Button BackBtn;

        [Header("Character Select Screen")]
        public TMP_Text InfoText;
        public GameObject CharSlotOneBtn;
        public GameObject CharSlotTwoBtn;

        public static PlayFabLoginManager Instance;
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void OpenLoginPanel()
        {
            LoginPanel.SetActive(true);
            RegisterPanel.SetActive(false);
            CharacterSelectPanel.SetActive(false);
        }

        public void OpenRegistrationPanel()
        {
            LoginPanel.SetActive(false);
            CharacterSelectPanel.SetActive(false);
            RegisterPanel.SetActive(true);
        }

        public void OpenCharacterSelectPanel()
        {
            LoginPanel.SetActive(false);
            RegisterPanel.SetActive(false);
            CharacterSelectPanel.SetActive(true);

            TryRetrieveCharacters();
        }

        private  void TryRetrieveCharacters()
        {
            PlayFabCharacterManager.Instance.TryRetrievePlayerCharacters();
        }

        public void OnTryLogin()
        {
            string email = LoginEmailField.text;
            string password = LoginPasswordField.text;
            LoginPanelButtonsActive(false);

            LoginWithEmailAddressRequest loginWithEmailAddressRequest = new LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password
            };

            PlayFabClientAPI.LoginWithEmailAddress(loginWithEmailAddressRequest,
            res =>
            {
                ConnectionManager.Instance.LocalPlayerPlayFabID = res.PlayFabId;
                ConnectionManager.Instance.ConnectToServer(res.PlayFabId);
            },
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
                LoginPanelButtonsActive(false);
            });
        }

        private void LoginPanelButtonsActive(bool active)
        {
            LoginBtn.interactable = active;
            RegisterBtn.interactable = active;
        }

        public void OnTryRegisterNewAccount()
        {

            BackBtn.interactable = true;
            RegisterAccountBtn.interactable = true;

            string email = RegisterEmailField.text;
            string displayName = RegisterDisplayNameField.text;
            string password = RegisterPasswordwordField.text;

            RegisterPlayFabUserRequest req = new RegisterPlayFabUserRequest
            {
                Email = email,
                DisplayName = displayName,
                Password = password,
                RequireBothUsernameAndEmail = false
            };

            BackBtn.interactable = false;
            RegisterAccountBtn.interactable = false;

            PlayFabClientAPI.RegisterPlayFabUser(req,
            res =>
            {
                BackBtn.interactable = true;
                RegisterAccountBtn.interactable = true;
                OpenLoginPanel();
            },
            err =>
            {
                BackBtn.interactable = true;
                RegisterAccountBtn.interactable = true;
                Debug.Log("Error: " + err.ErrorMessage);
            });

        }


    }
}
