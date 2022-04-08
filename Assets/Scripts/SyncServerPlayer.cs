using SimpleJSON;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SyncServerPlayer : MonoBehaviour
{
    [Range(0.01f,10f)] private float networkSmoothingFactor = 0.5f;
    [SerializeField] private string playerID = "Player1";//replaced with unique ID at runtime
    
    private Vector3 playerPosition = Vector3.zero;

    private void Awake() => playerID = gameObject.GetInstanceID().ToString();
    private void Update()
    {

        syncRemotePos();
    }

    void syncRemotePos()
    {
        playerPosition = transform.position;

        string url = $"localhost:8000/player" +
            $"/{playerID}" +
            $"/{playerPosition.x}" +
            $"/{playerPosition.y}" +
            $"/{playerPosition.z}";

        StartCoroutine(GetDataFromServer(url));
    }

    IEnumerator GetDataFromServer(string serverURL)
    {
        var startTime = Time.time;
        UnityWebRequest www = UnityWebRequest.Get(serverURL);
        yield return www.SendWebRequest();
        var endTime = Time.time;
        var elapsedTime = endTime - startTime;
        //Debug.Log($"Time Elapsed{elapsedTime}, i.e.{1/elapsedTime}/second");

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Something went wrong: " + www.error);
        }
        else
        {
            string dataRcvd = www.downloadHandler.text;
            //Debug.Log("Rcvd: " + dataRcvd);
            ProcessServerResponse(dataRcvd);
        }
    }

    void ProcessServerResponse(string rawResponse)
    {
        JSONNode node = JSON.Parse(rawResponse);
        string playerID = node["playerID"].Value;
        //if (playerID == this.playerID) return; //skip current player

        float x = node["pos"][0]["value"];
        float y = node["pos"][1]["value"];
        float z = node["pos"][2]["value"];

        Debug.Log($"{playerID} Remote pos: ({x},{y},{z})");
        var newPos = new Vector3(x, y, z);
        transform.position = Vector3.Slerp(transform.position, newPos,Time.deltaTime * networkSmoothingFactor);
    }
}

#region using Unity JsonUtility
//PlayerData d1 = JsonUtility.FromJson<PlayerData>(rawResponse);
//Debug.Log($"PlayerID:{d1.playerID}, Pos{d1.pos.X}");
public class PlayerData
{
    //these will have to match field names on the server
    public string playerID;
    public string playerName;
    public POS pos;

    public class POS
    {
        public float X, Y, Z;
    }
}
#endregion
