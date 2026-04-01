using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EventDataLoader : MonoBehaviour
{
    private const string SheetURL = "https://docs.google.com/spreadsheets/d/1GOnAJjcQV4ZlfeWOjQxG4L6o8uYbqgEyCePhL-jTcSw/edit?gid=61961225#gid=61961225/export?format=csv";

    public IEnumerator LoadEvents(System.Action<List<EventData>> onComplete)
    {
        using var request = UnityWebRequest.Get(SheetURL);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"시트 로드 실패: {request.error}");
            yield break;
        }

        var events = ParseCSV(request.downloadHandler.text);
        onComplete?.Invoke(events);
    }

    private List<EventData> ParseCSV(string csv)
    {
        var result = new List<EventData>();
        var lines = csv.Split('\n');

        // 4 줄은 헤더
        for (int i = 4; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var cols = lines[i].Split(',');
            if (cols.Length < 5) continue;

            result.Add(new EventData
            {
                eventId = cols[1].Trim(),
                triggerType = cols[2].Trim(),
                triggerValue = int.TryParse(cols[3].Trim(), out int v) ? v : 0,
                actions = cols[4].Trim(),
                isOneTime = cols[5].Trim() == "TRUE"
            });
        }
        return result;
    }
}
