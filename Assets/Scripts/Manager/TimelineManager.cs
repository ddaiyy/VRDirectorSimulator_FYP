using System.Collections.Generic;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    public MasterTimelineTrack masterTrack;
    public List<TimelineTrack> tracks = new List<TimelineTrack>();

    void Awake()
    {
        Instance = this;
        // 自动收集场景中的所有TimelineTrack（不包括masterTrack本身）
        tracks.Clear();
        foreach (var track in FindObjectsOfType<TimelineTrack>())
        {
            if (track != masterTrack)
                tracks.Add(track);
        }
    }

    public void RegisterTrack(TimelineTrack track)
    {
        if (!tracks.Contains(track) && track != masterTrack)
            tracks.Add(track);
    }

    public void UnregisterTrack(TimelineTrack track)
    {
        if (tracks.Contains(track))
            tracks.Remove(track);
    }

    public List<TimelineTrack> GetAllTracks()
    {
        return tracks;
    }
}