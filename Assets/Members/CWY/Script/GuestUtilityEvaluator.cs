using System.Collections.Generic;
using UnityEngine;

public class GuestUtilityEvaluator
{
    public EFacilityType EvaluateTargetFacilityType(GuestStates guestStates)
    {
        if (guestStates == null)
        {
            return EFacilityType.None;
        }

        int hunger = guestStates.Hunger;
        int thirst = guestStates.Thirst;
        int fatigue = guestStates.Fatigue;

        int highestValue = Mathf.Max(hunger, thirst, fatigue);
        List<EGuestNeedType> candidates = new List<EGuestNeedType>();

        if (hunger == highestValue) candidates.Add(EGuestNeedType.Hunger);
        if (thirst == highestValue) candidates.Add(EGuestNeedType.Thirst);
        if (fatigue == highestValue) candidates.Add(EGuestNeedType.Fatigue);

        if (candidates.Count == 0)
        {
            return EFacilityType.None;
        }

        EGuestNeedType selectedNeed = candidates[Random.Range(0, candidates.Count)];

        Debug.Log($"[GuestUtilityEvaluator] 최고 상태 선택 | Need={selectedNeed}, Value={highestValue}, CandidateCount={candidates.Count}");

        switch (selectedNeed)
        {
            case EGuestNeedType.Hunger:
                return EFacilityType.Restaurant;
            case EGuestNeedType.Thirst:
                return EFacilityType.VendingMachine;
            case EGuestNeedType.Fatigue:
                return EFacilityType.HotSpring;
            default:
                return EFacilityType.None;
        }
    }
}