using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class VideoLink {
    public string tag;
    public string title;
    public string vid;
}
[Serializable]
public class VideoLinks {
    public VideoLink[] links;
}
public class VideoManager : Manager<VideoManager> {

    public TextAsset DataJSON;
    public VideoLink[] Links;

    void Awake() {
        Links = JsonUtility.FromJson<VideoLinks>(DataJSON.text).links;
    }
}
