using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum GAME_STATUS
{
    READY,
    START
}

class Constants
{
    public const string LOOP_COUNT = "LoopCount";
    public const string BEST_SCORE = "BestScore";
    public const string BEST_CLEAR_LV = "BestClearLv";
    public const int INCREASE_TIME = 1;
    public const int WATCH_VIDEO_COIN = 20;
    public const int RATE_SHIELD = 5;
    public const int RATE_COUNT = 5;
    public const int VIDEO_COUNTDOWN_TIME = 60 * 5;
}
