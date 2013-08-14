    Shader "Bump/Specular Map" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _SpecMap ("SpecMap", 2D) = "white" {}
      _BumpMap ("Normalmap", 2D) = "bump" {}
	  _EmissiveMap ("Emissive Map", 2D) = "black" {}
	  _SpecularPower ("Power", Range (1.0,128.0)) = 32.0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
		LOD 1000
    CGPROGRAM
    #pragma surface surf ColoredSpecular
     
    struct MySurfaceOutput {
        half3 Albedo;
        half3 Normal;
        half3 Emission;
        half Specular;
        half3 GlossColor;
        half Alpha;
    };
     
     
    inline half4 LightingColoredSpecular (MySurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
    {
      half3 h = normalize (lightDir + viewDir);
     
      half diff = max (0, dot (s.Normal, lightDir));
     
      float nh = max (0, dot (s.Normal, h));
      float spec = pow (nh, 32.0);
      half3 specCol = spec * s.GlossColor;
     
      half4 c;
      c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * specCol) * (atten * 2);
      c.a = s.Alpha;
      return c;
    }
     
    inline half4 LightingColoredSpecular_PrePass (MySurfaceOutput s, half4 light)
    {
        half3 spec = light.a * s.GlossColor;
       
        half4 c;
        c.rgb = (s.Albedo * light.rgb + light.rgb * spec);
        c.a = s.Alpha + spec * _SpecColor.a;
        return c;
    }
     
     
    struct Input {
      float2 uv_MainTex;
      float2 uv_SpecMap;
      float2 uv_BumpMap;
	  float2 uv_EmissiveMap;
    };
    sampler2D _MainTex;
    sampler2D _SpecMap;
    sampler2D _BumpMap;
	sampler2D _EmissiveMap;
	float _SpecularPower;
     
    void surf (Input IN, inout MySurfaceOutput o)
    {
      o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
      half4 spec = tex2D (_SpecMap, IN.uv_SpecMap);
      o.GlossColor = spec.rgb;
      o.Specular = _SpecularPower / 128.0;
      o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	  o.Emission = tex2D (_MainTex, IN.uv_MainTex) * tex2D (_EmissiveMap, IN.uv_EmissiveMap).rgb + tex2D (_EmissiveMap, IN.uv_EmissiveMap).rgb;
    }
    ENDCG
    }
    Fallback "Diffuse"
    }