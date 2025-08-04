Shader "Custom/ShakeEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        [Header(Shake Settings)]
        _ShakeIntensity ("Shake Intensity", Range(0, 1)) = 0
        _ShakeSpeed ("Shake Speed", Range(0, 50)) = 10
        _ShakeDirection ("Shake Direction", Vector) = (1, 1, 0, 0)
        
        [Header(Advanced)]
        _ShakeType ("Shake Type", Range(0, 2)) = 0
        // 0: Random, 1: Sin Wave, 2: Perlin Noise
    }
    
    SubShader
    {
        Tags { "Queue"="Geometry" }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            
            float _ShakeIntensity;
            float _ShakeSpeed;
            float4 _ShakeDirection;
            float _ShakeType;
            
            // 简单的噪声函数
            float noise(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // Perlin噪声近似
            float perlinNoise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                
                float4 worldPos = v.vertex;
                
                // 计算抖动偏移
                float2 shake = float2(0, 0);
                
                if (_ShakeIntensity > 0)
                {
                    float time = _Time.y * _ShakeSpeed;
                    
                    if (_ShakeType < 0.5) // Random
                    {
                        shake.x = (noise(float2(time * 1.1, 0)) - 0.5) * 2;
                        shake.y = (noise(float2(time * 1.3, 1)) - 0.5) * 2;
                    }
                    else if (_ShakeType < 1.5) // Sin Wave
                    {
                        shake.x = sin(time * 1.1) * cos(time * 0.7);
                        shake.y = cos(time * 1.3) * sin(time * 0.9);
                    }
                    else // Perlin Noise
                    {
                        shake.x = (perlinNoise(float2(time * 0.1, 0)) - 0.5) * 2;
                        shake.y = (perlinNoise(float2(time * 0.1, 1)) - 0.5) * 2;
                    }
                    
                    // 应用方向和强度
                    shake *= _ShakeIntensity * _ShakeDirection.xy;
                    worldPos.xy += shake * 0.1; // 调整系数
                }
                
                o.vertex = UnityObjectToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                return col;
            }
            ENDCG
        }
    }
}