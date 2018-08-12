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

    new Camera camera;
    
    void Awake() {
        camera = Camera.main;
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            var wPos = camera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(wPos.withZ(-10), Vector3.forward, 100);
            if (hit.collider) {
                Destroy(hit.collider.gameObject);
                HitTag(hit.collider.GetComponent<ScrollingWord>().Tag);
            }
        }
    }

    //TODO(momin): combo system/momentum, losing points with more extreme momentum
    public void HitTag(string tag) {
        if (VideoManager.Inst.Panels.Find(tag, (p, t) => p.Link.tag == t)) {
            HitCorrect();
        } else {
            HitWrong();
        }
    }
    public void HitCorrect() {
        Score += Math.Floor(Math.Pow(BaseScore, 1 + VideoManager.Inst.Panels.Count * Exponent)) * 10;
    }
    public void HitWrong() {
        Score -= Math.Floor(Score / 20) * 10;
    }
}
