using System.Collections.Generic;
using UnityEngine;

public class GuestUtilityEvaluator
{
    private struct NeedFacilityCandidate
    {
        public EGuestNeedType NeedType;
        public EFacilityType FacilityType;
        public int Value;

        public NeedFacilityCandidate(EGuestNeedType needType, EFacilityType facilityType, int value)
        {
            NeedType = needType;
            FacilityType = facilityType;
            Value = value;
        }
    }

    public bool TryGetBestAvailableFacility(
        GuestController controller,
        FacilityRegistry facilityRegistry,
        out EGuestNeedType selectedNeedType,
        out FacilityRuntime selectedFacility)
    {
        selectedNeedType = EGuestNeedType.None;
        selectedFacility = null;

        if (controller == null || controller.GuestStates == null)
        {
            return false;
        }

        if (facilityRegistry == null)
        {
            Debug.LogWarning("[GuestUtilityEvaluator] FacilityRegistry가 없습니다.");
            return false;
        }

        List<NeedFacilityCandidate> candidates = BuildCandidates(controller.GuestStates);

        if (candidates.Count == 0)
        {
            return false;
        }

        candidates.Sort((a, b) => b.Value.CompareTo(a.Value));

        int index = 0;

        while (index < candidates.Count)
        {
            int currentValue = candidates[index].Value;
            List<NeedFacilityCandidate> sameValueGroup = new List<NeedFacilityCandidate>();

            while (index < candidates.Count && candidates[index].Value == currentValue)
            {
                sameValueGroup.Add(candidates[index]);
                index++;
            }

            List<(EGuestNeedType needType, FacilityRuntime facility)> availableFacilities =
                new List<(EGuestNeedType, FacilityRuntime)>();

            for (int i = 0; i < sameValueGroup.Count; i++)
            {
                NeedFacilityCandidate candidate = sameValueGroup[i];

                FacilityRuntime facility = facilityRegistry.GetFirstFacilityByType(candidate.FacilityType);

                if (facility == null)
                {
                    continue;
                }

                if (!controller.CanSelectFacilityType(candidate.FacilityType))
                {
                    continue;
                }

                availableFacilities.Add((candidate.NeedType, facility));
            }

            if (availableFacilities.Count > 0)
            {
                int randomIndex = Random.Range(0, availableFacilities.Count);
                selectedNeedType = availableFacilities[randomIndex].needType;
                selectedFacility = availableFacilities[randomIndex].facility;

                Debug.Log(
                    $"[GuestUtilityEvaluator] 시설 선택 완료 | " +
                    $"Need={selectedNeedType}, FacilityID={selectedFacility.FacilityID}, " +
                    $"Value={currentValue}, CandidateCount={availableFacilities.Count}");

                return true;
            }

            Debug.Log($"[GuestUtilityEvaluator] Value={currentValue} 그룹에서 선택 가능한 시설이 없어 다음 우선순위 검사");
        }

        Debug.Log("[GuestUtilityEvaluator] 설치된 시설 또는 잠금 해제된 시설이 없어 선택 실패");
        return false;
    }

    private List<NeedFacilityCandidate> BuildCandidates(GuestStates guestStates)
    {
        List<NeedFacilityCandidate> candidates = new List<NeedFacilityCandidate>
        {
            new NeedFacilityCandidate(EGuestNeedType.Hunger, EFacilityType.Restaurant, guestStates.Hunger),
            new NeedFacilityCandidate(EGuestNeedType.Thirst, EFacilityType.Cafe, guestStates.Thirst),
            new NeedFacilityCandidate(EGuestNeedType.Fatigue, EFacilityType.Onsen, guestStates.Fatigue)
        };

        if (guestStates.CanUseShop)
        {
            candidates.Add(new NeedFacilityCandidate(
                EGuestNeedType.Shop,
                EFacilityType.Shop,
                guestStates.ShopNeed));
        }

        if (guestStates.CanUseTraining)
        {
            candidates.Add(new NeedFacilityCandidate(
                EGuestNeedType.Training,
                EFacilityType.TrainingGround,
                guestStates.TrainingNeed));
        }

        return candidates;
    }
}