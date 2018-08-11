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
    public List<VideoLink> UsedLinks = new List<VideoLink>(50);

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
        var link = Links.RandomWhere(UsedLinks, (l, used) => !used.Contains(l));
        UsedLinks.Add(link);
        return link;
    }

    void SetupNextVideo() {
        nextVideoPanel = Instantiate(VideoPanelPrefab);
        nextVideoPanel.GetComponent<VideoPanel>().ShouldPlay = true;
        nextVideoPanel.transform.parent = transform;
    }

    public void StartNextVideo() {
        if (nextVideoPanel == null) {
            SetupNextVideo();
        }

        var panel = nextVideoPanel;
        nextVideoPanel = null;
        SetupNextVideo();

        panel.transform.parent = PanelContainer.transform;
        panel.transform.SetAsFirstSibling();
        Panels.Add(panel.GetComponent<VideoPanel>());
    }

    public void Stop() {
        Panels.ForEach(p => Destroy(p.gameObject));
        Panels.Clear();
        UsedLinks.Clear();
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
