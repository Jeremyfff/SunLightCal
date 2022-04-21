using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Data {
    public List<int> dateIndexList;
    public List<int> minuteIndexList;
    public List<float> bestInputVarList;
    public List<float> bestInputVarList2;
    public List<float> bestDirectScoreList;
    public List<float> bestReflectScoreList;
    public List<float> bestTotalScoreList;
    public List<float> bestPVScoreList;
    public List<float> bestDirectValueList;
    public List<float> bestReflectValueList;
    public List<float> bestTotalValueList;
    public List<float> bestPVValueList;
    public List<bool> needToCalList;
    public string info;

    public Data(List<int> dateIndexList, List<int> minuteIndexList, List<float> bestInputVarList, List<float> bestInputVarList2,
        List<float> bestDirectScoreList, List<float> bestReflectScoreList, List<float> bestTotalScoreList, List<float> bestPVScoreList,
        List<float> bestDirectValueList, List<float> bestReflectValueList, List<float> bestTotalValueList, List<float> bestPVValueList,
        List<bool> needToCalList,
        string info) {
        this.dateIndexList = dateIndexList;
        this.minuteIndexList = minuteIndexList;
        this.bestInputVarList = bestInputVarList;
        this.bestInputVarList2 = bestInputVarList2;
        this.bestDirectScoreList = bestDirectScoreList;
        this.bestReflectScoreList = bestReflectScoreList;
        this.bestTotalScoreList = bestTotalScoreList;
        this.bestPVScoreList = bestPVScoreList;
        this.bestDirectValueList = bestDirectValueList;
        this.bestReflectValueList = bestReflectValueList;
        this.bestTotalValueList = bestTotalValueList;
        this.bestPVValueList = bestPVValueList;
        this.needToCalList = needToCalList;
        this.info = info;
    }
    public Data() {
        this.dateIndexList = new List<int>();
        this.minuteIndexList = new List<int>();
        this.bestInputVarList = new List<float>();
        this.bestInputVarList2 = new List<float>();
        this.bestDirectScoreList = new List<float>();
        this.bestReflectScoreList = new List<float>();
        this.bestTotalScoreList = new List<float>();
        this.bestPVScoreList = new List<float>();
        this.bestDirectValueList = new List<float>();
        this.bestReflectValueList = new List<float>();
        this.bestTotalValueList = new List<float>();
        this.bestPVValueList = new List<float>();
        this.needToCalList = new List<bool>();
        this.info = "";
    }

    
}

