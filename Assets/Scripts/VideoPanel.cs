﻿using UnityEngine;
using System.Collections;
using System;
using RenderHeads.Media.AVProVideo;
using UnityEngine.UI;

public enum VideoState { None, Loading, Ready, Waiting, Playing, Error }
public class VideoPanel : MonoBehaviour {

    [Range(0, 15)]
    public int Index;
    public VideoLink Link;
    public string WrongTag;
    public VideoState CurrentState = VideoState.None;
    public bool ShouldPlay;
    [Range(0, 10)]
    public float InitialTimeout = 3f;
    float timeout;
    float loadingTime;

    [Range(0, 0.15f)]
    public float FlashSpeed = 0.07f;
    [Range(0, 1)]
    public float FlashAlpha = 0.7f;

    public MediaPlayer Player;
    public DisplayUGUI Display;
    Image flash;

    void Awake() {
        flash = GetComponentInChildren<Image>();
        timeout = InitialTimeout;
        Player.Events.AddListener((mp, e, code) => {
            switch (e) {
                case MediaPlayerEvent.EventType.Closing:
                case MediaPlayerEvent.EventType.FinishedPlaying :
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
                    WordManager.Inst.CreateMaterialsForTagPair(Index, Link.tag, WrongTag);
                    CurrentState = VideoState.Playing;
                    break;
            }
        });
    }

    void Start() {
    }

    public void FlashColor(Color color) {
        flash.color = color.withAlpha(FlashAlpha);
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
        // color
        flash.color = flash.color.withAlpha(Mathf.Lerp(flash.color.a, 0, FlashSpeed));
    }
}
