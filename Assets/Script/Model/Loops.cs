using System.Collections;
using System;
using UnityEngine;

[Serializable]
public class Loops
{
    public Loop[] ballz;

    public Color getColor(int score)
    {
        Color color = Color.white;
        if (score >= ballz[ballz.Length - 1].maxScore)
        {
            int[] color1 = ballz[ballz.Length - 1].color;
            color = new Color(color1[0]/255f, color1[1]/255f, color1[2]/255f);
        }
        else
        {
            for (int i = 0; i < ballz.Length; i++)
            {
                if (score <= ballz[i].maxScore)
                {
                    float count = ballz[i].maxScore - ballz[i].minScore + 1;
                    int[] color1 = ballz[i].color;
                    int[] color2 = color1;

                    if (i < ballz.Length - 1)
                    {
                        color2 = ballz[i + 1].color;
                    }

                    float a = 1 / count * (score - ballz[i].minScore);
                    color = new Color((color1[0] * 1.0f + (color2[0] - color1[0]) * a) / 255.0f,
                        (color1[1] * 1.0f + (color2[1] - color1[1]) * a) / 255.0f,
                        (color1[2] * 1.0f + (color2[2] - color1[2]) * a) / 255.0f);

                    break;
                }
                else
                {
                    continue;
                }
            }
        }
        return color;
    }
}

[Serializable]
public class Loop 
{
    public int minScore;
    public int maxScore;
    public int[] color;
}

