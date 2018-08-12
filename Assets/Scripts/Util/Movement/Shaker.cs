using UnityEngine;
using System.Collections;

public class Shaker : MonoBehaviour {

    public bool Shaking = false;
    [Range(0, 1)]
    public float Strength = 0.05f;
    [Range(0, 20)]
    public float StrengthMult = 1f;
    [Range(1, 10)]
    public int FrameInterval = 1;

	bool frameShaking = true;
	int framesToShake;

    int frameCount;
    Vector3 previousShake;

    void Awake() {
        if (Shaking) {
            frameShaking = false;
        }
    }

    void Update() {
        if (!frameShaking && Time.deltaTime == 0) // still run update if shaking by frames
            return;

        if (Shaking) {
            frameCount++;
            if (frameCount % FrameInterval == 0) {
                Shake();
            }
			if (frameShaking && frameCount >= framesToShake) {
				frameCount = 0;
				Shaking = false;
			}
        } else {
			frameShaking = false;
			if (previousShake != Vector3.zero) {
	            transform.localPosition = transform.localPosition - previousShake;
	            previousShake = Vector3.zero;
			}
        }
    }

    void Shake() {
        transform.localPosition -= previousShake;
        Vector3 shake = Random.insideUnitCircle.normalized * Strength * StrengthMult;
        transform.localPosition += shake;
        previousShake = shake;
    }

	public void ShakeForFrames(int numFrames) {
		if (numFrames <= 0) {
			return;
		}
		framesToShake = numFrames;
		frameShaking = true;
		Shaking = true;
	}
}
