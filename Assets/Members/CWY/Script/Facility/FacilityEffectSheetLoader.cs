using UnityEngine;


public class FacilityEffectSheetLoader : MonoBehaviour
{
    [Header("Sheet")]
    [SerializeField] private SheetData _facilityEffectSheet;

    [Header("Target Database")]
    [SerializeField] private FacilityEffectDatabaseSO _facilityEffectDatabase;

    [Header("Sheet Row Settings")]
    [Tooltip("데이터 시작 줄 인덱스. 예: 1이면 두 번째 줄부터 읽음")]
    [SerializeField] private int _startRowIndex = 1;

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(_facilityEffectSheet.Url))
        {
            return;
        }

        if (_facilityEffectDatabase == null)
        {
            return;
        }

        StartCoroutine(_facilityEffectSheet.Load(SetFacilityEffectDatas));
    }

    public void SetFacilityEffectDatas(char splitSymbol, string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        _facilityEffectDatabase.Clear();

        for (int i = _startRowIndex; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            string[] cols = lines[i].Split(splitSymbol);

            // 예상 컬럼 수:
            // 0 FacilityID
            // 1 EFacilityType
            // 2 HungerEffect
            // 3 ThirstEffect
            // 4 FatigueEffect
            // 5 CleanEffect
            // 6 SatisfactionEffect
            if (cols.Length < 6)
            {
                Debug.LogWarning($"[FacilityEffectSheetLoader] Invalid column count at line {i}. Line skipped.");
                continue;
            }

            for (int j = 0; j < cols.Length; j++)
            {
                cols[j] = cols[j].Trim();
            }

            FacilityEffectRow row = new FacilityEffectRow();
            row.SetData(cols);

            _facilityEffectDatabase.AddEffectRow(row);

        }

    }
}