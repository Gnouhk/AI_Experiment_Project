using System.Collections;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    #region Members
    public float CaptureRadius = 3;
    public float RewardValue { get; set; }
    public float DistanceToPrevious { get; set; }
    
    // The accumulated distance in Unity units from the first to this checkpoint
    public float AccumulatedDistance { get; set; }
    public float AccumulatedReward { get; set; }

    #endregion

    #region Methods

    // Calculates the reward earned for the given distance to the checkpoint
    public float GetRewardValue(float currentDistance)
    {
        // Calculate how clase the distance is to captureing this check point, relative to the distance
        float completePerc = (DistanceToPrevious - currentDistance) / DistanceToPrevious;

        // Reward according to capture percentage
        if(completePerc < 0)
        {
            return 0;
        }
        else
        {
            return completePerc * RewardValue;
        }
    }

    #endregion
}
