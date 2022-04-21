using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunManager : MonoBehaviour {
    public bool AutoUpdate = false;
    public float lon;
    public float lat;
    public int month;
    public int day;
    public int hour;
    public int minute;

    

    private Vector3 SunDir = new Vector3(0, 0, 0);
    private Time time;

    public static SunManager instance;

    private void Awake() {
        SunManager.instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        time = new Time(month, day, hour, minute);
    }

    // Update is called once per frame
    void Update() {
        if (AutoUpdate) {
            SetTimeAndUpdate(month, day, hour, minute);
        }
    }

    public Vector3 SetTimeAndUpdate(int month,int day,int hour,int minute) {
        this.month = month;
        this.day = day;
        this.hour = hour;
        this.minute = minute;
        time.Set(month, day, hour, minute);
        return UpdateSunDir();
    }

    /// <summary>
    /// ����̫������ ��������direction light�� ���ͬʱ����direction light �ķ���
    /// </summary>
    /// <returns></returns>
    private Vector3 UpdateSunDir() {
        // �����γ
        int dateIndex = time.ConvertToDayIndex();
        float b = 2f * Mathf.PI * dateIndex / 365f;
        float delta = 0.006918f - 0.399912f * Mathf.Cos(b) + 0.070257f * Mathf.Sin(b) - 0.006758f * Mathf.Cos(2f * b) + 0.000907f * Mathf.Sin(2f * b) - 0.002697f * Mathf.Cos(3f * b) + 0.00148f * Mathf.Sin(3f * b);
        // ��̫��ʱ
        Time timeShift = new Time(0, 0, 0, 0);

        Time four = new Time(0, 0, 0, 4);
        Time realTime = time.Add(four.Multiply(120f - lon).Reverse()).Add(timeShift);
        

        //̫��ʱ��t(degree)
        float t = (realTime.ConvertToHours() - 12f) * 15f;


        //ת��Ϊ����

        t = t / 180 * Mathf.PI;

        //̫���߶Ƚ�
        var sinHs = Mathf.Sin(lat) * Mathf.Sin(delta) + Mathf.Cos(lat) * Mathf.Cos(delta) * Mathf.Cos(t);
        var Hs = Mathf.Asin(sinHs);

        // ̫����λ��
        var cosAs = (sinHs * Mathf.Sin(lat) - Mathf.Sin(delta)) / (Mathf.Cos(Hs) * Mathf.Cos(lat));
        var As = Mathf.Acos(cosAs);
        if (t < 0) {
            As = -As;
        }

        
        transform.localRotation = Quaternion.Euler(Hs * 180f / Mathf.PI, As * 180f / Mathf.PI, 0);
        SunDir = transform.forward;

        return SunDir;

    }

    public Vector3 GetSunDir() {
        return this.SunDir;
    }


    public float GetSunHs() {
        // �����γ
        int dateIndex = time.ConvertToDayIndex();
        float b = 2f * Mathf.PI * dateIndex / 365f;
        float delta = 0.006918f - 0.399912f * Mathf.Cos(b) + 0.070257f * Mathf.Sin(b) - 0.006758f * Mathf.Cos(2f * b) + 0.000907f * Mathf.Sin(2f * b) - 0.002697f * Mathf.Cos(3f * b) + 0.00148f * Mathf.Sin(3f * b);


        //̫��ʱ��t(degree)
        float t = 0;


        //̫���߶Ƚ�
        var sinHs = Mathf.Sin(lat) * Mathf.Sin(delta) + Mathf.Cos(lat) * Mathf.Cos(delta) * Mathf.Cos(t);
        var Hs = Mathf.Asin(sinHs);
        return Hs;
    }
    override
    public string ToString() {
        return "Time:[month:" + this.month + ",day:" + this.day + ",hour:" + this.hour + ",minute:" + this.minute + "]";
    }

}
