using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;

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
    public List<string> UsedTags = new List<string>(50);

    public GameObject VideoPanelPrefab;
    GameObject nextVideoPanel;

    public GameObject PanelContainer;
    public List<VideoPanel> Panels = new List<VideoPanel>(32);

    void Awake() {
        Links = JsonUtility.FromJson<VideoLinks>(DataJSON.text).links;
        var existingPanels = PanelContainer.GetComponentsInChildren<VideoPanel>();
        existingPanels.ForEach(p => Destroy(p.gameObject));
        SetupNextVideo();
    }

    public VideoLink GetUnusedLink() {
        var link = Links.RandomWhere(UsedTags, (l, used) => !used.Contains(l.tag));
        UsedTags.Add(link.tag);
        return link;
    }

    void SetupNextVideo() {
        nextVideoPanel = Instantiate(VideoPanelPrefab);
        nextVideoPanel.transform.parent = transform;
        var vid = nextVideoPanel.GetComponent<VideoPanel>();
        vid.Index = Panels.Count;
        vid.WrongTag = GetUnusedLink().tag; // one wrong tag for each new video
    }

    public void StartNextVideo() {
        if (Panels.Count >= 32) {
            return;
        }

        if (nextVideoPanel == null) {
            SetupNextVideo();
        }

        var panel = nextVideoPanel;
        nextVideoPanel = null;
        SetupNextVideo();

        panel.transform.parent = PanelContainer.transform;
        panel.transform.SetAsFirstSibling();
        var vid = panel.GetComponent<VideoPanel>();
        vid.ShouldPlay = true;
        Panels.Add(vid);
    }

    public void Stop() {
        Panels.ForEach(p => Destroy(p.gameObject));
        Panels.Clear();
        UsedTags.Clear();
    }

    void Update() {
        // gridify vids
        {
            var rows = Mathf.FloorToInt(Mathf.Sqrt(Panels.Count) + 0.20f);
            var columns = Mathf.CeilToInt((float)Panels.Count / rows);
            if (Screen.height > Screen.width) {
                var swap = columns;
                columns = rows;
                rows = swap;
            }
            for (int r = 0; r < rows; r++) {
                var actualColumns = r == rows - 1 && Panels.Count % columns != 0 ? Panels.Count % columns : columns;
                for (int c = 0; c < actualColumns; c++) {
                    var panel = Panels[r * columns + c].GetComponent<RectTransform>();
                    panel.anchorMin = new Vector2((c + 0) * (1f / actualColumns), (r + 0) * (1f / rows));
                    panel.anchorMax = new Vector2((c + 1) * (1f / actualColumns), (r + 1) * (1f / rows));
                    panel.offsetMin = Vector2.zero;
                    panel.offsetMax = Vector2.zero;
                    panel.transform.localScale = Vector3.one;
                }
            }
        }
    }
}
