using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using RenderHeads.Media.AVProVideo;
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
public class WordManager : Manager<WordManager> {

    public string BaseUrl = "https://api.datamuse.com/words?max=10&ml=";

    public GameObject WordPrefab;
    public RectTransform WordContainer;
    [Range(0, 3)]
    public float WordInterval = 1f;
    [Range(0, 3)]
    public float WordIntervalRandomness = 0.5f;
    float wordTime;

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
                //Debug.Log("WORDS FOR " + request.Tag + " (" + request.Word + "): " + string.Join(", ", bank.AvailableWords.ToList().Map(x => x.word)));
                //Debug.Log("NEXT FOR " + request.Tag + " (" + request.Word + "): " + bank.NextQueryWord);
            }
        }
        // new words
        wordTime -= Time.deltaTime;
        if (wordTime <= 0) {
            var playingPanel = VideoManager.Inst.Panels.RandomWhere(p => p.CurrentState == VideoState.Playing);
            if (playingPanel != null) {
                wordTime = WordInterval + Mathf.Lerp(-WordIntervalRandomness, WordIntervalRandomness, Random.value);
                var wordObj = Instantiate(WordPrefab);
                var word = wordObj.GetComponent<ScrollingWord>();
                word.TextMesh.text = GrabWordForTag(playingPanel.Link.tag) ?? "XXXXXXXXX";
                word.Container = WordContainer;
                word.transform.parent = WordContainer;
                word.transform.localScale = Vector3.one;
                word.RelativePos = word.RelativePos.withY(Random.value);
                word.RelativeVel = word.RelativeVel.withX(word.RelativeVel.x * Mathf.Lerp(0.8f, 1.25f, Random.value));
            }
        }
    }
}
