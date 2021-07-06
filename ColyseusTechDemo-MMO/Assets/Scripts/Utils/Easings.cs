using System;
using UnityEngine;

public class Easings
{
    public enum EaseType
    {
        None,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce
    }
    
    const float c1 = 1.70158f;
    const float n1 = 7.5625f;
    const float d1 = 2.75f;

    public static float Ease(float val, EaseType easeType)
    {
        switch (easeType)
        {
            case EaseType.None:
                return None(val);
            case EaseType.EaseInSine:
                return EaseInSine(val);
            case EaseType.EaseOutSine:
                return EaseOutSine(val);
            case EaseType.EaseInOutSine:
                return EaseInOutSine(val);
            case EaseType.EaseInQuad:
                return EaseInQuad(val);
            case EaseType.EaseOutQuad:
                return EaseOutQuad(val);
            case EaseType.EaseInOutQuad:
                return EaseInOutQuad(val);
            case EaseType.EaseInCubic:
                return EaseInCubic(val);
            case EaseType.EaseOutCubic:
                return EaseOutCubic(val);
            case EaseType.EaseInOutCubic:
                return EaseInOutCubic(val);
            case EaseType.EaseInQuart:
                return EaseInQuart(val);
            case EaseType.EaseOutQuart:
                return EaseOutQuart(val);
            case EaseType.EaseInOutQuart:
                return EaseInOutQuart(val);
            case EaseType.EaseInQuint:
                return EaseInQuint(val);
            case EaseType.EaseOutQuint:
                return EaseOutQuint(val);
            case EaseType.EaseInOutQuint:
                return EaseInOutQuint(val);
            case EaseType.EaseInExpo:
                return EaseInExpo(val);
            case EaseType.EaseOutExpo:
                return EaseOutExpo(val);
            case EaseType.EaseInOutExpo:
                return EaseInOutExpo(val);
            case EaseType.EaseInCirc:
                return EaseInCirc(val);
            case EaseType.EaseOutCirc:
                return EaseOutCirc(val);
            case EaseType.EaseInOutCirc:
                return EaseInOutCirc(val);
            case EaseType.EaseInBack:
                return EaseInBack(val);
            case EaseType.EaseOutBack:
                return EaseOutBack(val);
            case EaseType.EaseInOutBack:
                return EaseInOutBack(val);
            case EaseType.EaseInElastic:
                return EaseInElastic(val);
            case EaseType.EaseOutElastic:
                return EaseOutElastic(val);
            case EaseType.EaseInOutElastic:
                return EaseInOutElastic(val);
            case EaseType.EaseInBounce:
                return EaseInBounce(val);
            case EaseType.EaseOutBounce:
                return EaseOutBounce(val);
            case EaseType.EaseInOutBounce:
                return EaseInOutBounce(val);
            default:
                throw new ArgumentOutOfRangeException(nameof(easeType), easeType, null);
        }

    }

    public static float None(float val)
    {
        return Mathf.Clamp01(val);
    }

    public static float EaseInSine(float val)
    {
        return 1 - Mathf.Cos((val * Mathf.PI) / 2);
    }

    public static float EaseOutSine(float val)
    {
        return Mathf.Sin((val * Mathf.PI) / 2);
    }

    public static float EaseInOutSine(float val)
    {
        return -(Mathf.Cos(Mathf.PI * val) - 1) / 2;
    }

    public static float EaseInQuad(float val)
    {
        return val * val;
    }

    public static float EaseOutQuad(float x)
    {
        return 1 - (1 - x) * (1 - x);
    }

    public static float EaseInOutQuad(float x)
    {
        return x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;
    }

    public static float EaseInCubic(float x)
    {
        return x * x * x;
    }

    public static float EaseOutCubic(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3); 
    }

    public static float EaseInOutCubic(float x)
    {
        return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
    }

    public static float EaseInQuart(float x)
    {
        return x * x * x * x;
    }

    public static float EaseOutQuart(float x)
    {
        return 1 - Mathf.Pow(1 - x, 4);
    }

    public static float EaseInOutQuart(float x)
    {
        return x < 0.5 ? 8 * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 4) / 2;
    }

    public static float EaseInQuint(float x)
    {
        return x * x * x * x * x;
    }

    public static float EaseOutQuint(float x)
    {
        return 1 - Mathf.Pow(1 - x, 5);
    }

    public static float EaseInOutQuint(float x)
    {
        return x < 0.5 ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }

    public static float EaseInExpo(float x)
    {
        return x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);
    }

    public static float EaseOutExpo(float x)
    {
        return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
    }

    public static float EaseInOutExpo(float x)
    {
        return x == 0
            ? 0
            : x == 1
                ? 1
                : x < 0.5 ? Mathf.Pow(2, 20 * x - 10) / 2
                    : (2 - Mathf.Pow(2, -20 * x + 10)) / 2;
    }

    public static float EaseInCirc(float x)
    {
        return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));
    }

    public static float EaseOutCirc(float val)
    {
        return Mathf.Sqrt(1 - Mathf.Pow(val - 1, 2));
    }

    public static float EaseInOutCirc(float x)
    {
        return x < 0.5
            ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2
            : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2;
    }

    public static float EaseInBack(float x)
    {
        float c3 = c1 + 1;

        return c3 * x * x * x - c1 * x * x;
    }

    public static float EaseOutBack(float x)
    {
        float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    public static float EaseInOutBack(float x)
    {
        float c2 = c1 * 1.525f;

        return x < 0.5
            ? (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
            : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
    }

    public static float EaseInElastic(float x)
    {
        float c4 = (2 * Mathf.PI) / 3;

        return x == 0
            ? 0
            : x == 1
                ? 1
                : -Mathf.Pow(2, 10 * x - 10) * Mathf.Sin((x * 10 - 10.75f) * c4);
    }

    public static float EaseOutElastic(float x)
    {
        float c4 = (2 * Mathf.PI) / 3;

        return x == 0
            ? 0
            : x == 1
                ? 1
                : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;

    }

    public static float EaseInOutElastic(float x)
    {
        float c5 = (2 * Mathf.PI) / 4.5f;

        return x == 0
            ? 0
            : x == 1
                ? 1
                : x < 0.5
                    ? -(Mathf.Pow(2, 20 * x - 10) * Mathf.Sin((20 * x - 11.125f) * c5)) / 2
                    : (Mathf.Pow(2, -20 * x + 10) * Mathf.Sin((20 * x - 11.125f) * c5)) / 2 + 1;
    }

    public static float EaseInBounce(float x)
    {
        return 1 - EaseOutBounce(1 - x);
    }

    public static float EaseOutBounce(float x)
    {

        if (x < 1 / d1)
        {
            return n1 * x * x;
        }
        else if (x < 2 / d1)
        {
            return n1 * (x -= 1.5f / d1) * x + 0.75f;
        }
        else if (x < 2.5 / d1)
        {
            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        }
        else
        {
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }

    public static float EaseInOutBounce(float x)
    {
        return x < 0.5
            ? (1 - EaseOutBounce(1 - 2 * x)) / 2
            : (1 + EaseOutBounce(2 * x - 1)) / 2;
    }
}
