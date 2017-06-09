using System;
using UnityEngine;

namespace SnakeBalls
{
  [AddComponentMenu("SnakeBalls/SpawnManager")]
  public class SpawnManager : MonoBehaviour
  {
    public int intervalHeightMin = 1;
    public int intervalHeightMax = 3;
    public float intervalBallProbability = 0.1f;
    public int minBallValue = 1;
    public int maxBallValue = 5;
    public int barragePeriodMin = 2;
    public int barragePeriodMax = 4;
    public int blockLineCountMin = 1;
    public int blockLineCountMax = 3;
    public int minBlockValue = 1;
    public int maxBlockValue = 20;
    public float minBarrageBlockValueFactor = 0.5f;
    public float minFirstAndLastBlockLineValueFactor = 0.25f;
    public float barProbability = 0.2f;
    public SpawnManager.StartingPhase startingPhase;
    public SpawnManager.SecondPhase secondPhase;
    private static SpawnManager instance;

    public static SpawnManager Instance
    {
      get
      {
        return SpawnManager.instance;
      }
    }

    private void Awake()
    {
      if ((UnityEngine.Object) SpawnManager.instance == (UnityEngine.Object) null)
      {
        SpawnManager.instance = this;
      }
      else
      {
        Debug.LogWarning((object) "A singleton can only be instantiated once!");
        UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
      }
    }

    private void OnDestroy()
    {
      if (!((UnityEngine.Object) SpawnManager.instance == (UnityEngine.Object) this))
        return;
      SpawnManager.instance = (SpawnManager) null;
    }

    [Serializable]
    public class StartingPhase
    {
      public int intervalHeight = 5;
      public int ballCount = 3;
    }

    [Serializable]
    public class SecondPhase
    {
      public int intervalHeight = 3;
    }
  }
}
