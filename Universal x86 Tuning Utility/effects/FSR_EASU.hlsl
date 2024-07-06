// FidelityFX-FSR EASU Channel

//!MAGPIE EFFECT
//!VERSION 2

//!TEXTURE
Texture2D INPUT;

//!SAMPLER
//!FILTER POINT
SamplerState sam;

//!PASS 1
//!IN INPUT
//!BLOCK_SIZE 16
//!NUM_THREADS 64

#define min3(a, b, c) min(a, min(b, c))
#define max3(a, b, c) max(a, max(b, c))

void FsrEasuTap(
    inout float3 aC,
    inout float aW,
    float2 off,
    float2 dir,
    float2 len,
    float lob,
    float clp,
    float3 c
) {
    float2 v = mul(off, float2x2(dir.x, dir.y, -dir.y, dir.x));
    v *= len;
    float d2 = min(dot(v, v), clp);
    float wB = 2.0f / 5.0f * d2 - 1;
    float wA = lob * d2 - 1;
    wB *= wB;
    wA *= wA;
    float w = 25.0f / 16.0f * wB - (25.0f / 16.0f - 1.0f);
    w *= wA;
    aC += c * w;
    aW += w;
}

void FsrEasuSet(
    inout float2 dir,
    inout float len,
    float2 pp,
    bool biS, bool biT, bool biU, bool biV,
    float lA, float lB, float lC, float lD, float lE
) {
    float w = (biS ? (1 - pp.x) * (1 - pp.y) : 0) +
        (biT ? pp.x * (1 - pp.y) : 0) +
        (biU ? (1 - pp.x) * pp.y : 0) +
        (biV ? pp.x * pp.y : 0);

    float dc = lD - lC;
    float cb = lC - lB;
    float lenX = rcp(max(abs(dc), abs(cb)));
    float dirX = lD - lB;
    dir.x += dirX * w;
    lenX = saturate(abs(dirX) * lenX);
    lenX *= lenX;
    len += lenX * w;

    float ec = lE - lC;
    float ca = lC - lA;
    float lenY = rcp(max(abs(ec), abs(ca)));
    float dirY = lE - lA;
    dir.y += dirY * w;
    lenY = saturate(abs(dirY) * lenY);
    lenY *= lenY;
    len += lenY * w;
}

float3 FsrEasuF(uint2 pos, float4 con0, float4 con1, float4 con2, float2 con3) {
    float2 pp = pos * con0.xy + con0.zw;
    float2 fp = floor(pp);
    pp -= fp;

    float2 p0 = fp * con1.xy + con1.zw;
    float2 p1 = p0 + con2.xy;
    float2 p2 = p0 + con2.zw;
    float2 p3 = p0 + con3;

    float4 bczzR = INPUT.GatherRed(sam, p0);
    float4 bczzG = INPUT.GatherGreen(sam, p0);
    float4 bczzB = INPUT.GatherBlue(sam, p0);
    float4 ijfeR = INPUT.GatherRed(sam, p1);
    float4 ijfeG = INPUT.GatherGreen(sam, p1);
    float4 ijfeB = INPUT.GatherBlue(sam, p1);
    float4 klhgR = INPUT.GatherRed(sam, p2);
    float4 klhgG = INPUT.GatherGreen(sam, p2);
    float4 klhgB = INPUT.GatherBlue(sam, p2);
    float4 zzonR = INPUT.GatherRed(sam, p3);
    float4 zzonG = INPUT.GatherGreen(sam, p3);
    float4 zzonB = INPUT.GatherBlue(sam, p3);

    float4 bczzL = bczzB * 0.5 + (bczzR * 0.5 + bczzG);
    float4 ijfeL = ijfeB * 0.5 + (ijfeR * 0.5 + ijfeG);
    float4 klhgL = klhgB * 0.5 + (klhgR * 0.5 + klhgG);
    float4 zzonL = zzonB * 0.5 + (zzonR * 0.5 + zzonG);

    float bL = bczzL.x, cL = bczzL.y, iL = ijfeL.x, jL = ijfeL.y;
    float fL = ijfeL.z, eL = ijfeL.w, kL = klhgL.x, lL = klhgL.y;
    float hL = klhgL.z, gL = klhgL.w, oL = zzonL.z, nL = zzonL.w;

    float2 dir = 0;
    float len = 0;
    FsrEasuSet(dir, len, pp, true, false, false, false, bL, eL, fL, gL, jL);
    FsrEasuSet(dir, len, pp, false, true, false, false, cL, fL, gL, hL, kL);
    FsrEasuSet(dir, len, pp, false, false, true, false, fL, iL, jL, kL, nL);
    FsrEasuSet(dir, len, pp, false, false, false, true, gL, jL, kL, lL, oL);

    float2 dir2 = dir * dir;
    float dirR = dir2.x + dir2.y;
    bool zro = dirR < 1.0f / 32768.0f;
    dirR = rsqrt(dirR);
    dirR = zro ? 1 : dirR;
    dir.x = zro ? 1 : dir.x;
    dir *= dirR;

    len = len * 0.5;
    len *= len;
    float stretch = dot(dir, dir) * rcp(max(abs(dir.x), abs(dir.y)));
    float2 len2 = { 1 + (stretch - 1) * len, 1 - 0.5 * len };
    float lob = 0.5 + ((0.25 - 0.04) - 0.5) * len;
    float clp = rcp(lob);

    float3 min4 = min(min3(float3(ijfeR.z, ijfeG.z, ijfeB.z), float3(klhgR.w, klhgG.w, klhgB.w), float3(ijfeR.y, ijfeG.y, ijfeB.y)),
        float3(klhgR.x, klhgG.x, klhgB.x));
    float3 max4 = max(max3(float3(ijfeR.z, ijfeG.z, ijfeB.z), float3(klhgR.w, klhgG.w, klhgB.w), float3(ijfeR.y, ijfeG.y, ijfeB.y)),
        float3(klhgR.x, klhgG.x, klhgB.x));

    float3 aC = 0;
    float aW = 0;
    FsrEasuTap(aC, aW, float2(0.0, -1.0) - pp, dir, len2, lob, clp, float3(bczzR.x, bczzG.x, bczzB.x));
    FsrEasuTap(aC, aW, float2(1.0, -1.0) - pp, dir, len2, lob, clp, float3(bczzR.y, bczzG.y, bczzB.y));
    FsrEasuTap(aC, aW, float2(-1.0, 1.0) - pp, dir, len2, lob, clp, float3(ijfeR.x, ijfeG.x, ijfeB.x));
    FsrEasuTap(aC, aW, float2(0.0, 1.0) - pp, dir, len2, lob, clp, float3(ijfeR.y, ijfeG.y, ijfeB.y));
    FsrEasuTap(aC, aW, float2(0.0, 0.0) - pp, dir, len2, lob, clp, float3(ijfeR.z, ijfeG.z, ijfeB.z));
    FsrEasuTap(aC, aW, float2(-1.0, 0.0) - pp, dir, len2, lob, clp, float3(ijfeR.w, ijfeG.w, ijfeB.w));
    FsrEasuTap(aC, aW, float2(1.0, 1.0) - pp, dir, len2, lob, clp, float3(klhgR.x, klhgG.x, klhgB.x));
    FsrEasuTap(aC, aW, float2(2.0, 1.0) - pp, dir, len2, lob, clp, float3(klhgR.y, klhgG.y, klhgB.y));
    FsrEasuTap(aC, aW, float2(2.0, 0.0) - pp, dir, len2, lob, clp, float3(klhgR.z, klhgG.z, klhgB.z));
    FsrEasuTap(aC, aW, float2(1.0, 0.0) - pp, dir, len2, lob, clp, float3(klhgR.w, klhgG.w, klhgB.w));
    FsrEasuTap(aC, aW, float2(1.0, 2.0) - pp, dir, len2, lob, clp, float3(zzonR.z, zzonG.z, zzonB.z));
    FsrEasuTap(aC, aW, float2(0.0, 2.0) - pp, dir, len2, lob, clp, float3(zzonR.w, zzonG.w, zzonB.w));

    return min(max4, max(min4, aC * rcp(aW)));
}

void Pass1(uint2 blockStart, uint3 threadId) {
    uint2 gxy = blockStart + Rmp8x8(threadId.x);
    if (!CheckViewport(gxy)) {
        return;
    }

    uint2 inputSize = GetInputSize();
    uint2 outputSize = GetOutputSize();
    float2 inputPt = GetInputPt();

    float4 con0, con1, con2;
    float2 con3;
    con0[0] = (float)inputSize.x / (float)outputSize.x;
    con0[1] = (float)inputSize.y / (float)outputSize.y;
    con0[2] = 0.5f * con0[0] - 0.5f;
    con0[3] = 0.5f * con0[1] - 0.5f;
    con1[0] = inputPt.x;
    con1[1] = inputPt.y;
    con1[2] = inputPt.x;
    con1[3] = -inputPt.y;
    con2[0] = -inputPt.x;
    con2[1] = 2.0f * inputPt.y;
    con2[2] = inputPt.x;
    con2[3] = 2.0f * inputPt.y;
    con3[0] = 0;
    con3[1] = 4.0f * inputPt.y;

    WriteToOutput(gxy, FsrEasuF(gxy, con0, con1, con2, con3));

    gxy.x += 8u;
    if (CheckViewport(gxy)) {
        WriteToOutput(gxy, FsrEasuF(gxy, con0, con1, con2, con3));
    }

    gxy.y += 8u;
    if (CheckViewport(gxy)) {
        WriteToOutput(gxy, FsrEasuF(gxy, con0, con1, con2, con3));
    }

    gxy.x -= 8u;
    if (CheckViewport(gxy)) {
        WriteToOutput(gxy, FsrEasuF(gxy, con0, con1, con2, con3));
    }
}