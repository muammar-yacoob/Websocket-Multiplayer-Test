using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using Newtonsoft.Json;

public class SyncManager : MonoBehaviour
{
    [SerializeField] private List<Transform> players;
    [SerializeField] [Range(0.01f, 1f)] private float networkSmoothingFactor = 0.5f; //1 being realtime
    [SerializeField] bool simulateNetworkPlayer;
    [SerializeField] [Range(0.0f, 1f)] private float simulatedDelay;
    [SerializeField] TMP_Text fpsText;

    private string serverURL;
    private List<PlayerData> lastPlayersDataSent;
    private List<PlayerData> playersDataSent;
    private List<PlayerData> playersDataRcvd;
    private float lastFPS;
    private float FPSUpdateThresh = 1f;


    private void Start()
    {
        serverURL = $"localhost:8000/unity";
        playersDataSent = new List<PlayerData>();

        //initialise playersData
        //players.ToList().ForEach(p => playersData.Add(new PlayerData(p.GetInstanceID(), p.name, p.position)));

        foreach (var player in players)
        {
            //populate initial players data
            playersDataSent.Add(new PlayerData(player.GetInstanceID(), player.name, player.position));
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
        playersDataSent.Clear();//Slower
        foreach (var player in players)
        {
            var pos = player.position;
            //if (pos == playersData[ctr].pos) continue; //skip update to save time

            //playersData.RemoveAt(ctr);
            //playersData.Insert(ctr, new PlayerData(player.GetInstanceID(), player.name, player.position));
            //playersData[ctr] = 
            //ctr++;
            var pData = new PlayerData(player.GetInstanceID(), player.name, player.position);
            playersDataSent.Add(pData);
        }

        //if (playersDataSent != lastPlayersDataSent)//players moved
        {
            //Debug.Log("player moved, sending...");
            string playersDataJson = JsonConvert.SerializeObject(playersDataSent); //convert to JSON
            StartCoroutine(Post(serverURL, playersDataJson));
        }
        lastPlayersDataSent = playersDataSent;
    }

    IEnumerator Post(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "GET");//POST
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //request.SetRequestHeader("Content-type", "text/plain");
        request.SetRequestHeader("Content-Type", "application/json");

        var startTime = Time.time;
        yield return request.SendWebRequest();
        yield return new WaitForSeconds(simulatedDelay);
        LogServerTime(startTime);

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
        playersDataRcvd = JsonConvert.DeserializeObject<List<PlayerData>>(rawResponse);

        foreach (var playerData in playersDataRcvd)
        {
            foreach(var player in players)
            {
                if (player.GetInstanceID() == playerData.PlayerID)
                {
                    player.position = Vector3.Slerp(player.position, playerData.Pos, networkSmoothingFactor * Time.deltaTime);
                }
            }
        }
    }

    private void LogServerTime(float startTime)
    {
        var endTime = Time.time;
        var elapsedTime = endTime - startTime;
        var currentFPS = Mathf.Round(1 / elapsedTime);
        if (Mathf.Abs(currentFPS - lastFPS) < FPSUpdateThresh)
        {
            fpsText.text = "Network FPS: " + currentFPS.ToString("00");
        }
        lastFPS = currentFPS;
        //Debug.Log($"Time Elapsed{elapsedTime}, i.e.{currentFPS}");

    }

    private void OnDrawGizmos()
    {
        if (!simulateNetworkPlayer || !Application.isPlaying) return;

        Gizmos.color = new Color(0, 1, 0, 0.6f);
        if (playersDataRcvd == null) return;
        foreach (var playerData in playersDataRcvd)
        {
            foreach (var player in players)
            {
                if (player.GetInstanceID() == playerData.PlayerID)
                {
                    var smoothedPos = Vector3.Slerp(player.position, playerData.Pos, networkSmoothingFactor * Time.deltaTime);
                    Gizmos.DrawCube(playerData.Pos, Vector3.one * 1.2f);
                }
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
