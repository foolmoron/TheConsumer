using UnityEngine;
using System.Collections;
using System;
using RenderHeads.Media.AVProVideo;

public class VideoPanel : MonoBehaviour {

    public VideoLink Link;
    public MediaPlayerEvent.EventType CurrentState = MediaPlayerEvent.EventType.Closing;

    MediaPlayer player;

    void Awake() {
        player = GetComponent<MediaPlayer>();
        player.Events.AddListener((mp, e, code) => {
            switch (e) {
                case MediaPlayerEvent.EventType.Closing:
                    CurrentState = MediaPlayerEvent.EventType.Closing;
                    break;
                case MediaPlayerEvent.EventType.Error:
                    Debug.LogError("Broken vid: " + Link.tag + " " + Link.title);
                    CurrentState = MediaPlayerEvent.EventType.Error;
                    break;
                case MediaPlayerEvent.EventType.Started:
                    CurrentState = MediaPlayerEvent.EventType.Started;
                    break;
            }
        });
    }

    void Start() {
        Link = VideoManager.Inst.Links.Random();
        player.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, Link.vid);
    }
}
