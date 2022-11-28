using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class TurnOnLight : MonoBehaviour
{
    [SerializeField]
    List<LightTimeline> lightTimelines = new List<LightTimeline>();

    Light light = null;

    float initialBrightness = 0;

    Coroutine turnOn_Corou = null;



    // Start is called before the first frame update
    void OnEnable()
    {
        if (light == null)
            light = GetComponent<Light>();

        if (initialBrightness == 0)
            initialBrightness = light.intensity;

        turnOn_Corou = StartCoroutine(TurnOn());
    }

    private void OnDisable()
    {
        if (turnOn_Corou != null)   
            StopCoroutine(turnOn_Corou);
    }

    IEnumerator TurnOn()
    {
        float previousTime = 0;

        foreach (LightTimeline lightData in lightTimelines)
        {
            light.intensity = lightData.Intensity * initialBrightness;

            yield return new WaitForSeconds(lightData.Time - previousTime);

            previousTime = lightData.Time;
        }

        light.intensity = initialBrightness;
    }
}

[System.Serializable]
public class LightTimeline
{
    [SerializeField]
    private float time = 0;

    [SerializeField, Range(0,1)]
    private float intensity = 0;

    public float Time { get => time; private set => time = value; }
    public float Intensity { get => intensity; private set => intensity = value; }
}
