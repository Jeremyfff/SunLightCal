using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Optimizer : MonoBehaviour
{

    [SerializeField] private CalculateStucture structure;
    [SerializeField] private SunManager sunManager;

    [SerializeField] private bool 计算有效范围;
    [SerializeField] private bool 进行单次计算;


    [SerializeField] private float motor1Granularity;
    [SerializeField] private float motor2Granularity;


    private range motor1ValidRange;
    private range motor2ValidRange;
    private bool orgDrawGizmos;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("IE_ListenGetValidRange");
        StartCoroutine("IE_ListenSingleCal");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator IE_ListenGetValidRange()
    {
        yield return new WaitUntil(() => 计算有效范围 == true);
        计算有效范围 = false;
        Debug.Log("calculating valid range");

        this.motor1ValidRange = new range();
        this.motor2ValidRange = new range();

        GetValidRange(out this.motor1ValidRange,out this.motor2ValidRange);
        StartCoroutine("IE_ListenGetValidRange");
        
    }

    private void GetValidRange(out range range1,out range range2)
    {
        //初始化
        long startTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        int counter = 0;

        range1 = new range(0f,1f);
        range2 = new range();
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


    IEnumerator IE_ListenSingleCal()
    {
        yield return new WaitUntil(() => 进行单次计算 == true);
        进行单次计算 = false;
        SingleOptimize();
        StartCoroutine("IE_ListenSingleCal");
    }

    private void SingleOptimize()
    {
        var counter = 0;

        float[] area = new float[3];
        float[] distance = new float[3];
        
        var bestScore = 0f;
        var bestArea = 0f;
        var bestMotor1Pos = -1f;
        var bestMotor2Pos = -1f;

        orgDrawGizmos = structure.DrawGizmos;
        structure.DrawGizmos = false;

        //初始化电机1的粒度
        var currentGranularity1 = motor1ValidRange.GetLength() / 8f;
        var possibleRange1 = motor1ValidRange;
        while (currentGranularity1 > motor1Granularity)
        {
            //电机1 的循环
            //当粒度未满足要求时
            var motor1Pos = possibleRange1.min;//重置电机1位置


            while(motor1Pos < possibleRange1.max)
            {
                //当电机位置在可能区间内时,对motor2进行计算

                //初始化电机2 的粒度
                var currentGranularity2 = motor2ValidRange.GetLength() / 8f;
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
                        structure.SingleCal(false, out area, out distance);
                        counter++;
                        Debug.Log("counter:" + counter+"/10000");
                        if(counter > 10000)
                        {
                            Debug.LogWarning("counter out of max range,exit");
                            return;
                        }

                        //记录计算结果，并与最佳结果进行比较
                        var score = 0.0f;
                        var totalArea = 0.0f;
                        for (int i = 0; i < 3; i++)
                        {
                            score += area[i] + 0.01f / distance[i];
                            totalArea += area[i];
                        }

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestArea = totalArea;
                            bestMotor1Pos = motor1Pos;
                            bestMotor2Pos = motor2Pos;
                        }
                        //根据当前粒度，增加电机2位置
                        motor2Pos += currentGranularity2;

                    }//电机2在当前粒度下步进结束

                    //根据best pos 更新下一次的possible 区间
                    possibleRange2 = new range(bestMotor2Pos - currentGranularity2, bestMotor2Pos + currentGranularity2);


                    currentGranularity2 /= 8f;//更新粒度

                }//电机2 总循环结束

                //根据当前粒度，增加电机1位置
                motor1Pos += currentGranularity1;

            }//电机1在当前粒度下步进结束

            //根据best pos 更新下一次的possible 区间
            possibleRange1 = new range(bestMotor1Pos - currentGranularity1,bestMotor1Pos+currentGranularity1);



            currentGranularity1 /= 8f;//更新粒度

        }//电机1 总循环结束
        Debug.Log("=======cal complete=======");
        Debug.Log("bestArea" + bestArea);
        Debug.Log("bestMotor1Pos" + bestMotor1Pos);
        Debug.Log("bestMotor2Pos" + bestMotor2Pos);
        Debug.Log("共进行" + counter + "次计算，共耗时" + "ms");

        structure.DrawGizmos = orgDrawGizmos;
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