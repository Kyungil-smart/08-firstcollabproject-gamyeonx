using System.Collections.Generic;
using UnityEngine;

public class GuestUtilityEvaluator
{
    public EGuestNeedType EvaluateHighestNeed(GuestStates guestStates)
    {
        if (guestStates == null)
        {
            return EGuestNeedType.None;
        }

        List<(EGuestNeedType needType, int value)> candidates = new List<(EGuestNeedType, int)>
        {
            (EGuestNeedType.Hunger, guestStates.Hunger),
            (EGuestNeedType.Thirst, guestStates.Thirst),
            (EGuestNeedType.Fatigue, guestStates.Fatigue)
        };

        if (guestStates.CanUseShop)
        {
            candidates.Add((EGuestNeedType.Shop, guestStates.ShopNeed));
        }

        if (guestStates.CanUseTraining)
        {
            candidates.Add((EGuestNeedType.Training, guestStates.TrainingNeed));
        }

        int highestValue = int.MinValue;
        List<EGuestNeedType> highestCandidates = new List<EGuestNeedType>();

        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].value > highestValue)
            {
                highestValue = candidates[i].value;
                highestCandidates.Clear();
                highestCandidates.Add(candidates[i].needType);
            }
            else if (candidates[i].value == highestValue)
            {
                highestCandidates.Add(candidates[i].needType);
            }
        }

        if (highestCandidates.Count == 0)
        {
            return EGuestNeedType.None;
        }

        EGuestNeedType selectedNeed = highestCandidates[Random.Range(0, highestCandidates.Count)];

        Debug.Log($"[GuestUtilityEvaluator] 최고 Need 선택 | Need={selectedNeed}, Value={highestValue}, Count={highestCandidates.Count}");
        return selectedNeed;
    }

    public EFacilityType EvaluateTargetFacilityType(EGuestNeedType highestNeed)
    {
        switch (highestNeed)
        {
            case EGuestNeedType.Hunger:
                return EFacilityType.Restaurant;
            case EGuestNeedType.Thirst:
                return EFacilityType.VendingMachine;
            case EGuestNeedType.Fatigue:
                return EFacilityType.Onsen;
            case EGuestNeedType.Shop:
                return EFacilityType.Shop;
            case EGuestNeedType.Training:
                return EFacilityType.TrainingGround;
            default:
                return EFacilityType.None;
        }
    }
}