using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using System;
using System.Linq;

namespace DarkRiftRPG
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public GameObject playerPrefab;
        public Dictionary<ushort, GameObject> ConnectedPlayers = new Dictionary<ushort, GameObject>();
        public ushort LocalPlayerID;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            LocalPlayerID = ConnectionManager.Instance.LocalClientID;
            Debug.Log("Local Player ID set to: " + LocalPlayerID);

            ConnectionManager.Instance.TryJoinGameAsCharacterRequest(PlayFabCharacterManager.Instance.currentSelectedCharacter.CharacterID);
        }

        public void SendClickPosToServer(Vector3 point)
        {
            ConnectionManager.Instance.SendClickPosToServer(point);
        }

        public void HandlePlayerMovementUpdate(ProccessedPlayerMovementData data)
        {
            List<PlayerPositionInputData> playerPositions = data.ProccessedMovementUpdate.ToList();
            foreach (PlayerPositionInputData pos in playerPositions)
            {
                if (ConnectedPlayers.ContainsKey(pos.ID))
                {
                    PlayerController controller = ConnectedPlayers[pos.ID].GetComponent<PlayerController>();
                    controller.SetNewDest(pos.Pos);
                }
            }
        }

        public void SpawnPlayer(PlayerSpawnData data)
        {
            if (!ConnectedPlayers.ContainsKey(data.ID))
            {
                GameObject go = Instantiate(playerPrefab, data.Position, Quaternion.identity);
                go.GetComponentInChildren<TextMesh>().text = data.PlayerCharacterName;

                //Set camera to follow player
                if (data.ID == LocalPlayerID)
                {
                    Camera.main.gameObject.GetComponent<SimpleCameraController>().Init(go);
                }

                ConnectedPlayers.Add(data.ID, go);
            }
        }

        public void RemovePlayerFromGame(PlayerDespawnData data)
        {
            if (ConnectedPlayers.ContainsKey(data.ID))
            {
                Destroy(ConnectedPlayers[data.ID]);
            }
        }
    }
}
