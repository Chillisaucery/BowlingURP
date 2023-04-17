using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class GlobalMethods
{
    public static readonly Regex nonAlphanumericRegex = new Regex("[^a-zA-Z0-9 ]+");

    public static IEnumerator DelayedInvoke(float delay, Action action)
    {

        yield return new WaitForSeconds(delay);

        action();
    }

    public static IEnumerator RepeatedInvoke(float delay, float interval, Action action)
    {
        yield return new WaitForSeconds(delay);

        while (true)
        {
            yield return new WaitForSeconds(interval);

            action();
        }
    }

    public static string ClearString(string str)
    {
        return nonAlphanumericRegex.Replace(str, "");
    }
}
