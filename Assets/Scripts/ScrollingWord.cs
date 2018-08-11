using UnityEngine;
using System.Collections;
using System;
using RenderHeads.Media.AVProVideo;
using TMPro;

public class ScrollingWord : MonoBehaviour {

    public TextMeshProUGUI TextMesh;
    public Vector2 RelativePos = new Vector2(1.2f, 0.5f);
    public Vector2 RelativeVel = new Vector2(-0.1f, 0f);

    [Range(0, 5)]
    public int FrameInterval = 4;
    int frame;

    public RectTransform Container { get; set; }
    RectTransform rt;

    void Awake() {
        rt = GetComponent<RectTransform>();
    }

    void Start() {
    }

    void Update() {
        // physics
        {
            RelativePos += RelativeVel * Time.deltaTime;
        }
        // position based on container dimensions, skipping frames for effect
        {
            frame = (frame + 1) % FrameInterval;
            if (frame == 0) {
                rt.anchoredPosition = RelativePos * Container.rect.size;
            }
        }
        // die
        {
            if (RelativePos.x < 0) {
                Destroy(gameObject);
            }
        }
    }
}
