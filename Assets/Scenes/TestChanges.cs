using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestChanges : MonoBehaviour
{

    List<int> currentValue = new List<int>(1);
    List<int> lastValue = new List<int>(1);

    int ctr = 0;

    [ExecuteInEditMode]
    [ContextMenu("Move")]
    public void Move()
    {
        if (currentValue.Count < 1) currentValue.Add(1);
        if (lastValue.Count < 1) lastValue = new List<int>(currentValue);

        currentValue[0] = ctr++;

        Debug.Log(lastValue[0]+"-"+ currentValue[0]);
        lastValue = new List<int>(currentValue);
    }
}
