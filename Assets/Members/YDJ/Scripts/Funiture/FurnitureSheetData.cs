using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public struct FurnitureSheetData
{
    [field: SerializeField] public string Url { get; private set; }
    [field: SerializeField] public SheetType Type { get; private set; }
    public char SplitSymbol => Type == SheetType.CSV ? ',' : '\t';
    
    public IEnumerator Load(Action<char, string[]> successCallback)
    {
        string sheetId = Url.Split("d/")[1].Split('/')[0];
        
        string gid = Url.Split("gid=")[1].Split('&')[0].Split('#')[0];

        string format = Type == SheetType.CSV ? "csv" : "tsv";
        
        string exportUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format={format}&gid={gid}";
        
        using (UnityWebRequest uwr = UnityWebRequest.Get(exportUrl))
        {
            yield return uwr.SendWebRequest();
            
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(uwr.error);
                yield break;
            }
            
            string sheetDataText = uwr.downloadHandler.text;
            
            string[] lines = sheetDataText.Split('\n');

            successCallback?.Invoke(SplitSymbol, lines);
            Debug.Log("[FurnitureSheetData] Success Loaded Google Sheet Data");
        }
    }
}
