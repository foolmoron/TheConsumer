using UnityEngine;
using System.Collections;
using System;
using RenderHeads.Media.AVProVideo;
using TMPro;

public class ScrollingWord : MonoBehaviour {

    public string Tag;

    public TextMeshProUGUI TextMesh;
    public Vector2 RelativePos = new Vector2(1.5f, 0.5f);
    public Vector2 RelativeVel = new Vector2(-0.1f, 0f);

    [Range(0, 5)]
    public int FrameInterval = 2;
    int frame;

    public RectTransform Container { get; set; }
    RectTransform rt;
    BoxCollider2D col;

    void Awake() {
        rt = GetComponent<RectTransform>();
        col = GetComponent<BoxCollider2D>();
    }

    void Start() {
    }

    public void SetText(string text) {
        TextMesh.text = text;
        TextMesh.ForceMeshUpdate();
        rt.sizeDelta = rt.sizeDelta.withX(TextMesh.bounds.size.x);
        TextMesh.ForceMeshUpdate();
        col.offset = TextMesh.bounds.center;
        col.size = TextMesh.bounds.size.to2() + new Vector2(20, 10);
    }

    public void SetTag(string tag) {
        Tag = tag;
        WordManager.Inst.ModsForTag[tag].Apply(this);
    }

    void Update() {
        // physics
        {
            RelativePos += RelativeVel * Time.deltaTime;
        }
        // position based on container dimensions, skipping frames for effect
        {
            frame = FrameInterval > 0 ? (frame + 1) % FrameInterval : 0;
            if (frame == 0) {
                rt.anchoredPosition = RelativePos * Container.rect.size;
            }
        }
        // die
        {
            if (RelativePos.x < 0) {
                if (VideoManager.Inst.Panels.Find(Tag, (p, t) => p.Link.tag == t)) {
                    GameManager.Inst.HitWrong();
                }
                Destroy(gameObject);
            }
        }
    }
}
