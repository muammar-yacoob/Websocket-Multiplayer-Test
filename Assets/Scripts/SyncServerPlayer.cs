using Newtonsoft.Json;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SyncServerPlayer : MonoBehaviour
{
    [Header("Network Parameters")]
    [SerializeField] [Range(0.01f, 1f)] private float networkSmoothingFactor = 0.05f; //1 being realtime
    [SerializeField] TMP_Text fpsText;
    [SerializeField] bool simulateNetworkPlayer;
    [SerializeField] [Range(0.0f, 1f)] private float simulatedDelay;

    private float FPSUpdateThresh = 1f;
    private float lastFPS;
    private string serverURL;
    private Vector3 serverPos;

    private void Awake()
    {
        serverURL = $"localhost:8000/unity";
    }


    private void LateUpdate()
    {
        syncRemotePos();
    }

    void syncRemotePos()
    {
        var playerData = new PlayerData(transform.GetInstanceID(), transform.position);
        string playersDataJson = JsonConvert.SerializeObject(playerData); //convert to JSON

        StartCoroutine(Post(serverURL, playersDataJson));
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
            Debug.LogError(request.error, this);
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
        var playerData = JsonConvert.DeserializeObject<PlayerData>(rawResponse);
        serverPos = playerData.Pos;
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

        //var smoothedPos = Vector3.Slerp(transform.position, serverPos, networkSmoothingFactor * Time.deltaTime);
        Gizmos.color = new Color(0,1, 0, 0.6f);
        Gizmos.DrawCube(serverPos, Vector3.one *1.2f);
    }
}
