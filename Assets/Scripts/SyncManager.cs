using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public class SyncManager : MonoBehaviour
{
    [SerializeField] private List<Transform> players;
    private string serverURL;
    private List<PlayerData> playersData;
    private List<PlayerData> lastPlayersData;

    private void Start()
    {
        serverURL = $"localhost:8000";
        playersData = new List<PlayerData>();

        //initialise playersData
        //players.ToList().ForEach(p => playersData.Add(new PlayerData(p.GetInstanceID(), p.name, p.position)));

        foreach (var player in players)
            playersData.Add(new PlayerData(player.GetInstanceID(), player.name, player.position));
    }

    private void Update() => syncRemotePos();

    private void syncRemotePos()
    {
        int ctr = 0;
        foreach (var player in players)
        {
            var pos = player.position;
            if (pos == playersData[ctr].pos) continue; //skip update to save time

            //playersData.RemoveAt(ctr);
            //playersData.Insert(ctr, new PlayerData(player.GetInstanceID(), player.name, player.position));
            playersData[ctr] = new PlayerData(player.GetInstanceID(), player.name, player.position);
            ctr++;
        }

        if (playersData != lastPlayersData)//players moved
        {
            string playersDataJson = JsonConvert.SerializeObject(playersData); //convert to JSON
            Debug.Log("moved");
            StartCoroutine(Post(serverURL, playersDataJson));
        }
        lastPlayersData = playersData;
    }

    IEnumerator Post(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //request.SetRequestHeader("Content-type", "text/html");
        //request.SetRequestHeader("Content-type", "text/plain");

        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error,this);
        }
        else
        {
            string dataRcvd = request.downloadHandler.text;
            //Debug.Log("Rcvd: " + dataRcvd);
            ProcessServerResponse(dataRcvd);
        }
    }

    void ProcessServerResponse(string rawResponse)
    {
        //foreach player
        //var playerData = JsonConvert.DeserializeObject<PlayerData>(playersDataJson); //deserialize JSON
        //serverPlayer.position = Vector3.Slerp(serverPlayer.position, newPos, networkSmoothingFactor );
    }
    private class PlayerData
    {
        //these will have to match field names on the server
        public int playerID;
        public string playerName;
        public Vector3 pos;

        public PlayerData(int playerID, string playerName, Vector3 pos)
        {
            this.playerID = playerID;
            this.playerName = playerName;
            this.pos = pos;
        }
    }
}

#region using Unity JsonUtility
//PlayerData d1 = JsonUtility.FromJson<PlayerData>(rawResponse);
//Debug.Log($"PlayerID:{d1.playerID}, Pos{d1.pos.X}");

#endregion
