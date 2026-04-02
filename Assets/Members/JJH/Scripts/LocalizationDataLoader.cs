using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;

public class LocalizationDataLoader : MonoBehaviour
{
    [Header("Google Sheet Settings")]
    [SerializeField] private string sheetURL = "https://docs.google.com/spreadsheets/d/1GOnAJjcQV4ZlfeWOjQxG4L6o8uYbqgEyCePhL-jTcSw/export?format=tsv&gid=1454231805";
    [SerializeField] private string tableName = "Tutorial";

    public void UpdateLocalization(System.Action onComplete = null)
    {
        StartCoroutine(LoadLocalizationRoutine(onComplete));
    }

    private IEnumerator LoadLocalizationRoutine(System.Action onComplete)
    {
        Debug.Log("구글 시트 로컬라이제이션 로드 시작...");
        using var request = UnityWebRequest.Get(sheetURL);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"시트 로드 실패: {request.error}");
            yield break;
        }

        ProcessTSV(request.downloadHandler.text);
        onComplete?.Invoke();
    }

    private void ProcessTSV(string tsv)
    {
        var koTable = LocalizationSettings.StringDatabase.GetTable(tableName, LocalizationSettings.AvailableLocales.GetLocale("ko")) as StringTable;
        var enTable = LocalizationSettings.StringDatabase.GetTable(tableName, LocalizationSettings.AvailableLocales.GetLocale("en")) as StringTable;

        if (koTable == null || enTable == null) return;

        string[] lines = tsv.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        
        for (int i = 4; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = lines[i].Split('\t');
            if (cols.Length < 4) continue;
            
            string key = cols[1].Trim();
            string engValue = cols[2].Trim();
            string korValue = cols[3].Trim();

            if (string.IsNullOrEmpty(key)) continue;

            koTable.AddEntry(key, korValue);
            enTable.AddEntry(key, engValue);
             Debug.Log($"[Localize] {key} 로드 완료: {korValue}");
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(koTable);
        UnityEditor.EditorUtility.SetDirty(enTable);
#endif
        Debug.Log("모든 로컬라이제이션 데이터 매칭 완료!");
    }
}