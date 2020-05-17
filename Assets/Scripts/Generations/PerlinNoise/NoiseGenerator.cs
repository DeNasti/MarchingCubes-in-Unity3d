using System;
using System.Collections.Generic;
using UnityEngine;


public class NoiseGenerator
{

    Dictionary<string, float> _cache = new Dictionary<string, float>();

    public float Scale { get; set; }

    public float amplifier = 10;


    public NoiseGenerator(float scale)
    {
        Scale = scale;
    }

    public float GetPerlinNoise3D(float x, float y, float z)
    {
        x /= Scale;
        y /= Scale;
        z /= Scale;

        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);

        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        float abc = ab + bc + ac + ba + cb + ca;
        return (abc / 6f);
    }


    public float Get3dNoise(Vector3 coordinates) //, int octaves, float persistence)
    {
        var key = $"{coordinates.x}_{coordinates.y}_{coordinates.z}"; // _{octaves}_{persistence}";

        if (_cache.TryGetValue(key, out var outVal))
        {
            return outVal;
        }

        var pn = GetPerlinNoise3D(coordinates.x, coordinates.y, coordinates.z );

        _cache.Add(key, pn);
        return pn;
    }
    


    public float OctavePerlin(float x, float y, float z, int octaves, float persistence)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
        for (int i = 0; i < octaves; i++)
        {
            total += GetPerlinNoise3D(x * frequency, y * frequency, z * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }

    public void ClearCache()
    {
        _cache = new Dictionary<string, float>();
    }

}
