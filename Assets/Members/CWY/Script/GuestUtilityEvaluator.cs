using UnityEngine;

public class GuestUtilityEvaluator
{
    /// <summary>
    /// 현재 손님 상태를 보고 가장 급한 욕구를 찾음
    /// </summary>
    public EGuestNeedType EvaluateHighestNeed(GuestStates guestStates)
    {
        if (guestStates == null)
        {
            Debug.LogWarning("[GuestUtilityEvaluator] EvaluateHighestNeed failed. GuestStates is null.");
            return EGuestNeedType.None;
        }

        int hungerScore = guestStates.GetHungerScore();
        int thirstScore = guestStates.GetThirstScore();
        int fatigueScore = guestStates.GetFatigueScore();
        int cleanlinessScore = guestStates.GetCleanlinessScore();

        int highestScore = -1;
        EGuestNeedType highestNeed = EGuestNeedType.None;

        if (hungerScore > highestScore)
        {
            highestScore = hungerScore;
            highestNeed = EGuestNeedType.Hunger;
        }

        if (thirstScore > highestScore)
        {
            highestScore = thirstScore;
            highestNeed = EGuestNeedType.Thirst;
        }

        if (fatigueScore > highestScore)
        {
            highestScore = fatigueScore;
            highestNeed = EGuestNeedType.Fatigue;
        }

        if (cleanlinessScore > highestScore)
        {
            highestScore = cleanlinessScore;
            highestNeed = EGuestNeedType.Cleanliness;
        }

        Debug.Log($"[GuestUtilityEvaluator] HighestNeed = {highestNeed}, Score = {highestScore}");
        return highestNeed;
    }

    /// <summary>
    /// 가장 급한 욕구를 시설 타입으로 변환
    /// </summary>
    public EFacilityType EvaluateTargetFacilityType(GuestStates guestStates)
    {
        EGuestNeedType highestNeed = EvaluateHighestNeed(guestStates);

        switch (highestNeed)
        {
            case EGuestNeedType.Hunger:
                return EFacilityType.Food;

            case EGuestNeedType.Thirst:
                return EFacilityType.Drink;

            case EGuestNeedType.Fatigue:
                return EFacilityType.Rest;

            case EGuestNeedType.Cleanliness:
                return EFacilityType.Clean;

            default:
                return EFacilityType.None;
        }
    }

    /// <summary>
    /// 디버그 확인용 문자열
    /// </summary>
    public string GetDebugEvaluationText(GuestStates guestStates)
    {
        if (guestStates == null)
        {
            return "[GuestUtilityEvaluator] GuestStates is null.";
        }

        int hungerScore = guestStates.GetHungerScore();
        int thirstScore = guestStates.GetThirstScore();
        int fatigueScore = guestStates.GetFatigueScore();
        int cleanlinessScore = guestStates.GetCleanlinessScore();

        EGuestNeedType highestNeed = EvaluateHighestNeed(guestStates);
        EFacilityType targetFacilityType = EvaluateTargetFacilityType(guestStates);

        return $"Hunger={hungerScore}, Thirst={thirstScore}, Fatigue={fatigueScore}, Cleanliness={cleanlinessScore}, HighestNeed={highestNeed}, TargetFacilityType={targetFacilityType}";
    }
}