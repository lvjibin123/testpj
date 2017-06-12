using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SnakeBalls/Spawner")]
public class Spawner : MonoBehaviour
{
    public float windowTopOffset = 5f;
    public float windowBottomOffset = -5f;
    public float beginLineBottom = 5f;
    public int column = 5;
    public Vector2 cellSize = new Vector2(1.115f, 1.115f);
    public float barWidth = 0.006f;

    private HashSet<Spawner.Coordinates> startingPhaseBallCoordinates = new HashSet<Spawner.Coordinates>();
    private int currentLine;
    private int currentIntervalHeight;
    private int lineRemainingBeforeNextBlockLine;
    private int blockLineRemainingBeforeNextBarrage;
    private int currentBlockLineCountBetweenBarrage;
    private bool startingPhase;
    private bool secondPhase;
    private float startOffset;
    private static Spawner instance;

    GameController gameController;

    public float getLineHeight(int lineCount) {
        return (currentLine + lineCount) * cellSize.y;
    }

    void Awake()
    {
        gameController = GameObject.Find("Game").GetComponent<GameController>();
        if ((Object)Spawner.instance == (Object)null)
        {
            Spawner.instance = this;
        }
        else
        {
            Debug.LogWarning((object)"A singleton can only be instantiated once!");
            Object.Destroy((Object)this.gameObject);
        }

    }

    public static Spawner Instance
    {
        get
        {
            return Spawner.instance;
        }
    }

    public float Bottom
    {
        get
        {
            return this.transform.position.y + this.windowBottomOffset;
        }
    }

    public void OnLoadGame()
    {
        this.startOffset = this.transform.position.y;
        this.currentLine = 0;
        SpawnManager instance = SpawnManager.Instance;
        this.SelectNextIntervalHeight(instance.startingPhase.intervalHeight);
        this.blockLineRemainingBeforeNextBarrage = 0;
        this.currentBlockLineCountBetweenBarrage = 0;
        this.startingPhase = true;
        this.secondPhase = false;
        int ballCount = instance.startingPhase.ballCount;
        int intervalHeight = instance.startingPhase.intervalHeight;
        List<Spawner.Coordinates> coordinatesList = new List<Spawner.Coordinates>();
        for (int i = 0; i < intervalHeight; ++i)
        {
            for (int j = 0; j < this.column; ++j)
                coordinatesList.Add(new Spawner.Coordinates(i, j));
        }
        this.startingPhaseBallCoordinates.Clear();
        for (int index1 = 0; index1 < ballCount; ++index1)
        {
            int index2 = Random.Range(0, coordinatesList.Count);
            this.startingPhaseBallCoordinates.Add(coordinatesList[index2]);
            coordinatesList.RemoveAt(index2);
        }
    }

    void OnAwakeEnd()
    {
        if (!((Object)Spawner.instance == (Object)this))
            return;
        Spawner.instance = (Spawner)null;
    }

    private void Update()
    {
        if (gameController.getGameStatus() != GAME_STATUS.START)
            return;

        SpawnManager instance = SpawnManager.Instance;
        float num1 = this.transform.position.y + this.windowTopOffset;
        while (true)
        {
            float num3 = (float)((double)this.beginLineBottom + (double)this.startOffset + (double)this.currentLine * (double)this.cellSize.y);

            if ((double)num1 >= (double)num3)
            {
                // 生成新元素的高度
                float num4 = num3 + this.cellSize.y * 0.5f;
                bool flag1 = false;
                bool flag2 = false;
                --this.lineRemainingBeforeNextBlockLine;
                if (this.lineRemainingBeforeNextBlockLine <= 0)
                {
                    flag1 = true;
                    this.SelectNextIntervalHeight();
                    --this.blockLineRemainingBeforeNextBarrage;
                    if (this.blockLineRemainingBeforeNextBarrage <= 0)
                    {
                        flag2 = true;
                        this.SelectNextBarrage();
                    }
                }
                HashSet<int> intSet = new HashSet<int>();
                if (flag1 && !flag2)
                {
                    int num5 = Random.Range(instance.blockLineCountMin, instance.blockLineCountMax + 1);
                    List<int> intList = new List<int>();
                    for (int index = 0; index < this.column; ++index)
                        intList.Add(index);
                    for (int index1 = 0; index1 < num5; ++index1)
                    {
                        int index2 = Random.Range(0, intList.Count);
                        int num6 = intList[index2];
                        intSet.Add(num6);
                        intList.RemoveAt(index2);
                        intList.Remove(num6 - 1);
                        intList.Remove(num6 + 1);
                        if (intList.Count <= 0)
                            break;
                    }
                }
                bool flag3 = false;
                int num7 = -1;
                if (flag2)
                    num7 = Random.Range(0, this.column);
                else if (flag1 && (this.blockLineRemainingBeforeNextBarrage == 1 || this.currentBlockLineCountBetweenBarrage - this.blockLineRemainingBeforeNextBarrage == 0))
                    flag3 = true;
                for (int j = 0; j < this.column; ++j)
                {
                    Vector3 zero = Vector3.zero;
                    zero.x = (float)(j - 2) * this.cellSize.x;
                    zero.y = num4;
                    bool flag4 = false;
                    bool flag5 = false;
                    if (flag2)
                        flag4 = true;
                    else if (this.startingPhase)
                    {
                        if (this.startingPhaseBallCoordinates.Contains(new Spawner.Coordinates(this.currentLine, j)))
                            flag5 = true;
                    }
                    else if (!this.secondPhase)
                    {
                        if (flag1)
                        {
                            if (intSet.Contains(j))
                                flag4 = true;
                            else if ((double)Random.Range(0.0f, 1f) <= (double)instance.intervalBallProbability)
                                flag5 = true;
                        }
                        else if ((double)Random.Range(0.0f, 1f) <= (double)instance.intervalBallProbability)
                            flag5 = true;
                    }
                    if (flag4)
                    {
                        int num5 = instance.minBlockValue;
                        int a = instance.maxBlockValue;
                        if (flag2 && num7 == j || (flag3 || this.startingPhase))
                        {
                            int b = (int)((double)gameController.getBallCount() * (!flag3 ? (double)instance.minBarrageBlockValueFactor : (double)instance.minFirstAndLastBlockLineValueFactor));
                            num5 = Mathf.Min(num5, b);
                            a = Mathf.Min(a, b);
                        }
                        if (a > 0)
                        {
                            // flag2 == true  is wall
                            gameController.initBrick(Random.Range(num5, a + 1), zero);
                        }
                    }
                    if (flag5 && !this.secondPhase)
                    {
                        gameController.initGainBall(zero, Random.Range(instance.minBallValue, instance.maxBallValue + 1));
                    }
                    if (flag1 && !this.startingPhase && (!this.secondPhase && j < this.column - 1) && (double)Random.Range(0.0f, 1f) <= (double)instance.barProbability)
                    {
                        zero = zero + new Vector3(0.5f * this.cellSize.x + barWidth, (currentIntervalHeight + 1) * 0.5f * this.cellSize.y, 0);
                        if (this.currentIntervalHeight == 2)
                        {
                            gameController.initBar2(zero);
                        }
                        else
                        {
                            gameController.initBar(zero);
                        }
                    }
                }
                if (this.startingPhase)
                {
                    if (flag2)
                    {
                        this.startingPhase = false;
                        this.secondPhase = true;
                        this.SelectNextIntervalHeight(instance.secondPhase.intervalHeight);
                    }
                }
                else if (this.secondPhase && flag1)
                    this.secondPhase = false;
                ++this.currentLine;
            }
            else
                break;
        }
    }

    private void SelectNextBarrage()
    {
        SpawnManager instance = SpawnManager.Instance;
        this.currentBlockLineCountBetweenBarrage = Random.Range(instance.barragePeriodMin, instance.barragePeriodMax + 1);
        this.blockLineRemainingBeforeNextBarrage = this.currentBlockLineCountBetweenBarrage + 1;
    }

    private void SelectNextIntervalHeight()
    {
        SpawnManager instance = SpawnManager.Instance;
        this.SelectNextIntervalHeight(Random.Range(instance.intervalHeightMin, instance.intervalHeightMax + 1));
    }

    private void SelectNextIntervalHeight(int height)
    {
        this.currentIntervalHeight = height;
        this.lineRemainingBeforeNextBlockLine = this.currentIntervalHeight + 1;
    }

    public class Coordinates
    {
        private int i;
        private int j;

        public Coordinates(int i, int j)
        {
            this.i = i;
            this.j = j;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Spawner.Coordinates coordinates = obj as Spawner.Coordinates;
            if (coordinates == null || this.i != coordinates.i)
                return false;
            return this.j == coordinates.j;
        }

        public override int GetHashCode()
        {
            return this.i ^ this.j;
        }
    }
}
