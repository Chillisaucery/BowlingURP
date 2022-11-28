using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalMethods
{
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
}
