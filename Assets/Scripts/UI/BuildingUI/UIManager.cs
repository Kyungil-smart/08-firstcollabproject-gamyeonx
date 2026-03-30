using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public bool IsStop = false; // 일시정지 판정

    [Header("각 건물의 건축 버튼")]
    [SerializeField] private List<FurnitureButtonData> _furnitureButtons;
    [Header("메인 건축 버튼")]
    public GameObject _buildButton;

    [System.Serializable]
    public class FurnitureButtonData
    {
        public EFacilityType type;
        public GameObject button;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public void SetFurnitureButtonActive(EFacilityType type, bool active)
    {
        var data = _furnitureButtons.Find(x => x.type == type);
        if (data != null && data.button != null)
            data.button.SetActive(active);
    }
}
