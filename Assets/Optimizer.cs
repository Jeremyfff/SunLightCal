using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Optimizer : MonoBehaviour {

    
    [Button("计算有效范围")]
    private void ClickEvent_GetValidValue() { Debug.Log("calculating valid range");this.motor1ValidRange = new range(0.0f, 1.0f);this.motor2ValidRange = new range(0.0f, 0.0f);GetValidRange(out this.motor1ValidRange, out this.motor2ValidRange);}

    [Button("进行单次计算")]
    private void ClickEvent_SingleOptimize() { float motor1pos;float motor2pos;float bestarea; SingleOptimize(out motor1pos, out motor2pos, out bestarea,true); }

    [Button("单日计算")]
    private void ClickEvent_DayOptimize() { StartCoroutine("IE_DayOptimize",false); }

    [Button("全年计算")]
    private void ClickEvent_AnnualOptimize() { StartCoroutine("IE_AnnualOptimize"); }



    [SerializeField] private float motor1Granularity;
    [SerializeField] private float granularityStep1 = 8f;
    [SerializeField] private float motor2Granularity;
    [SerializeField] private float granularityStep2 = 8f;

    [HorizontalLine(height: 0.5f)]
    [SerializeField] private int minuteStart;
    [SerializeField] private int minuteEnd;
    [SerializeField] private int minuteGap;
    [ProgressBar("MinuteProgress",100,EColor.Green)]
    [SerializeField] private int minuteProgress;
    private bool minuteWorking;


    [HorizontalLine(height: 0.5f)]
    [SerializeField] private int dayStart;
    [SerializeField] private int dayEnd;
    [SerializeField] private int dayGap;
    [ProgressBar("DayProgress", 100, EColor.Green)]
    [SerializeField] private int dayProgress;
    private bool dayWorking;

    [HorizontalLine(height: 0.5f)]
    [SerializeField] private CalculateStucture structure;
    [SerializeField] private SunManager sunManager;

    


    [TextArea]
    [SerializeField] private string dataPool;


    //GUI Debug
    
    float motor1pos; float motor2pos;
    [DebugGUIGraph(min: 0, max: 2)]
    float bestArea; 
    float bestRadiation;

    //

    private range motor1ValidRange = new range(0.0f,1.0f);
    private range motor2ValidRange = new range(0.0f,1.0f);
    private bool orgDrawGizmos;
    private float orgMotor1Pos;
    private float orgMotor2Pos;

    private List<float> DirectHavList;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }




    private void GetValidRange(out range range1,out range range2)
    {

        //初始化
        long startTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        int counter = 0;

        range1 = new range(0f,1f);
        range2 = new range(0f,0f);
        //错误检查
        if(this.motor1Granularity == 0 || this.motor2Granularity == 0)
        {
            Debug.LogWarning("粒度不能为0，请检查Motor1或2 的 Granularity设置");
            return;
        }

        orgDrawGizmos = structure.DrawGizmos;
        if(structure.DrawGizmos == true)
        {
            structure.DrawGizmos = false;
        }

        float[] result = new float[3];
        float[] distance = new float[3];

        var valid = false;
        var motor2Pos = 0f;

        //正跑
        while (motor2Pos < 1)
        {
            structure.SetMotor2(motor2Pos);
            valid = structure.SingleCal(true, out result,out distance);
            counter++;
            if (valid == true)
            {
                break;
            }
            motor2Pos += motor2Granularity;
        }
        //错误检查
        if (valid == false)
        {
            //如果跑完全程依然没有valid
            //有两种原因，第一种是粒度不够，第二种是无有效区间
            Debug.LogWarning("未找到有效区间，请检查是否粒度过大，或是否有结构问题");
            return;

        }

        range2.min = motor2Pos;//将结果保存
        motor2Pos = 1;
        //反跑
        while(motor2Pos  > 0)
        {
            structure.SetMotor2(motor2Pos);
            valid = structure.SingleCal(true, out result,out distance);
            counter++;
            if (valid == true)
            {
                break;
            }
            motor2Pos -= motor2Granularity;
        }

        range2.max = motor2Pos;//将结果保存

        long endTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;

        Debug.Log("successfully cal valid range");
        Debug.Log("motor1 valid range:" + range1.toString());
        Debug.Log("motor2 valid range:" + range2.toString());
        Debug.Log("------------------------------");
        Debug.Log("共进行了"+counter +"次计算， 耗时" + (endTime - startTime).ToString()+"ms");
        structure.DrawGizmos = orgDrawGizmos;//恢复初始状态
        return;
    }



    /// <summary>
    /// 以当前日期时间进行优化
    /// </summary>
    /// <param name="debugMode">是否打印输出</param>
    private bool SingleOptimize(out float result_motor1pos,out float result_motor2pos, out float result_totalArea, bool debugMode)
    {
        result_motor1pos = 0f;
        result_motor2pos = 0f;
        result_totalArea = 0f;

        var counter = 0;
        long startTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        var motorBoundary1 = motor1ValidRange;
        var motorBoundary2 = motor2ValidRange;

        float[] area = new float[3];
        float[] distance = new float[3];
        
        var bestScore = 0f;
        var bestArea = 0f;
        var bestMotor1Pos = -1f;
        var bestMotor2Pos = -1f;

        orgDrawGizmos = structure.DrawGizmos;
        orgMotor1Pos = structure.GetMotor1();
        orgMotor2Pos = structure.GetMotor2();
        
        structure.DrawGizmos = false;

        //初始化电机1的粒度
        var currentGranularity1 = motor1ValidRange.GetLength() / granularityStep1;
        var currentGranularity2_g = 0f;

        var possibleRange1 = motor1ValidRange;
        while (currentGranularity1 > motor1Granularity)
        {
            //电机1 的循环
            //当粒度未满足要求时
            if (debugMode) {
                Debug.Log("currentGranularity1 = " + currentGranularity1);
            }
            
            var motor1Pos = possibleRange1.min;//重置电机1位置


            while(motor1Pos < possibleRange1.max)
            {
                //当电机位置在可能区间内时,对motor2进行计算
                
                //初始化电机2 的粒度
                var currentGranularity2 = motor2ValidRange.GetLength() / granularityStep2;
                //初始化range
                var possibleRange2 = motor2ValidRange;
                while (currentGranularity2 > motor2Granularity)
                {
                    //电机2 的循环
                    //当粒度未满足要求时
                    var motor2Pos = possibleRange2.min;//重置电机2位置


                    while (motor2Pos < possibleRange2.max)
                    {
                        //当电机位置在可能区间内时，进行计算
                        structure.SetMotor1(motor1Pos);
                        structure.SetMotor2(motor2Pos);
                        var success = structure.SingleCal(false, out area, out distance);
                        counter++;

                        if (!success) {
                            Debug.LogWarning("出现计算错误，已停止");
                            structure.SetMotor1(orgMotor1Pos);
                            structure.SetMotor2(orgMotor2Pos);
                            structure.DrawGizmos = orgDrawGizmos;
                            return false;
                        }
                        //Debug.Log("counter:" + counter+"/10000");
                        if(counter > 10000)
                        {
                            Debug.LogWarning("counter out of max range,exit");
                            structure.SetMotor1(orgMotor1Pos);
                            structure.SetMotor2(orgMotor2Pos);
                            structure.DrawGizmos = orgDrawGizmos;
                            return false;
                        }

                        //记录计算结果，并与最佳结果进行比较
                        var score = 0.0f;
                        var totalArea = 0.0f;
                        for (int i = 0; i < 3; i++)
                        {
                            score += area[i] + 0.001f / distance[i];
                            totalArea += area[i];
                        }
/*                        if (debugMode) {
                            Debug.Log("score = " + score + "motor1 = " + motor1Pos + "motor2 = " + motor2Pos);
                        }*/
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestArea = totalArea;
                            bestMotor1Pos = motor1Pos;
                            bestMotor2Pos = motor2Pos;
                            //Debug.Log("find better score: score = " + score + ", motor1 pos = " + motor1Pos + ", motor2 pos = "+motor2Pos);
                        }
                        //根据当前粒度，增加电机2位置
                        motor2Pos += currentGranularity2;

                    }//电机2在当前粒度下步进结束

                    //根据best pos 更新下一次的possible 区间
                    possibleRange2 = new range(bestMotor2Pos - currentGranularity2, bestMotor2Pos + currentGranularity2,motorBoundary2);


                    currentGranularity2 /= granularityStep2;//更新粒度
                    currentGranularity2_g = currentGranularity2;//传递给外圈
                }//电机2 总循环结束

                //根据当前粒度，增加电机1位置
                motor1Pos += currentGranularity1;
                
            }//电机1在当前粒度下步进结束

            //根据best pos 更新下一次的possible 区间
            possibleRange1 = new range(bestMotor1Pos - currentGranularity1,bestMotor1Pos+currentGranularity1,motorBoundary1);



            currentGranularity1 /= granularityStep1;//更新粒度

        }//电机1 总循环结束

        long endTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        if (debugMode) {
            Debug.Log("===================cal complete====================");
            Debug.Log("final granularity1 = " + currentGranularity1 + "|final granularity2 = " + currentGranularity2_g);
            Debug.Log("共进行" + counter + "次计算，共耗时" + (endTime - startTime) + "ms");
            Debug.Log("bestArea" + bestArea);
            Debug.Log("best Motor1 Pos = " + bestMotor1Pos + "| best Motor2 Pos = " + bestMotor2Pos);
        }

        structure.SetMotor1(bestMotor1Pos);
        structure.SetMotor2(bestMotor2Pos);
        structure.DrawGizmos = orgDrawGizmos;

        result_motor1pos = bestMotor1Pos;
        result_motor2pos = bestMotor2Pos;
        result_totalArea = bestArea;

        return true;
    }



    IEnumerator IE_DayOptimize(bool record) {
        minuteWorking = true;//标记进入工作状态
        int currentMinuteIndex = this.minuteStart;
        minuteProgress = 0;
        while (currentMinuteIndex < this.minuteEnd) {
            Debug.Log("current minute index = " + currentMinuteIndex);
            var sunDir = sunManager.SetTimeAndUpdate(0, sunManager.day, 0, currentMinuteIndex);
/*            if(Vector3.Dot(sunDir,Vector3.up)>0 || Vector3.Dot(sunDir, -Vector3.forward) > 0) {
                //排除太阳位置不可能照射到的情况
                currentMinuteIndex += this.minuteGap;
                minuteProgress = 100 * (currentMinuteIndex - minuteStart) / (minuteEnd - minuteStart);

                continue;
            }*/
            
            var success = SingleOptimize(out motor1pos, out motor2pos, out bestArea, false);

            
            if(DirectHavList == null) {
                DirectHavList = FileOperator.ReadDirectHav();
            }
            bestRadiation = bestArea * GetAvgDirectHav(DirectHavList, sunManager.day, currentMinuteIndex,dayGap);

            
            currentMinuteIndex += this.minuteGap;
            minuteProgress = 100*(currentMinuteIndex - minuteStart) / (minuteEnd - minuteStart);

            if (record) {
                float[] values = { motor1pos, motor2pos, bestArea,bestRadiation };
                dataPool += toStringCsv(sunManager.day, sunManager.minute, values);
            }
            yield return null;

        }
        minuteWorking = false;
    }


    IEnumerator IE_AnnualOptimize() {
        dayWorking = true;
        int currentDayIndex = this.dayStart;
        dayProgress = 0;
        dataPool = "";//清空dataPool
        while(currentDayIndex < this.dayEnd) {
            Debug.Log(">>current day index = " + currentDayIndex);
            var sunDir = sunManager.SetTimeAndUpdate(0, currentDayIndex, 0, sunManager.minute);

            StartCoroutine("IE_DayOptimize",true);
            yield return new WaitUntil(() => minuteWorking == false);
            
            currentDayIndex += this.dayGap;
            dayProgress = 100 * (currentDayIndex - dayStart) / (dayEnd - dayStart);
        }

        Debug.Log("cal complete");
        Debug.Log(dataPool);

        dayWorking = false;
    }

    private string toStringCsv(int dayIndex,int minuteIndex,float[] values) {
        string result = dayIndex.ToString() + "," + minuteIndex.ToString();
        foreach(var value in values) {
            result += "," + value.ToString();
        }
        result += "\n";
        return result;
    }

    private float GetAvgDirectHav(List<float> DirectHav,int dayIndex, int minuteIndex, int dayStep) {
        float SumDirectHav = 0;
        int count = 0;
        for (int i = dayIndex - dayStep / 2; i <= dayIndex + dayStep / 2; i++) {
            SumDirectHav += DirectHav[(i * 24 + minuteIndex / 60 + DirectHav.Count) % DirectHav.Count];
            count++;
        }

        return SumDirectHav / count;
    }
}

    

class range
{
    public float min;
    public float max;
    public range(float min,float max)
    {
        this.min = min;
        this.max = max;
    }
    public range()
    {
        this.min = 0;
        this.max = 0;
    }
    public range(float min,float max,range boundary) {
        if(min > boundary.min) {
            this.min = min;
        } else {
            this.min = boundary.min;
        }

        if(max < boundary.max) {
            this.max = max;

        } else {
            this.max = boundary.max;
        }
    }
    public string toString()
    {
        var result = "range:[" + this.min + ", " + this.max + "]";
        return result;
    }
    public float GetLength()
    {
        return this.max - this.min;
    }
}