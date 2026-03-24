using TMPro;
using UnityEngine;

public class TestGold : MonoBehaviour
{
    public int testGold;
    public float speed = 5f;

    [SerializeField] private Transform _targetObj; 
    [SerializeField] private TMP_Text _testGoldText; 
    private Rigidbody2D rb;

    public int TTestGold
    {
        get => testGold;
        set => testGold = value;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        UpdateTestGold();
    }

    private void FixedUpdate()
    {
        if (_targetObj == null) return;

        Vector2 direction = (_targetObj.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }

    public void UpdateTestGold()
    {
        if (_testGoldText != null)
            _testGoldText.text = "Gold : " + testGold.ToString();
    }

    public void UseTestGold(int value)
    {
        testGold -= value;
        UpdateTestGold();
    }
}