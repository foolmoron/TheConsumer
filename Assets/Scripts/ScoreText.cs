using UnityEngine;
using System.Collections;
using System;
using RenderHeads.Media.AVProVideo;
using TMPro;

public class ScoreText : MonoBehaviour {

    TextMeshProUGUI tm;
    
    void Awake() {
        tm = GetComponent<TextMeshProUGUI>();
    }

    void Start() {
    }

    void Update() {
        tm.text = ((long)GameManager.Inst.Score).ToString();
    }
}
