using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SyncManager : MonoBehaviour
{
    [SerializeField] [Range(0.01f, 1f)] private float networkSmoothingFactor = 0.5f; //1 being realtime
    [SerializeField] private bool simulateNetworkPlayer;
    [SerializeField] [Range(0.0f, 1f)] private float simulatedDelay;
    [SerializeField] private TMP_Text fpsText;
    private static List<Transform> players = new List<Transform>();

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
        lastPlayersDataSent = new List<PlayerData>();

        //initialise playersData after spawn
        if (players.Count > 0)
        {
            players.ToList().ForEach(p => playersDataSent.Add(new PlayerData(
                p.GetInstanceID(),
                p.localPosition.Shorten(2),
                p.localRotation.eulerAngles.Shorten(2)
                )));
        }
    }
    private void Update() => RefreshPlayersData();
    
    private void RefreshPlayersData()
    {
        if (players.Count == 0) return;

        int ctr = 0;
        bool dataChanged = false; //reset changes flag
        foreach (var player in players)
        {
            if (lastPlayersDataSent.Count < 1) break ;
            if (player.localPosition.Shorten(2) == lastPlayersDataSent[ctr].Pos &&
                player.localRotation.eulerAngles.Shorten(2) == lastPlayersDataSent[ctr].Rot) continue; //skip player update

            //A Player's Data has changed!
            dataChanged = true;
            //what changed?
            Debug.Log($"Changed: {player.name}, Old Pos: {lastPlayersDataSent[ctr].Pos}, New Pos:{player.localPosition.Shorten(2)}, " +
                $"new rot {player.localRotation.eulerAngles.Shorten(2)}");

            playersDataSent.RemoveAt(ctr);
            playersDataSent.Insert(ctr, new PlayerData(
                player.GetInstanceID(),
                player.localPosition.Shorten(2),
                player.localRotation.eulerAngles.Shorten(2)
                ));
            ctr++;
        }

        //bool dataChanged = !Enumerable.SequenceEqual(playersDataSent, lastPlayersDataSent);
        if (dataChanged)
        {
            var changesFound = new List<PlayerData>();
            changesFound = lastPlayersDataSent.Except(playersDataSent).ToList();
            //string playersDataJson = JsonConvert.ToJson(changesFound);
            string playerDataJson = JsonConvert.SerializeObject(playersDataSent);

            //Debug.Log(playerDataJson);
            StartCoroutine(Post(serverURL, playerDataJson));
        }
        lastPlayersDataSent = playersDataSent;
    }

    private IEnumerator Post(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "GET");//POST
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("Content-type", "text/plain");
        request.SetRequestHeader("Content-Type", "application/json");

        var startTime = Time.time;
        yield return request.SendWebRequest();
        yield return new WaitForSeconds(simulatedDelay);
        LogServerTime(startTime);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error, this);
        }
        else
        {
            string dataRcvd = request.downloadHandler.text;
            //Debug.Log("Rcvd: " + dataRcvd);
            ProcessServerResponse(dataRcvd);
        }
    }

    private void ProcessServerResponse(string rawResponse)
    {
        playersDataRcvd = JsonConvert.DeserializeObject<List<PlayerData>>(rawResponse);

        foreach (var playerData in playersDataRcvd)
        {
            foreach (var player in players)
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
        if (!simulateNetworkPlayer || !Application.isPlaying || playersDataRcvd == null) return;
        Gizmos.color = new Color(0, 1, 0, 0.6f);

        foreach (var playerData in playersDataRcvd)
        {
            foreach (var player in players)
            {
                if (player == transform)
                {
                    continue; //skips container
                }

                if (player.GetInstanceID() == playerData.PlayerID)
                {
                    var smoothedPos = Vector3.Slerp(player.localPosition, playerData.Pos, networkSmoothingFactor * Time.deltaTime);
                    //Gizmos.matrix = player.worldToLocalMatrix;
                    Gizmos.DrawCube(playerData.Pos, Vector3.one * 1.2f);
                }
            }
        }
    }
    public static void AddPlayer(Transform player) => players.Add(player);
}

[System.Serializable]
public class PlayerData
{
    //these will have to match field names on the server
    public int PlayerID;
    public Vector3 Pos;
    public Vector3 Rot;

    public PlayerData(int playerID, Vector3 pos, Vector3 rot)
    {
        PlayerID = playerID;
        Pos = pos;
        Rot = rot;
    }
}

public static class TransformExtensions
{
    public static Vector3 Shorten(this Vector3 pos, int precesion)
    {
        var xx = Math.Round(Convert.ToDouble(pos.x), precesion);
        var yy = Math.Round(Convert.ToDouble(pos.y), precesion);
        var zz = Math.Round(Convert.ToDouble(pos.z), precesion);

        return new Vector3((float)xx, (float)yy, (float)zz);
    }
}