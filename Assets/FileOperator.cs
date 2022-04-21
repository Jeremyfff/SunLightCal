using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SFB;
using UnityEngine;

class FileOperator {
    public static void WriteToCsv(Data Data,string defaultFileName) {
        // Save file with filter
        //dateIndexList, minuteIndexList, bestInputVarList, bestDirectScoreList, bestReflectScoreList, bestTotalScoreList, bestPVScoreList, bestDirectValueList, bestReflectValueList, bestTotalValueList, bestPVValueList

        Data.info = GetCurrentDateInfo() + "-" + Data.info;
        
        //info   20220312130155-D:0:365:30-T:361:1081:30-S:256:2:3.5:10-L:120:32-M:-20:0.5:1.2:2.0:6:-20:40:0.2
        var extensionList = new[] {
        new ExtensionFilter("Csv File", "csv"),
        };


        var FileSavePath = StandaloneFileBrowser.SaveFilePanel("Save File", "", GetCurrentDateInfo()+"_"+defaultFileName, extensionList);

        if(FileSavePath == null || FileSavePath == ""){
            return;
        }
        string data = "";
        data += "dateIndex,minuteIndex,bestInputVar,bestInputVar2,bestDirectScore,bestReflectScore,bestTotalScore,bestPVScore,bestDirectValue,bestReflectValue,bestTotalValue,bestPVValue,needToCal," + Data.info +"\n";
        for (int i = 0; i < Data.dateIndexList.Count; i++) {
            data += Data.dateIndexList[i] + "," + Data.minuteIndexList[i] + "," + Data.bestInputVarList[i] + "," + Data.bestInputVarList2[i] + ","
                + Data.bestDirectScoreList[i] + "," + Data.bestReflectScoreList[i] + "," + Data.bestTotalScoreList[i] + "," + Data.bestPVScoreList[i] + ","
                + Data.bestDirectValueList[i] + "," + Data.bestReflectValueList[i] + "," + Data.bestTotalValueList[i] + "," + Data.bestPVValueList[i] + ","
                + Data.needToCalList[i]
                + "\n";
        }


        File.WriteAllText(FileSavePath, data);
    }

    public static List<float> ReadDirectHav() {

         return ReadHav("directHav");
    }

    public static List<float> ReadDiffuseHav() {

        return ReadHav("diffuseHav");
    }

    public static List<float> ReadGlobalHav() {

        return ReadHav("GHav");
    }

    private static List<float> ReadHav(string name) {
        var data = Resources.Load<TextAsset>(name).ToString();

        var lines = data.Split("\n");
        var result = new List<float>();
        foreach (var line in lines) {
            if(line != "") {
                try {
                    result.Add(float.Parse(line));
                } catch {

                    Debug.LogWarning("error line" + line + ",i = " + result.Count);
                }
            }
            
            
        }
        return result;
    }

    private static string GetCurrentDateInfo() {

        string dateInfo = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        var dateInfoList = dateInfo.Split("-");
        string date = "";
        foreach (var d in dateInfoList) {
            date += d;
        }
        return date;
    }

    public static Data ReadCsv(string path) {
        try {
            var data = File.ReadAllText(path);
            Debug.Log(data);
            var lines = data.Split("\n");

            var result = new Data();
            var column = lines[0].Split(",");
            result.info = column[column.Length - 1];
            for (int i = 1; i < lines.Length - 1; i++) {
                var lineItem = lines[i].Split(",");
                Debug.Log(lineItem[0]);
                result.dateIndexList.Add(int.Parse(lineItem[0]));
                result.minuteIndexList.Add(int.Parse(lineItem[1]));
                result.bestInputVarList.Add(float.Parse(lineItem[2]));
                result.bestInputVarList2.Add(float.Parse(lineItem[3]));
                result.bestDirectScoreList.Add(float.Parse(lineItem[4]));
                result.bestReflectScoreList.Add(float.Parse(lineItem[5]));
                result.bestTotalScoreList.Add(float.Parse(lineItem[6]));
                result.bestPVScoreList.Add(float.Parse(lineItem[7]));
                result.bestDirectValueList.Add(float.Parse(lineItem[8]));
                result.bestReflectValueList.Add(float.Parse(lineItem[9]));
                result.bestTotalValueList.Add(float.Parse(lineItem[10]));
                result.bestPVValueList.Add(float.Parse(lineItem[11]));
                result.needToCalList.Add(lineItem[12] == "True" ? true : false);

            }

            return result;
        } catch {
            Debug.LogError("文件不符合规范");
            return null;
        }
        
    }
}

