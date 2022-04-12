using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] Transform playerPrefab;
    [SerializeField] int countX =2, countY = 2;
    [SerializeField] List<Color> playerColors;
    float padding = 2f;
    private void Awake() => SpawnPlayers(countX, countY, padding);

    private void ColorPlayer(Transform player)
    {
        var mat = player.GetComponentInChildren<MeshRenderer>().material;
        var randIndex = Random.Range(0, playerColors.Count - 1);
        var c = playerColors[randIndex];
        mat.color = c;
    }

    internal void SpawnPlayers(int rows, int cols, float padding)
    {
        var parentTrans = transform;
        var initialY = 2;
        int count = 1;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var pos = new Vector3(i * padding, initialY, j * padding);
                var player = Instantiate(playerPrefab, pos, Quaternion.identity);
                ColorPlayer(player);
                player.SetParent(parentTrans);
                player.name = $"Player-{count++}";
                SyncManager.AddPlayer(player);
            }
        }
    }
}
