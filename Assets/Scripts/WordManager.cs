using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using RenderHeads.Media.AVProVideo;
using TMPro;
using Random = UnityEngine.Random;

[Serializable]
public class Word {
    public string word;
    public int score;
}
[Serializable]
public class Words {
    public Word[] words;
}
public struct Mod {
    public Material FontMaterial;
    public float CharSpacing;
    public int? FrameInterval;
    public bool Capitals;
    public bool Italics;

    public void Apply(ScrollingWord word) {
        word.TextMesh.fontSharedMaterial = FontMaterial;
        word.TextMesh.characterSpacing = CharSpacing;
        word.FrameInterval = FrameInterval ?? word.FrameInterval;
        var style = (FontStyles) 0;
        if (Capitals) style |= FontStyles.UpperCase;
        if (Italics) style |= FontStyles.Italic;
        word.TextMesh.fontStyle = style;

    }

    public static Mod GetForIndex(Material baseMaterial, int index) {
        var mod = new Mod { FontMaterial = new Material(baseMaterial) };
        for (int i = 0; i <= index; i++) {
            mod = mod.AddModForIndex(i, Random.value < 0.5f, false);
        }
        return mod;
    }
    public Mod AddModForIndex(int index, bool opposite, bool cloneMaterial = true) {
        if (cloneMaterial) {
            FontMaterial = new Material(FontMaterial);
        }
        switch (index % 12) {
            case 0:
                float h, s, v;
                var fc = FontMaterial.GetColor(ShaderUtilities.ID_FaceColor);
                Color.RGBToHSV(fc, out h, out s, out v);
                var newColor = Color.HSVToRGB((h + 0.4f * (Random.value - 0.5f) + 1f) % 1f, s * (opposite ? 0.5f : 1f), v);
                FontMaterial.SetColor(ShaderUtilities.ID_FaceColor, newColor);
                break;
            case 1:
                var outline = opposite 
                    ? Mathf.Lerp(0.11f, 0.15f, Random.value) 
                    : Mathf.Lerp(0.01f, 0.065f, Random.value)
                    ;
                FontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, outline);
                FontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outline);
                break;
            case 2:
                CharSpacing = opposite
                    ? Mathf.Lerp(1.5f, 2.8f, Random.value)
                    : Mathf.Lerp(4.2f, 5.7f, Random.value)
                    ;
                break;
            case 3:
                var thick = opposite
                    ? Mathf.Lerp(0.025f, 0.050f, Random.value)
                    : Mathf.Lerp(0.080f, 0.120f, Random.value)
                    ;
                FontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, FontMaterial.GetFloat(ShaderUtilities.ID_OutlineWidth) + thick);
                break;
            case 4:
                var oc = Color.HSVToRGB((0.66f + 0.4f * (Random.value - 0.5f) + 1f) % 1f, 1f, 1f);
                oc.a = opposite ? 1f : 0.5f;
                FontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, oc);
                break;
            case 5:
                FrameInterval = opposite
                    ? Mathf.RoundToInt(Mathf.Lerp(0, 1, Random.value))
                    : Mathf.RoundToInt(Mathf.Lerp(12, 18, Random.value))
                    ;
                break;
            case 6:
                Capitals = opposite;
                break;
            case 7:
                Italics = opposite;
                break;
            case 8:
                var shadowX = Mathf.Lerp(0.06f, 0.16f, Random.value);
                var shadowY = Mathf.Lerp(-0.1f, -0.2f, Random.value);
                shadowX += FontMaterial.GetFloat(ShaderUtilities.ID_FaceDilate);
                shadowY -= FontMaterial.GetFloat(ShaderUtilities.ID_FaceDilate);
                if (opposite) {
                    shadowX *= -1;
                    shadowY *= -1;
                }
                FontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, shadowX);
                FontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, shadowY);
                break;
            case 9:
                var sc = Color.HSVToRGB((0.16f + 0.4f * (Random.value - 0.5f) + 1f) % 1f, opposite ? 0.6f : 1f, 1f).withAlpha(FontMaterial.GetColor(ShaderUtilities.ID_UnderlayColor).a);
                FontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, sc);
                break;
            case 10:
                var tsoft = opposite
                    ? Mathf.Lerp(0.070f, 0.120f, Random.value)
                    : Mathf.Lerp(0.250f, 0.500f, Random.value)
                    ;
                FontMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, tsoft);
                break;
            case 11:
                var ssoft = opposite
                        ? Mathf.Lerp(0.050f, 0.090f, Random.value)
                        : Mathf.Lerp(0.200f, 0.350f, Random.value)
                    ;
                FontMaterial.SetFloat(ShaderUtilities.ID_UnderlaySoftness, ssoft);
                break;
        }
        return this;
    }
}
public class WordManager : Manager<WordManager> {

    public string BaseUrl = "https://api.datamuse.com/words?max=10&ml=";

    public GameObject WordPrefab;
    public RectTransform WordContainer;
    [Range(0, 3)]
    public float CorrectWordInterval = 1.5f;
    float correctWordTime;
    [Range(0, 3)]
    public float WrongWordInterval = 0.9f;
    float wrongWordTime;

    public Dictionary<string, Mod> ModsForTag = new Dictionary<string, Mod>(32);

    public class WordBank {
        public Queue<Word> AvailableWords = new Queue<Word>(20);
        public string NextQueryWord;

        public string GetWord() {
            return AvailableWords.Count > 0 ? AvailableWords.Dequeue().word : null;
        }
    }
    public readonly Dictionary<string, WordBank> WordsForTag = new Dictionary<string, WordBank>(30);

    public class WordRequest {
        public string Tag;
        public string Word;
        public WWW Request;

        public WordRequest(string baseUrl, string tag, string word) {
            Tag = tag;
            Word = word;
            Request = new WWW(baseUrl + WWW.EscapeURL(word));
        }
    }
    readonly List<WordRequest> requests = new List<WordRequest>(10);

    void Awake() {
        for (int i = 0; i < WordContainer.childCount; i++) {
            Destroy(WordContainer.GetChild(i).gameObject);
        }
    }

    public string GrabWordForTag(string tag) {
        var bank = WordsForTag.Get(tag);
        var word = bank?.GetWord();
        // new request when running low
        if (word != null && bank.AvailableWords.Count <= 3 && bank.NextQueryWord != null) {
            requests.Add(new WordRequest(BaseUrl, tag, bank.NextQueryWord));
            bank.NextQueryWord = null;
        }
        return word;
    }

    public void SpawnWord(string tag, string wordText) {
        if (wordText != null) {
            wrongWordTime = WrongWordInterval * Mathf.Lerp(0.8f, 1.25f, Random.value);
            var wordObj = Instantiate(WordPrefab);
            var word = wordObj.GetComponent<ScrollingWord>();
            word.Container = WordContainer;
            word.transform.parent = WordContainer;
            word.transform.localScale = Vector3.one;
            word.RelativePos = word.RelativePos.withY(Random.value); //TODO(momin): use lane system
            word.RelativeVel = word.RelativeVel.withX(word.RelativeVel.x * Mathf.Lerp(0.8f, 1.25f, Random.value));
            word.SetTag(tag);
            word.SetText(wordText);
        } else {
            var panel = VideoManager.Inst.Panels.Find(tag, (p, t) => p.tag == t);
            if (panel) {
                Debug.LogError("NO WORDS: " + panel.Link.tag + " || " + panel.Link.title);
                panel.CurrentState = VideoState.Error;
            }
        }
    }

    public void CreateMaterialsForTagPair(int panelIndex, string correctTag, string wrongTag) {
        //TODO: mutations
        var baseMod = Mod.GetForIndex(WordPrefab.GetComponent<TextMeshProUGUI>().fontSharedMaterial, panelIndex - 1);
        var opposite = Random.value < 0.5f;
        ModsForTag[correctTag] = baseMod.AddModForIndex(panelIndex, opposite);
        ModsForTag[wrongTag] = baseMod.AddModForIndex(panelIndex, !opposite);
    }

    void Update() {
        // new requests
        foreach (var tag in VideoManager.Inst.UsedTags) {
            if (!WordsForTag.ContainsKey(tag)) {
                WordsForTag[tag] = new WordBank();
                requests.Add(new WordRequest(BaseUrl, tag, tag));
            }
        }
        // check request
        for (var i = 0; i < requests.Count; i++) {
            var request = requests[i];
            if (request.Request.isDone) {
                requests.RemoveAt(i);
                i--;

                var words = JsonUtility.FromJson<Words>("{\"words\":"+request.Request.text+"}").words;
                var bank = WordsForTag[request.Tag];
                bank.NextQueryWord = words.Length > 0 ? words.Random().word : null;
                foreach (var word in words) {
                    bank.AvailableWords.Enqueue(word);
                }
                Debug.Log("WORDS FOR " + request.Tag + " (" + request.Word + "): " + string.Join(", ", bank.AvailableWords.ToList().Map(x => x.word)));
                Debug.Log("NEXT FOR " + request.Tag + " (" + request.Word + "): " + bank.NextQueryWord);
            }
        }
        // new words
        correctWordTime -= Time.deltaTime;
        if (correctWordTime <= 0) {
            var playingPanel = VideoManager.Inst.Panels.RandomWhere(p => p.CurrentState == VideoState.Playing);
            if (playingPanel != null) {
                var correctWord = GrabWordForTag(playingPanel.Link.tag);
                SpawnWord(playingPanel.Link.tag, correctWord);
                correctWordTime = CorrectWordInterval * Mathf.Lerp(0.8f, 1.25f, Random.value);
            }
        }
        wrongWordTime -= Time.deltaTime;
        if (wrongWordTime <= 0) {
            var playingPanel = VideoManager.Inst.Panels.RandomWhere(p => p.CurrentState == VideoState.Playing);
            if (playingPanel != null) {
                var wrongWord = GrabWordForTag(playingPanel.WrongTag);
                SpawnWord(playingPanel.WrongTag, wrongWord);
                wrongWordTime = WrongWordInterval * Mathf.Lerp(0.8f, 1.25f, Random.value);
            }
        }
    }
}
