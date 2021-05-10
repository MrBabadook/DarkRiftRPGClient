using System;
using System.Net;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarkRiftRPG
{
    public class ConnectionManager : MonoBehaviour
    {
        //We want a  reference to ConnectionManager so it can be called directly from other scripts
        public static ConnectionManager Instance;
        //A reference to the Client component on this game object. 
        public UnityClient Client { get; private set; }

        public string LocalPlayerPlayFabID;
        public ushort LocalClientID;

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
        void Start()
        {
            Client = GetComponent<UnityClient>();
            PlayFabCharacterManager.Instance.Init();
        }

        public void SendClickPosToServer(Vector3 point)
        {
            using (Message message = Message.Create((ushort)Tags.PlayerMovementRequest, new PlayerMovementRequestData(point)))
            {
                Client.SendMessage(message, SendMode.Reliable);
            }
        }

        
        public void ConnectToServer(string playFabId)
        {
            
            Client.ConnectInBackground(IPAddress.Loopback, Client.Port, false, ConnectCallback);
            Client.MessageReceived += OnMessage;
        }

        private void ConnectCallback(Exception e)
        {
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                using (Message message = Message.Create((ushort)Tags.JoinServerRequest, new JoinServerRequestData(LocalPlayerPlayFabID)))
                {
                    Client.SendMessage(message, SendMode.Reliable);
                }
            }
            else
            {
                Debug.LogError($"Unable to connect to server. Reason: {e.Message} ");
            }
        }

        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            using (Message m = e.GetMessage())
            {
                switch ((Tags)m.Tag)
                {
                    case Tags.JoinServerResponse:
                        OnJoinServerResponse(m.Deserialize<JoinServerResponseData>());
                        break;
                    case Tags.PlayerMovementUpdate:
                        OnPlayerMovementUpdate(m.Deserialize<ProccessedPlayerMovementData>());
                        break;
                    case Tags.SpawnPlayer:
                        OnSpawnPlayer(m.Deserialize<PlayerSpawnData>());
                        break;
                    case Tags.DespawnPlayer:
                        OnDespawnPlayer(m.Deserialize<PlayerDespawnData>());
                        break;
                }
            }
        }

        private void OnJoinServerResponse(JoinServerResponseData data)
        {
            if (!data.JoinServerRequestAccepted)
            {
                Debug.Log("Unable to join server");
                return;
            }

            LocalClientID = Client.Client.ID;

            PlayFabLoginManager.Instance.OpenCharacterSelectPanel();
        }

        public void OnTryCreateNewCharacter(string name)
        {
            using (Message message = Message.Create((ushort)Tags.RegisterNewCharacterRequest, new RegisterNewCharacterRequestData(name)))
            {
                Client.SendMessage(message, SendMode.Reliable);
            }
        }

        private void OnDespawnPlayer(PlayerDespawnData data)
        {
            GameManager.Instance.RemovePlayerFromGame(data);
        }

        private void OnSpawnPlayer(PlayerSpawnData data)
        {
            if (GameManager.Instance != null)
            {
                //Sanity Check
                Debug.Log(data.ID);
                Debug.Log(data.PlayerCharacterName);
                Debug.Log(data.Position.ToString());
                GameManager.Instance.SpawnPlayer(data);
            } else
            {
                Debug.Log("Called too soon");
            }
            
        }

        private void OnPlayerMovementUpdate(ProccessedPlayerMovementData data)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.HandlePlayerMovementUpdate(data);
        }

        public void TryJoinGameAsCharacterRequest(string characterID)
        {
            Debug.Log("Inside TryJoinGameAsCharacterRequest, passing characterID " + characterID);
            using (Message message = Message.Create((ushort)Tags.JoinGameAsCharacterRequest, new JoinGameAsCharacterRequestData(characterID)))
            {
                Client.SendMessage(message, SendMode.Reliable);
            }
        }

        private void OnDestroy()
        {
            Client.MessageReceived -= OnMessage;
        }

    }

}
