using UnityEngine;


public class GuestSheetLoader : MonoBehaviour
{
    [Header("Sheet")]
    [SerializeField] private SheetData _guestSheet;

    [Header("Target Database")]
    [SerializeField] private GuestDataDatabaseSO _guestDataDatabase;

    [Header("Sheet Row Settings")]
    [Tooltip("데이터 시작 줄 인덱스. 예: 1이면 두 번째 줄부터 읽음")]
    [SerializeField] private int _startRowIndex = 1;

    private void Start()
    {
        if(string.IsNullOrWhiteSpace(_guestSheet.Url))
        {
            Debug.LogError("[GuestSheetLoader] GuestSheet Url is missing.");
            return;
        }

        if(_guestDataDatabase == null)
        {
            Debug.LogError("[GuestSheetLoader] GuestDataDatabase is missing.");
            return;
        }

        StartCoroutine(_guestSheet.Load(SetGuestDatas));
    }

    public void SetGuestDatas(char splitSymbol, string[] lines)
    {
        if(lines == null || lines.Length == 0)
        {
            return;
        }

        _guestDataDatabase.Clear();

        for(int i = _startRowIndex; i < lines.Length; i++)
        {
            if(string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            string[] cols = lines[i].Split(splitSymbol);

            if(cols.Length < 4)
            {
                continue;
            }

            for(int j = 0; j < cols.Length; j++)
            {
                cols[j] = cols[j].Trim();
            }

            GuestDataRow row = new GuestDataRow();
            row.SetData(cols);

            _guestDataDatabase.AddRow(row);

        }

    }
}