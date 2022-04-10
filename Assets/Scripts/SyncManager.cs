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
    [SerializeField] [Range(0.01f, 1f)] private float networkSmoothingFactor = 0.5f; //1 being realtime
    private string serverURL;
    private List<PlayerData> playersData;
    private List<PlayerData> lastPlayersData;

    private void Start()
    {
        serverURL = $"localhost:8000/unity";
        playersData = new List<PlayerData>();

        //initialise playersData
        //players.ToList().ForEach(p => playersData.Add(new PlayerData(p.GetInstanceID(), p.name, p.position)));

        foreach (var player in players)
        {
            //populate initial players data
            playersData.Add(new PlayerData(player.GetInstanceID(), player.name, player.position));
            CreateServerGhost(player);
        }
    }

    private void CreateServerGhost(Transform player)
    {
        //add network transform debugger
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = player;
        cube.transform.localScale = Vector3.one / 3;
        cube.transform.localPosition = Vector3.zero;
        var cubeColor = Color.cyan;
        cubeColor.a = 0.3f;
        cube.GetComponent<MeshRenderer>().material.color = cubeColor;
        cube.name = $"{player.name}-network transform";
    }

    private void Update() => syncRemotePos();

    private void syncRemotePos()
    {
        //int ctr = 0;
        playersData.Clear();//Slower
        foreach (var player in players)
        {
            var pos = player.position;
            //if (pos == playersData[ctr].pos) continue; //skip update to save time

            //playersData.RemoveAt(ctr);
            //playersData.Insert(ctr, new PlayerData(player.GetInstanceID(), player.name, player.position));
            //playersData[ctr] = 
            //ctr++;
            var pData = new PlayerData(player.GetInstanceID(), player.name, player.position);
            playersData.Add(pData);
        }

        //if (playersData != lastPlayersData)//players moved
        {
            //Debug.Log("moved");
            string playersDataJson = JsonConvert.SerializeObject(playersData); //convert to JSON
            StartCoroutine(Post(serverURL, playersDataJson));
        }
        lastPlayersData = playersData;
    }

    IEnumerator Post(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "GET");//POST
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
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
            Debug.Log("Rcvd: " + dataRcvd);
            ProcessServerResponse(dataRcvd);
        }
    }

    void ProcessServerResponse(string rawResponse)
    {
        List<PlayerData> playersData = JsonConvert.DeserializeObject<List<PlayerData>>(rawResponse);

        //foreach player
        foreach (var playerData in playersData)
        {
            foreach(var player in players)
            {
                var serverPlayer = player.GetComponentInChildren<MeshRenderer>().transform;
                serverPlayer.position = Vector3.Slerp(serverPlayer.position, player.position, networkSmoothingFactor );
            }
        }
    }
}


public class PlayerData
{
    //these will have to match field names on the server
    public int PlayerID;
    public string PlayerName;
    public Vector3 Pos;

    public PlayerData(int playerID, string playerName, Vector3 pos)
    {
        PlayerID = playerID;
        PlayerName = playerName;
        Pos = pos;
    }
}

#region using Unity JsonUtility
//PlayerData d1 = JsonUtility.FromJson<PlayerData>(rawResponse);
//Debug.Log($"PlayerID:{d1.playerID}, Pos{d1.pos.X}");

#endregion
