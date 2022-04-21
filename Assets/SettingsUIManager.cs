using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsUIManager : MonoBehaviour
{
    [SerializeField] TMP_InputField DayRangeStart;
    [SerializeField] TMP_InputField DayRangeEnd;
    [SerializeField] TMP_InputField DayStep;
    [SerializeField] TMP_InputField TimeRangeStart;
    [SerializeField] TMP_InputField TimeRangeEnd;
    [SerializeField] TMP_InputField TimeStep;
    [SerializeField] Toggle SaveAfterComplete;
    [SerializeField] TMP_InputField FilePath;
    [SerializeField] Toggle ShaderAutoUpdate;
    [SerializeField] Toggle Preview;
    [SerializeField] TMP_InputField Size;
    [SerializeField] TMP_InputField MSAA;
    [SerializeField] Button Apply;
    [SerializeField] TMP_InputField EmitRange;
    [SerializeField] TMP_InputField EmitDistance;
    [SerializeField] Toggle SunAutoUpdate;
    [SerializeField] TMP_InputField Lon;
    [SerializeField] TMP_InputField Lat;
    [SerializeField] TMP_InputField Month;
    [SerializeField] TMP_InputField Day;
    [SerializeField] Slider DayIndex;
    [SerializeField] TMP_InputField Hour;
    [SerializeField] TMP_InputField Minute;
    [SerializeField] Slider MinuteIndex;
    [SerializeField] TMP_InputField GlobalShadeAngle;
    [SerializeField] TMP_InputField GlobalShadeLengthMin;
    [SerializeField] TMP_InputField GlobalShadeLengthMax;
    [SerializeField] TMP_InputField GlobalShadeWidth;
    [SerializeField] TMP_InputField ShadeNumber;
    [SerializeField] TMP_InputField ShadeAngleRangeMin;
    [SerializeField] TMP_InputField ShadeAngleRangeMax;
    [SerializeField] TMP_InputField GlobalShadeAngleRangeMin;
    [SerializeField] TMP_InputField GlobalShadeAngleRangeMax;
    [SerializeField] TMP_InputField ShadeLength;
    [SerializeField] TMP_Dropdown PVMode;
    [SerializeField] TMP_Dropdown ModelMode;
    [SerializeField] TMP_InputField InnerSub;
    [SerializeField] TMP_InputField InnerLoopTime;
    [SerializeField] TMP_Text InnerTip;
    [SerializeField] TMP_InputField OuterSub;
    [SerializeField] TMP_InputField OuterLoopTime;
    [SerializeField] TMP_Text OuterTip;
    [SerializeField] Toggle UseWiderRange;
    [SerializeField] RectTransform ExpertModePanel;
    [SerializeField] RectTransform ExpertModeLogo;
    [SerializeField] TMP_InputField PVYOffset;
    [SerializeField] TMP_InputField PVZOffset;
    [SerializeField] TMP_InputField PVRotation;
    [SerializeField] TMP_InputField PVHeight;
    [SerializeField] TMP_InputField ShadeZOffset;

    private ComputeShaderTest main;
    private ShadeControl shadeControl;
    private SunManager sunManager;
    private GameInfo gameInfo;
    public static SettingsUIManager instance;
    private void Awake() {
        SettingsUIManager.instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine("_init");
        
        

    }

    IEnumerator _init() {
        main = ComputeShaderTest.instance;
        

        gameInfo = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameInfo>();
        shadeControl = GameObject.FindGameObjectWithTag("ShadeControl").GetComponent<ShadeControl>();
        sunManager = SunManager.instance;

        try {
            ReadAll();
        } catch {

        }

        yield return new WaitUntil(() => main.complete == true);

        shadeControl = GameObject.FindGameObjectWithTag("ShadeControl").GetComponent<ShadeControl>();
        ReadAll();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable() {
        SaveAll();
    }
    private void SaveAll() {
        PlayerPrefs.Save();
    }
    public void ReadAll() {
        DayRangeStart.text = PlayerPrefs.GetInt("DayRangeStart").ToString();
        DayRangeEnd.text = PlayerPrefs.GetInt("DayRangeEnd").ToString();
        DayStep.text = PlayerPrefs.GetInt("DayStep").ToString();
        TimeRangeStart.text = PlayerPrefs.GetInt("TimeRangeStart").ToString();
        TimeRangeEnd.text = PlayerPrefs.GetInt("TimeRangeEnd").ToString();
        TimeStep.text = PlayerPrefs.GetInt("TimeStep").ToString();
        SaveAfterComplete.isOn = ReadBool("SaveAfterComplete");
        //FilePath.text = PlayerPrefs.GetString("FilePath").ToString();
        ShaderAutoUpdate.isOn = ReadBool("ShaderAutoUpdate");
        Preview.isOn = ReadBool("Preview");
        Size.text = PlayerPrefs.GetInt("Size").ToString();
        MSAA.text = PlayerPrefs.GetInt("MSAA").ToString();
        EmitRange.text = PlayerPrefs.GetFloat("EmitRange").ToString();
        EmitDistance.text = PlayerPrefs.GetFloat("EmitDistance").ToString();
        SunAutoUpdate.isOn = ReadBool("SunAutoUpdate");
        Lon.text = PlayerPrefs.GetFloat("Lon").ToString();
        Lat.text = PlayerPrefs.GetFloat("Lat").ToString();
        Month.text = PlayerPrefs.GetInt("Month").ToString();
        Day.text = PlayerPrefs.GetInt("Day").ToString();
        Hour.text = PlayerPrefs.GetInt("Hour").ToString();
        Minute.text = PlayerPrefs.GetInt("Minute").ToString();
        GlobalShadeAngle.text = PlayerPrefs.GetFloat("GlobalShadeAngle").ToString();
        GlobalShadeLengthMin.text = PlayerPrefs.GetFloat("GlobalShadeLengthMin").ToString();
        GlobalShadeLengthMax.text = PlayerPrefs.GetFloat("GlobalShadeLengthMax").ToString();
        GlobalShadeWidth.text = PlayerPrefs.GetFloat("GlobalShadeWidth").ToString();
        ShadeNumber.text = PlayerPrefs.GetInt("ShadeNumber").ToString();
        ShadeAngleRangeMin.text = PlayerPrefs.GetFloat("ShadeAngleRangeMin").ToString();
        ShadeAngleRangeMax.text = PlayerPrefs.GetFloat("ShadeAngleRangeMax").ToString();
        GlobalShadeAngleRangeMin.text = PlayerPrefs.GetFloat("GlobalShadeAngleRangeMin").ToString();
        GlobalShadeAngleRangeMax.text = PlayerPrefs.GetFloat("GlobalShadeAngleRangeMax").ToString();
        ShadeLength.text = PlayerPrefs.GetFloat("ShadeLength").ToString();
        PVMode.value = gameInfo.currentPVMode;
        ModelMode.value = gameInfo.currentModelMode;

        InnerSub.text = PlayerPrefs.GetInt("InnerSub").ToString();
        InnerLoopTime.text = PlayerPrefs.GetInt("InnerLoopTime").ToString();
        OuterSub.text = PlayerPrefs.GetInt("OuterSub").ToString();
        OuterLoopTime.text = PlayerPrefs.GetInt("OuterLoopTime").ToString();
        UseWiderRange.isOn = ReadBool("UseWiderRange");
        UpdateToolTip();

        PVYOffset.text = PlayerPrefs.GetFloat("PVYOffset").ToString();
        PVZOffset.text = PlayerPrefs.GetFloat("PVZOffset").ToString();
        PVRotation.text = PlayerPrefs.GetFloat("PVRotation").ToString();
        PVHeight.text = PlayerPrefs.GetFloat("PVHeight").ToString();
        ShadeZOffset.text = PlayerPrefs.GetFloat("ShadeZOffset").ToString();


        //set parameters

        main.startDayIndex = int.Parse(DayRangeStart.text);
        main.endDayIndex = int.Parse(DayRangeEnd.text);
        main.DayStep = int.Parse(DayStep.text);
        main.startMinuteIndex = int.Parse(TimeRangeStart.text);
        main.endMinuteIndex = int.Parse(TimeRangeEnd.text);
        main.MinuteStep = int.Parse(TimeStep.text);
        main.SaveAfterCal = SaveAfterComplete.isOn;
        //main.FileSavePath = FilePath.text;
        main.AutoUpdate = ShaderAutoUpdate.isOn;
        main.Preview = Preview.isOn;
        main.size = int.Parse(Size.text);
        Debug.Log("main.size1"+main.size);
        main._multiSample = int.Parse(MSAA.text);
        main._emitRange = float.Parse(EmitRange.text);
        main._emitDistance = float.Parse(EmitDistance.text);
        sunManager.AutoUpdate = SunAutoUpdate.isOn;
        sunManager.lon = float.Parse(Lon.text);
        sunManager.lat = float.Parse(Lat.text);
        sunManager.month = int.Parse(Month.text);
        sunManager.day = int.Parse(Day.text);
        sunManager.hour = int.Parse(Hour.text);
        sunManager.minute = int.Parse(Minute.text);
        shadeControl.GlobalShadeAngle = float.Parse(GlobalShadeAngle.text);
        shadeControl.GlobalShadeLengthMin = float.Parse(GlobalShadeLengthMin.text);
        shadeControl.GlobalShadeLengthMax = float.Parse(GlobalShadeLengthMax.text);
        shadeControl.GlobalShadeWidth = float.Parse(GlobalShadeWidth.text);
        shadeControl.ShadeNum = int.Parse(ShadeNumber.text);
        shadeControl.Min = float.Parse(ShadeAngleRangeMin.text);
        shadeControl.Max = float.Parse(ShadeAngleRangeMax.text);
        shadeControl.Min2 = float.Parse(GlobalShadeAngleRangeMin.text);
        shadeControl.Max2 = float.Parse(GlobalShadeAngleRangeMax.text);
        shadeControl.ShadeLength = float.Parse(ShadeLength.text);

        main._innerSub = int.Parse(InnerSub.text);
        main._innerLoopTime = int.Parse(InnerLoopTime.text);
        main._outterSub = int.Parse(OuterSub.text);
        main._outterLoopTime = int.Parse(OuterLoopTime.text);
        main._useWiderRange = UseWiderRange.isOn;

        shadeControl.PVYOffset = float.Parse(PVYOffset.text);
        shadeControl.PVZOffset = float.Parse(PVZOffset.text);
        shadeControl.PVRotation = float.Parse(PVRotation.text);
        shadeControl.PVHeight = float.Parse(PVHeight.text);
        shadeControl.ShadeZOffset = float.Parse(ShadeZOffset.text);

    }
    private void SaveBool(string name,bool b) {
        PlayerPrefs.SetInt(name, b?1:0);

    }

    private bool ReadBool(string name) {
        return PlayerPrefs.GetInt(name) == 1 ? true : false;
    }

    private void UpdateToolTip() {
        var UseWiderRangeValue = ReadBool("UseWiderRange");
        var ShadeAngleRangeMinValue = PlayerPrefs.GetFloat("ShadeAngleRangeMin");
        var ShadeAngleRangeMaxValue = PlayerPrefs.GetFloat("ShadeAngleRangeMax");
        var InnerRange = Mathf.Abs(ShadeAngleRangeMaxValue - ShadeAngleRangeMinValue);
        var InnerSubValue = PlayerPrefs.GetInt("InnerSub");
        var InnerLoopTimeValue = PlayerPrefs.GetInt("InnerLoopTime");

        var InnerCalCount = UseWiderRangeValue ? InnerSubValue + InnerSubValue * 2 * (InnerLoopTimeValue - 1) : InnerSubValue * InnerLoopTimeValue;
        InnerTip.text = "Expected Accuracy: " + (float)InnerRange / (Mathf.Pow(InnerSubValue, InnerLoopTimeValue)) + " | Inter Cal Count :" + InnerCalCount;


        var GlobalShadeAngleRangeMinValue = PlayerPrefs.GetFloat("GlobalShadeAngleRangeMin");
        var GlobalShadeAngleRangeMaxValue = PlayerPrefs.GetFloat("GlobalShadeAngleRangeMax");
        var OuterRange = Mathf.Abs(GlobalShadeAngleRangeMaxValue - GlobalShadeAngleRangeMinValue);
        var OuterSubValue = PlayerPrefs.GetInt("OuterSub");
        var OuterLoopTimeValue = PlayerPrefs.GetInt("OuterLoopTime");

        var OuterCalCount = UseWiderRangeValue ? OuterSubValue + OuterSubValue * 2 * (OuterLoopTimeValue - 1) : OuterSubValue * OuterLoopTimeValue;

        OuterTip.text = "Expected Accuracy: " + (float)OuterRange / ( Mathf.Pow(OuterSubValue,OuterLoopTimeValue) ) + " | Outer Cal Count :" + OuterCalCount;
    }
    public void SetDayRangeStart() {
        PlayerPrefs.SetInt("DayRangeStart",int.Parse(DayRangeStart.text));
        main.startDayIndex = int.Parse(DayRangeStart.text);
        PlayerPrefs.Save();
    }

    public void SetDayRangeEnd() {
        PlayerPrefs.SetInt("DayRangeEnd", int.Parse(DayRangeEnd.text));
        main.endDayIndex = int.Parse(DayRangeEnd.text);
        PlayerPrefs.Save();
    }

    public void SetDayStep() {
        PlayerPrefs.SetInt("DayStep", int.Parse(DayStep.text));
        main.DayStep = int.Parse(DayStep.text);
        PlayerPrefs.Save();
    }

    public void SetTimeRangeStart() {
        PlayerPrefs.SetInt("TimeRangeStart", int.Parse(TimeRangeStart.text));
        main.startMinuteIndex = int.Parse(TimeRangeStart.text);
        PlayerPrefs.Save();
    }

    public void SetTimeRangeEnd() {
        PlayerPrefs.SetInt("TimeRangeEnd", int.Parse(TimeRangeEnd.text));
        main.endMinuteIndex = int.Parse(TimeRangeEnd.text);
        PlayerPrefs.Save();
    }

    public void SetTimeStep() {
        PlayerPrefs.SetInt("TimeStep", int.Parse(TimeStep.text));
        main.MinuteStep = int.Parse(TimeStep.text);
        PlayerPrefs.Save();
    }

    public void SetSaveAfterComplete() {
        SaveBool("SaveAfterComplete", SaveAfterComplete.isOn);
        main.SaveAfterCal = SaveAfterComplete.isOn;
        PlayerPrefs.Save();
    }

    public void SetFilePath() {
        //PlayerPrefs.SetString("FilePath", FilePath.text);
        main.defaultFileName = FilePath.text;
        //PlayerPrefs.Save();
    }

    public void SetShaderAutoUpdate() {
        SaveBool("ShaderAutoUpdate", ShaderAutoUpdate.isOn);
        main.AutoUpdate = ShaderAutoUpdate.isOn;
        PlayerPrefs.Save();
    }

    public void SetPreview() {
        SaveBool("Preview", Preview.isOn);
        main.Preview = Preview.isOn;
        PlayerPrefs.Save();
    }

    public void SetSize() {
        PlayerPrefs.SetInt("Size", int.Parse(Size.text));
        main.size = int.Parse(Size.text);
        Debug.Log("main.size2" + main.size);
        PlayerPrefs.Save();
    }

    public void SetMSAA() {
        PlayerPrefs.SetInt("MSAA", int.Parse(MSAA.text));
        main._multiSample = int.Parse(MSAA.text);
        PlayerPrefs.Save();
    }

    public void SetEmitRange() {
        PlayerPrefs.SetFloat("EmitRange", float.Parse(EmitRange.text));
        main._emitRange = float.Parse(EmitRange.text);
        PlayerPrefs.Save();
    }

    public void SetEmitDistance() {
        PlayerPrefs.SetFloat("EmitDistance", float.Parse(EmitDistance.text));
        main._emitDistance = float.Parse(EmitDistance.text);
        PlayerPrefs.Save();
    }

    public void SetSunAutoUpdate() {
        SaveBool("SunAutoUpdate", SunAutoUpdate.isOn);
        sunManager.AutoUpdate = SunAutoUpdate.isOn;
        PlayerPrefs.Save();
    }
    public void SetLon() {
        PlayerPrefs.SetFloat("Lon", float.Parse(Lon.text));
        sunManager.lon = float.Parse(Lon.text);
        PlayerPrefs.Save();
    }

    public void SetLat() {
        PlayerPrefs.SetFloat("Lat", float.Parse(Lat.text));
        sunManager.lat = float.Parse(Lat.text);
        PlayerPrefs.Save();
    }

    public void SetMonth() {
        PlayerPrefs.SetInt("Month", int.Parse(Month.text));
        sunManager.month = int.Parse(Month.text);
        PlayerPrefs.Save();
    }
    public void SetDay() {
        PlayerPrefs.SetInt("Day", int.Parse(Day.text));
        sunManager.day = int.Parse(Day.text);
        PlayerPrefs.Save();
    }
    public void SetHour() {
        PlayerPrefs.SetInt("Hour", int.Parse(Hour.text));
        sunManager.hour = int.Parse(Hour.text);
        PlayerPrefs.Save();
    }

    public void SetMinute() {
        PlayerPrefs.SetInt("Minute", int.Parse(Minute.text));
        sunManager.minute = int.Parse(Minute.text);
        PlayerPrefs.Save();
    }
    public void SetGlobalShadeAngle() {
        PlayerPrefs.SetFloat("GlobalShadeAngle", float.Parse(GlobalShadeAngle.text));
        shadeControl.GlobalShadeAngle = float.Parse(GlobalShadeAngle.text);
        PlayerPrefs.Save();
    }

    public void SetGlobalShadeLengthMin() {
        PlayerPrefs.SetFloat("GlobalShadeLengthMin", float.Parse(GlobalShadeLengthMin.text));
        shadeControl.GlobalShadeLengthMin = float.Parse(GlobalShadeLengthMin.text);
        PlayerPrefs.Save();
    }
    public void SetGlobalShadeLengthMax() {
        PlayerPrefs.SetFloat("GlobalShadeLengthMax", float.Parse(GlobalShadeLengthMax.text));
        shadeControl.GlobalShadeLengthMax = float.Parse(GlobalShadeLengthMax.text);
        PlayerPrefs.Save();
    }
    public void SetGlobalShadeWidth() {
        PlayerPrefs.SetFloat("GlobalShadeWidth", float.Parse(GlobalShadeWidth.text));
        shadeControl.GlobalShadeWidth = float.Parse(GlobalShadeWidth.text);
        PlayerPrefs.Save();
    }
    public void SetShadeNumber() {
        PlayerPrefs.SetInt("ShadeNumber", int.Parse(ShadeNumber.text));
        shadeControl.ShadeNum = int.Parse(ShadeNumber.text);
        PlayerPrefs.Save();
    }
    public void SetShadeAngleRangeMin() {
        PlayerPrefs.SetFloat("ShadeAngleRangeMin", float.Parse(ShadeAngleRangeMin.text));
        shadeControl.Min = float.Parse(ShadeAngleRangeMin.text);
        PlayerPrefs.Save();
    }
    public void SetShadeAngleRangeMax() {
        PlayerPrefs.SetFloat("ShadeAngleRangeMax", float.Parse(ShadeAngleRangeMax.text));
        shadeControl.Max = float.Parse(ShadeAngleRangeMax.text);
        PlayerPrefs.Save();
    }

    public void SetGlobalShadeAngleRangeMin() {
        PlayerPrefs.SetFloat("GlobalShadeAngleRangeMin", float.Parse(GlobalShadeAngleRangeMin.text));
        shadeControl.Min2 = float.Parse(GlobalShadeAngleRangeMin.text);
        PlayerPrefs.Save();
    }
    public void SetGlobalShadeAngleRangeMax() {
        PlayerPrefs.SetFloat("GlobalShadeAngleRangeMax", float.Parse(GlobalShadeAngleRangeMax.text));
        shadeControl.Max2 = float.Parse(GlobalShadeAngleRangeMax.text);
        PlayerPrefs.Save();
    }

    public void SetShadeLength() {
        PlayerPrefs.SetFloat("ShadeLength", float.Parse(ShadeLength.text));
        shadeControl.ShadeLength = float.Parse(ShadeLength.text);
        PlayerPrefs.Save();
    }

    public void SetPVMode() {
        PlayerPrefs.SetInt("PVMode", PVMode.value);
        gameInfo.currentPVMode = PVMode.value;
        PlayerPrefs.Save();

    }
    public void SetModelMode() {
        PlayerPrefs.SetInt("ModelMode", ModelMode.value);
        gameInfo.currentModelMode = ModelMode.value;
        PlayerPrefs.Save();
        

    }

    public void SetInnerSub() {
        PlayerPrefs.SetInt("InnerSub",int.Parse(InnerSub.text));
        main._innerSub = int.Parse(InnerSub.text);
        PlayerPrefs.Save();
        UpdateToolTip();
    }
    public void SetInnerLoopTime() {
        PlayerPrefs.SetInt("InnerLoopTime", int.Parse(InnerLoopTime.text));
        main._innerLoopTime = int.Parse(InnerLoopTime.text);
        PlayerPrefs.Save();
        UpdateToolTip();
    }

    public void SetOuterSub() {
        PlayerPrefs.SetInt("OuterSub", int.Parse(OuterSub.text));
        main._outterSub = int.Parse(OuterSub.text);
        PlayerPrefs.Save();
        UpdateToolTip();
    }
    public void SetOuterLoopTime() {
        PlayerPrefs.SetInt("OuterLoopTime", int.Parse(OuterLoopTime.text));
        main._outterLoopTime = int.Parse(OuterLoopTime.text);
        PlayerPrefs.Save();
        UpdateToolTip();
    }
    public void SetUseWiderRange() {
        SaveBool("UseWiderRange",UseWiderRange.isOn);
        main._useWiderRange = UseWiderRange.isOn;
        PlayerPrefs.Save();
        UpdateToolTip();
    }
    public void ExpertMode() {
        if(ExpertModePanel.sizeDelta.y <= 150f) {
            StopCoroutine("SetExpertModeSize");
            StartCoroutine("SetExpertModeSize", 300f);
            StopCoroutine("RotateExpertModeLogo");
            StartCoroutine("RotateExpertModeLogo", 180f);
        } else {
            StopCoroutine("SetExpertModeSize");
            StartCoroutine("SetExpertModeSize", 0f);
            StopCoroutine("RotateExpertModeLogo");
            StartCoroutine("RotateExpertModeLogo", 0f);
        }
    }

    public void SetPVYOffset() {
        PlayerPrefs.SetFloat("PVYOffset", float.Parse(PVYOffset.text));
        shadeControl.PVYOffset = float.Parse(PVYOffset.text);
        PlayerPrefs.Save();
    }

    public void SetPVZOffset() {
        PlayerPrefs.SetFloat("PVZOffset", float.Parse(PVZOffset.text));
        shadeControl.PVZOffset = float.Parse(PVZOffset.text);
        PlayerPrefs.Save();
    }

    public void SetPVRotation() {
        PlayerPrefs.SetFloat("PVRotation", float.Parse(PVRotation.text));
        shadeControl.PVRotation = float.Parse(PVRotation.text);
        PlayerPrefs.Save();
    }

    public void SetPVHeight() {
        PlayerPrefs.SetFloat("PVHeight", float.Parse(PVHeight.text));
        shadeControl.PVHeight = float.Parse(PVHeight.text);
        PlayerPrefs.Save();
    }

    public void SetShadeZOffset() {
        PlayerPrefs.SetFloat("ShadeZOffset", float.Parse(ShadeZOffset.text));
        shadeControl.ShadeZOffset = float.Parse(ShadeZOffset.text);
        PlayerPrefs.Save();
    }

    IEnumerator SetExpertModeSize(float y) {
        while (Mathf.Abs( ExpertModePanel.sizeDelta.y - y) > 1f) {
            ExpertModePanel.sizeDelta += new Vector2(0, (y - ExpertModePanel.sizeDelta.y) * 5f * UnityEngine.Time.deltaTime);
            yield return null;
        }
        ExpertModePanel.sizeDelta = new Vector2(ExpertModePanel.sizeDelta.x, y);
    }

    IEnumerator RotateExpertModeLogo(float target) {
        while(Mathf.Abs(ExpertModeLogo.transform.eulerAngles.z - target) > 1) {
            ExpertModeLogo.transform.eulerAngles += new Vector3(0, 0, (target - ExpertModeLogo.transform.eulerAngles.z) * 5f * UnityEngine.Time.deltaTime);
            yield return null;
        }
        ExpertModeLogo.transform.eulerAngles = new Vector3(ExpertModeLogo.transform.eulerAngles.x, ExpertModeLogo.transform.eulerAngles.y, target);
    }
    public void Restart() {
        Destroy(gameInfo.gameObject);
        SceneManager.LoadScene(0);
    }
}
