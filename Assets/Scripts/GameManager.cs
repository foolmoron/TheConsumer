using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : Manager<GameManager> {

    public double Score;
    public int HighestPanels;
    public float HighestPanelsTime;
    public double HighScore;

    [Range(0, 20)]
    public float BaseScore = 4;
    [Range(0, 2)]
    public float Exponent = 0.5f;

    public GameObject StaticPrefab;
    [Range(0, 5)]
    public float StaticTime = 0.8f;

    public Color CorrectColor = Color.green;
    public Color WrongColor = Color.red;

    public GameObject StaticEndPrefab;
    RectTransform staticEnd;
    float timeToKillStaticEnd;
    public bool Paused;

    public GameObject ScoreContainer;
    public TextMeshProUGUI AllScoresText;
    public TextMeshProUGUI OpinionText;

    new Camera camera;
    
    void Awake() {
        camera = Camera.main;
        ScoreContainer.SetActive(false);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            var wPos = camera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(wPos.withZ(-10), Vector3.forward, 100);
            if (hit.collider) {
                var vid = HitTag(hit.collider.GetComponent<ScrollingWord>().Tag);
                if (vid) {
                    vid.FlashColor(CorrectColor);
                    Destroy(hit.collider.GetComponent<ScrollingWord>());
                    var gtv = hit.collider.gameObject.AddComponent<GoToVideo>();
                    gtv.Target = vid;
                } else {
                    VideoManager.Inst.Panels.Find(hit.collider.GetComponent<ScrollingWord>().Tag, (p, t) => p.WrongTag == t).FlashColor(WrongColor);
                    Destroy(hit.collider.gameObject);
                    SpawnStatic(hit.collider.GetComponent<RectTransform>());
                }
            }
        }
        // panel score
        {
            if (VideoManager.Inst.Panels.Count > HighestPanels) {
                ScoreContainer.SetActive(false);
                HighestPanels = VideoManager.Inst.Panels.Count;
                HighestPanelsTime = 0;
            }
            HighestPanelsTime += Time.deltaTime;
        }
        // inflation
        {
            //TODO: compound inflation based on # of panels
        }
        // end
        {
            if (!Paused && timeToKillStaticEnd > 0 && staticEnd) {
                timeToKillStaticEnd -= Time.deltaTime;
                if (timeToKillStaticEnd <= 0) {
                    Destroy(staticEnd.gameObject);
                }
            }
        }
    }

    public void SpawnStatic(RectTransform rt) {
        var stat = Instantiate(StaticPrefab, rt.parent);
        var statT = stat.GetComponent<RectTransform>();
        statT.anchorMin = rt.anchorMin;
        statT.anchorMax = rt.anchorMax;
        statT.anchoredPosition = rt.anchoredPosition;
        statT.sizeDelta = rt.sizeDelta;
        Destroy(stat.gameObject, StaticTime);
    }

    //TODO(momin): combo system/momentum, losing points with more extreme momentum, lose flat # of points based on panels, game over on negative
    public VideoPanel HitTag(string tag) {
        var vid = VideoManager.Inst.Panels.Find(tag, (p, t) => p.Link.tag == t);
        if (vid) {
            HitCorrect();
        } else {
            HitWrong();
        }
        return vid;
    }
    public void HitCorrect() {
        Score += Math.Floor(Math.Pow(BaseScore, 1 + VideoManager.Inst.Panels.Count * Exponent));
    }
    public void HitWrong() {
        Score -= Math.Floor(Score / 2);
    }

    public void Stop() {
        if (VideoManager.Inst.Panels.Count < 1) {
            return;
        }
        // high score
        var score = (long)Score;
        long prevHigh;
        prevHigh = long.TryParse(PlayerPrefs.GetString("high"), out prevHigh) ? prevHigh : 0;
        PlayerPrefs.SetString("high", Math.Max(score, prevHigh).ToString());
        // high panels
        var scaledTime = HighestPanels * HighestPanelsTime;
        var prevPanels = PlayerPrefs.GetInt("panels");
        var prevTime = PlayerPrefs.GetFloat("time");
        var prevScaledTime = prevPanels * prevTime;
        if (scaledTime > prevScaledTime) {
            PlayerPrefs.SetInt("panels", HighestPanels);
            PlayerPrefs.SetFloat("time", HighestPanelsTime);
        }
        // display score
        ScoreContainer.SetActive(true);
        AllScoresText.text = 
            $"You gained {score} awareness\n" +
            $"You consumed {HighestPanels} panel{(HighestPanels == 1 ? "" : "s")} for {HighestPanelsTime:0.00} seconds\n\n" +
            $"Best awareness: {PlayerPrefs.GetString("high")}\n" +
            $"Best consumption: {PlayerPrefs.GetInt("panels")}p x {PlayerPrefs.GetFloat("time"):0.00}s"
            ;
        OpinionText.text = "Hey";
        // static
        staticEnd = Instantiate(StaticEndPrefab, FindObjectOfType<Canvas>().transform).GetComponent<RectTransform>();
        staticEnd.anchorMin = Vector2.zero;
        staticEnd.anchorMax = Vector2.one;
        staticEnd.anchoredPosition = Vector2.zero;
        staticEnd.sizeDelta = Vector2.zero;
        timeToKillStaticEnd = 1.5f;
        // reset
        Score = 0;
        HighestPanels = 0;
        HighestPanelsTime = 0;
        FindObjectsOfType<ScrollingWord>().ForEach(s => Destroy(s.gameObject));
        FindObjectsOfType<GoToVideo>().ForEach(v => Destroy(v.gameObject));
        VideoManager.Inst.Panels.ForEach(p => Destroy(p.gameObject));
        VideoManager.Inst.Panels.Clear();
        VideoManager.Inst.UsedTags.Clear();
    }

#if !DEBUG
    private void OnApplicationPause(bool pause) {
        if (pause && VideoManager.Inst.Panels.Count > 0) {
            Stop();
            timeToKillStaticEnd = 0.6f;
        }
        Paused = pause;
    }
#endif

}
