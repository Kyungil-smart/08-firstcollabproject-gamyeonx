using UnityEngine;
using System.Collections.Generic;

public class GuestPoolManager : MonoBehaviour
{
    public static GuestPoolManager Instance { get; private set; }

    [Header("오브젝트 풀링 설정")]
    [SerializeField] private GameObject _guestPrefab;
    [SerializeField] private int _initialPoolSize = 80;
    [SerializeField] private int _expandCount = 20;

    private readonly Queue<GameObject> _guestPool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        if (_guestPrefab == null)
        {
            Debug.LogError("[GuestPoolManager] Guest Prefab이 비어 있습니다.");
            return;
        }

        CreateGuest(_initialPoolSize);
    }

    private void CreateGuest(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject guest = Instantiate(_guestPrefab, transform);
            guest.SetActive(false);
            _guestPool.Enqueue(guest);
        }
    }

    public GameObject GetGuest(Vector3 position, Quaternion rotation)
    {
        if (_guestPrefab == null)
        {
            return null;
        }

        if (_guestPool.Count == 0)
        {
            CreateGuest(_expandCount);
        }

        GameObject guest = _guestPool.Dequeue();

        if (guest == null)
        {
            return null;
        }

        //guest.transform.SetParent(null);
        guest.transform.SetPositionAndRotation(position, rotation);
        guest.SetActive(true);

        return guest;
    }

    public void ReturnGuest(GameObject guest)
    {
        if (guest == null)
        {
            return;
        }

        guest.transform.SetParent(transform);
        guest.SetActive(false);
        _guestPool.Enqueue(guest);
    }
}