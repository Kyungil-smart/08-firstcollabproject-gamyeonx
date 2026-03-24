using UnityEngine;

public class Instantiate : MonoBehaviour
{
    public GameObject Prefab;
    public GameObject InstancePivot;
    public int CurrentInstance = 0;
    public float PivotDistance = 5f;

    public void InstantiateButton()
    {
        float X = InstancePivot.transform.position.x + (CurrentInstance * PivotDistance);
        float Y = InstancePivot.transform.position.y;
        Vector2 InstatiatePivot = new Vector2(X, Y);
        Instantiate(Prefab, InstatiatePivot, InstancePivot.transform.rotation);
        CurrentInstance++;
    }
}
