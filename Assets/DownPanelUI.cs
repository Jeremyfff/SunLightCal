using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DownPanelUI : MonoBehaviour
{
    [SerializeField] private RectTransform ProgressBarMask;

    [SerializeField] private ProgressBarMove[] progressBarMove;
    [SerializeField] private Text timeText;
    [SerializeField] private Text progressText;
    [SerializeField] private Text progressText2;
    [SerializeField] private Graph graphContainer1;
    [SerializeField] private Graph graphContainer2;
    [SerializeField] private TMP_Text SumText;
    private int counter;

    public static DownPanelUI instance;
    public static Graph graph1;
    public static Graph graph2;

    private void Awake() {
        DownPanelUI.instance = this;
        graph1 = graphContainer1;
        graph2 = graphContainer2;
    }
    // Start is called before the first frame update
    void Start()
    {
        SetTimeText(0, 0);
        SetProgress(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetProgress(float progress) {
        StopCoroutine("IE_SetProgress");
        StartCoroutine("IE_SetProgress", 1080 * progress);
        //ProgressBarMask.sizeDelta = new Vector2(1080 * progress, 40);
        var restTime = ComputeShaderTest.instance.GetRestTime(progress);
        if (progress < 0.05f) {
            progressText.enabled = false;
        } else {
            progressText.enabled = true;
            progressText.text =(int)(100f * progress) + "%";
        }
        if(progress > 0.2f) {
            progressText2.enabled = true;
            progressText2.text = "Ô¤¼ÆÊ£ÓàÊ±¼ä : " + (int)restTime+"(s)";
        } else {
            progressText2.enabled = false;
        }
        
    }

    IEnumerator IE_SetProgress(float target) {
        while (Mathf.Abs(ProgressBarMask.sizeDelta.x - target) > 1) {
            ProgressBarMask.sizeDelta += new Vector2((target - ProgressBarMask.sizeDelta.x) * 5 * UnityEngine.Time.deltaTime, 0);
            yield return null;
        }
        ProgressBarMask.sizeDelta = new Vector2(target, ProgressBarMask.sizeDelta.y);
    }
    public void CalStartEvent() {
        foreach(var p in progressBarMove) {
            p.StartMove();
        }
    }
    public void CalCompleteEvent() {
        foreach (var p in progressBarMove) {
            p.StopMove();
        }
        SetProgress(1.0f);
    }


    public void SetTimeText(int dayIndex,int minuteIndex) {
        timeText.text = "DayIndex:" + dayIndex + " | MinuteIndex: " + minuteIndex;
    }


    public void SetSumText(float value1,float value2) {
        SumText.text = "ValueSum/PVSum = " + (int)value1 + "/" + (int)value2;
    }
}
