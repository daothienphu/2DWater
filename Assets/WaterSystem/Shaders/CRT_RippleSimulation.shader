Shader "WaterSystem/RippleSimulation"
{
    Properties
    {
        _Spread("Spread", Range(0.0, 0.5)) = 0.2
        _Damping("Damping", Range(0.98, 1.0)) = 0.995
        _DeltaUV("Delta UV", Float) = 1
        _ImpactHeight("Initial Impact Height", Range(0.0, 1.0)) = 1
    }

    CGINCLUDE

    #include "UnityCustomRenderTexture.cginc"

    half _Spread;
    half _Damping;
    float _DeltaUV;
    float _ImpactHeight;

    float4 frag(v2f_customrendertexture i) : SV_Target
    {    
        float2 uv = i.globalTexcoord;
        
        float du = 1.0 / _CustomRenderTextureWidth;
        float dv = 1.0 / _CustomRenderTextureHeight;
        float3 duv = float3(du, dv, 0) * _DeltaUV;
        
        float2 prevHeight = tex2D(_SelfTexture2D, uv);
        
        float diff = _Spread * (
            tex2D(_SelfTexture2D, uv - duv.zy).r +
            tex2D(_SelfTexture2D, uv + duv.zy).r +
            tex2D(_SelfTexture2D, uv - duv.xz).r +
            tex2D(_SelfTexture2D, uv + duv.xz).r - 4 * prevHeight.r);

        float currHeight = (2 * prevHeight.r - prevHeight.g + diff) * _Damping;
        
        return float4(currHeight, prevHeight.r, 0, 0);
    }

    float4 frag_up_impact(v2f_customrendertexture _) : SV_Target
    {
        return float4(_ImpactHeight, 0, 0, 0);
    }
    
    float4 frag_down_impact(v2f_customrendertexture _) : SV_Target
    {
        return float4(-_ImpactHeight, 0, 0, 0);
    }
    
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Update"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            Name "Impact"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag_up_impact
            ENDCG
        }

        Pass
        {
            Name "Impact"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag_down_impact
            ENDCG
        }
    }
}