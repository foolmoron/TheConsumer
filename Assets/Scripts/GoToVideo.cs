using UnityEngine;
using System.Collections;
using System;
using RenderHeads.Media.AVProVideo;
using TMPro;

public class GoToVideo : MonoBehaviour {

    public VideoPanel Target;
    public Vector2 DesiredRelPos = new Vector2(0.5f, 0.125f);
    [Range(0, 0.1f)]
    public float ScaleSpeed = 0.02f;
    [Range(0, 0.1f)]
    public float PosSpeed = 0.07f;
    [Range(0, 10)]
    public float Life = 4f;

    RectTransform rt;
    
    void Awake() {
        rt = GetComponent<RectTransform>();
    }

    void Start() {
        Destroy(gameObject, Life);
    }

    void Update() {
        var trt = Target.GetComponent<RectTransform>();
        var desiredPos = DesiredRelPos * trt.rect.size;
        var worldPos = trt.TransformPoint(desiredPos);
        var objPos = rt.InverseTransformPoint(worldPos);
        if (!float.IsInfinity(objPos.x) && !float.IsInfinity(objPos.y)) {
            rt.localPosition = Vector3.Lerp(rt.localPosition, objPos, PosSpeed).withZ(0);
        }
        rt.localScale = Vector3.Lerp(rt.localScale, Vector3.zero, ScaleSpeed);
    }
}
