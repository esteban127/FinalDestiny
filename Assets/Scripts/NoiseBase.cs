using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum NInterpolation
{
	Linear,
	Cosine,
	Cubic
}

public class NoiseBase
{
	public long Seed;
	public double SeedF;
	public NInterpolation Interpolation = NInterpolation.Linear;
	public int heightMultplier = 0;
	public int heightAddition = 0;
    public int Frequency = 16;
	public bool ClampChunk = false;
	public int Smooth = 10;
    public float MaxPerlinHeight = float.MinValue;
    public float MinPerlinHeight = float.MaxValue;
    public Vector2 OctavesRange = new Vector2(-100000.0f, 100000.0f);
    public Vector2 OctavesOffset = new Vector2(0.0f, 0.0f);
	private int ChunkIndex = 0;
    private float halfWidth;
    private float halfHeight;
    private System.Random Rand;

	public NoiseBase(){
		Seed = 0;
		SeedF = 0;
        Rand = new System.Random();   
	}

	public NoiseBase(long Seed){
		this.Seed = Seed;
        Rand = new System.Random((int)Seed);
	}
    public NoiseBase(double SeedF)
    {
		this.SeedF = SeedF;
        Rand = new System.Random((int)SeedF);
	}

	private float Random(int x,int Range)
	{
		return (int)((x+Seed)^5) % Range;
	}

	//Perlin Noise por rango y totalmente aleatorio:
	//x = posicion en el valor de x.
	//range= el maximo rango que el podriamos generar
	public int GetNoise(int x,int Range)
	{
		float noise = 0.0f;

		if (ClampChunk) 
		{
			Range /= 2;
            while (Frequency > 0) 
			{
                ChunkIndex = (x / Frequency);
                float prog = (x % Frequency) / (Frequency * 1f);
				float left_random = Random (ChunkIndex, Range);
				float right_radom = Random (ChunkIndex + 1, Range);

				switch (Interpolation) 
				{
				case NInterpolation.Linear:
					//				x			a		x			b	
					noise += (1 - prog) * left_random + prog * right_radom;
					break;

				case NInterpolation.Cosine:
					float ft = (prog * Mathf.PI);
					float f = (1 - Mathf.Cos (ft)) * 0.5f;

					noise += (1 - f)* left_random + right_radom * f;
					break;
				case NInterpolation.Cubic:
					noise = 0.0f;
					break;
				}

                Frequency /= 2;
				Range /= 2;
				Range = Mathf.Max (1, Range);
			}
		} 
		else 
		{
            ChunkIndex = (x / Frequency);
            float prog = (x % Frequency) / (Frequency * 1f);
			float left_random = Random (ChunkIndex, Range);
			float right_radom = Random (ChunkIndex + 1, Range);

			switch (Interpolation)
			{
			case NInterpolation.Linear:
				noise = (1 - prog) * left_random + prog * right_radom;
				break;

			case NInterpolation.Cosine:
				float ft = (prog * Mathf.PI);
				float f = (1 - Mathf.Cos (ft)) * 0.5f;

				noise = (1 - f)* left_random + right_radom * f;
				break;
			case NInterpolation.Cubic:
				noise = 0.0f;
				break;
			}
		}

		return (int)noise + heightAddition;
	}

	public float GetNoise (float x,float y,float w,float h){
		return Mathf.PerlinNoise (x/w, y/h);
	}

    public float[,] GetNoise(int width,int height,float scale,int octaves = 1,float persistence = 1,float lacunarity = 1) 
    {
        float value = 0;
        float[,] noiseMap = new float[width,height];
        if (scale <= 0) scale = 1.0f;

        System.Random rand = new System.Random((int)Seed);
        Vector2[] octavesOffset = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offsetX = (float)Utils.FRandom((float)rand.NextDouble(), (float)OctavesRange.x, (float)OctavesRange.y) + OctavesOffset.x;
            float offsetY = (float)Utils.FRandom((float)rand.NextDouble(), (float)OctavesRange.x, (float)OctavesRange.y) + OctavesOffset.y;
            octavesOffset[i] = new Vector2(offsetX, offsetY);
        }

        halfWidth = width / 2.0f;
        halfHeight = height / 2.0f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float frequency = 1;
                    float amplitude = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        value = Mathf.PerlinNoise(
                            (x-halfWidth) / scale * frequency + octavesOffset[i].x,
                            (y-halfHeight) / scale * frequency + octavesOffset[i].y
                        ) * 2 - 1;

                        noiseHeight += value * amplitude;
                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > MaxPerlinHeight)
                        MaxPerlinHeight = noiseHeight;
                    else if (noiseHeight < MinPerlinHeight)
                        MinPerlinHeight = noiseHeight;

                    noiseMap[x, y] = noiseHeight;
                }
            }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(MinPerlinHeight,MaxPerlinHeight,noiseMap[x,y]);
            }
        }


        return noiseMap;
    }

	//xy = Posicion del recorrido.
	public int GetNoise(float x)
	{
		float Noise = Mathf.PerlinNoise ((float)SeedF, (float)(x / Smooth));
		return Mathf.RoundToInt (Noise * heightMultplier) + heightAddition;
	}

    public int GetWhiteNoise(bool clamp = true) 
    {
        int value = Rand.Next(256);
        if (clamp)
            return (value < 128) ? 0 : 255;
        else
            return value;
    }
}
