using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GuestDataDatabase", menuName = "Game/GuestDataDatabase")]
public class GuestDataDatabaseSO : ScriptableObject
{
    [SerializeField] private List<GuestDataRow> _guestDataRows = new List<GuestDataRow>();

    public IReadOnlyList<GuestDataRow> GuestDataRows => _guestDataRows;


    public void Clear()
    {
        _guestDataRows.Clear();
    }


    public void AddRow(GuestDataRow row)
    {
        if (row == null)
        {
            return;
        }

        _guestDataRows.Add(row);
    }


    public GuestDataRow GetGuestDataByVisitorID(int visitorID)
    {
        for (int i = 0; i < _guestDataRows.Count; i++)
        {
            GuestDataRow row = _guestDataRows[i];

            if (row == null)
            {
                continue;
            }

            if (row.VisitorID == visitorID)
            {
                return row;
            }
        }

        return null;
    }
}