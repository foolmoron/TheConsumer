using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Manager<GameManager> {

    public double Score;
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

    new Camera camera;
    
    void Awake() {
        camera = Camera.main;
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

    //TODO(momin): combo system/momentum, losing points with more extreme momentum
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
        Score += Math.Floor(Math.Pow(BaseScore, 1 + VideoManager.Inst.Panels.Count * Exponent)) * 10;
    }
    public void HitWrong() {
        Score -= Math.Floor(Score / 20) * 10;
    }
}
