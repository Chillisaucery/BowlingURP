using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class GlobalMethods
{
    //Prefix
    public static readonly string NAME_PREFIX = "/name ";
    public static readonly string ROOM_PREFIX = "/room ";

    public static readonly string FORCE_PREFIX = "/force ";
    public static readonly string POSITION_PREFIX = "/position ";

    public static readonly string START_GAME_PREFIX = "/start ";
    public static readonly string PAUSE_PREFIX = "/pause ";
    public static readonly string PLAYER_CODE_PREFIX = "/playerCode ";

    public static readonly string RESET_THROW_PREFIX = "/resetThrow ";
    public static readonly string CHANGE_PLAYER_PREFIX = "/changePlayer ";
    public static readonly string CHANGE_ROUND_PREFIX = "/changeRound ";
    public static readonly string RESET_PINS_PREFIX = "/resetPins ";


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
