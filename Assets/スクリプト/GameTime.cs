using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 継承しないと機能しない
/// </summary>
public class GameTime : MonoBehaviour
{
    public static bool isGaming;
    public static WaitUntil WaitGame() => new(() => isGaming);
    public static float Time { private set; get; } = 0;
    protected virtual void Update()
    {
        if (isGaming)
        {
            Time += UnityEngine.Time.deltaTime;
        }
    }
    public static WaitUntil WaitGameTime(float seconds)
    {
        float startTime = Time;
        return new WaitUntil(() => Time >= startTime + seconds);
    }
}
