using UnityEngine;

public class TestTrigerForGold : MonoBehaviour
{
    [SerializeField] private TestGold _testGold;


    private void Awake()
    {
        _testGold = FindObjectOfType<TestGold>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _testGold.UseTestGold(100);
    }
}
