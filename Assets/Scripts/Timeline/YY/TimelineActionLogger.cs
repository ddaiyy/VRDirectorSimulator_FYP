using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineActionLogger : MonoBehaviour
{
    public List<TimelineCharacterController> allCharacters;

    public void PrintAllActionStates()
    {
        foreach (var c in allCharacters)
        {
            if (c.IsActionPlaying())
            {
                Debug.Log($"🧍‍♂️ {c.GetCharacterName()} 正在播放 {c.GetCurrentActionName()}，已持续 {Time.time - c.GetCurrentActionStartTime():F2}s");
            }
        }
    }
}

