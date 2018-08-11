using UnityEngine;
using System.Collections;
using System;
using RenderHeads.Media.AVProVideo;

public enum VideoState { None, Loading, Ready, Waiting, Playing, Error }
public class VideoPanel : MonoBehaviour {

    public VideoLink Link;
    public VideoState CurrentState = VideoState.None;
    public bool ShouldPlay;
    [Range(0, 10)]
    public float InitialTimeout = 3f;
    float timeout;
    float loadingTime;

    public MediaPlayer Player;

    void Awake() {
        timeout = InitialTimeout;
        Player.Events.AddListener((mp, e, code) => {
            switch (e) {
                case MediaPlayerEvent.EventType.Closing:
                    CurrentState = VideoState.None;
                    break;
                case MediaPlayerEvent.EventType.Error:
                    Debug.LogError("ERROR VID: " + Link.tag + " || " + Link.title);
                    CurrentState = VideoState.Error;
                    break;
                case MediaPlayerEvent.EventType.ReadyToPlay:
                    CurrentState = VideoState.Ready;
                    break;
                case MediaPlayerEvent.EventType.Started:
                    CurrentState = VideoState.Playing;
                    break;
            }
        });
    }

    void Start() {
    }

    void Update() {
        // loading timeout
        if (ShouldPlay && CurrentState == VideoState.Loading) {
            loadingTime += Time.deltaTime;
            if (loadingTime >= timeout) {
                Debug.LogError("TIMEOUT " + timeout + " VID: " + Link.tag + " || " + Link.title);
                CurrentState = VideoState.Error;
                timeout *= 2;
            }
        }
        if (!ShouldPlay || CurrentState != VideoState.Loading) {
            loadingTime = 0;
            timeout = InitialTimeout;
        }
        // ready to waiting
        if (ShouldPlay && CurrentState == VideoState.Ready) {
            CurrentState = VideoState.Waiting;
        }
        // load or play
        switch (CurrentState) {
            case VideoState.None:
            case VideoState.Error:
                Link = VideoManager.Inst.GetUnusedLink();
                Player.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, Link.vid, false);
                CurrentState = VideoState.Loading;
                break;
            case VideoState.Waiting:
                Player.Play();
                break;
        }
    }
}
