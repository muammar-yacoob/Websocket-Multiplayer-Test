using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] List<Color> playerColors;
    [SerializeField] Transform playerPrefab;
    private void Awake() => SpawnPlayers(1, 1, 1.2f);

    private void ColorPlayer(Transform player)
    {
        var mat = player.GetComponentInChildren<MeshRenderer>().material;
        var randIndex = Random.Range(0, playerColors.Count - 1);
        var c = playerColors[randIndex];
        mat.color = c;
    }

    internal void SpawnPlayers(int rows, int cols, float padding)
    {
        var parentTrans = FindObjectOfType<SyncManager>().transform;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var pos = new Vector3(i * padding, 0, j * padding);
                var player = Instantiate(playerPrefab, pos, Quaternion.identity);
                ColorPlayer(player);
                player.SetParent(parentTrans);
                SyncManager.AddPlayer(player);
            }
        }
    }
}
