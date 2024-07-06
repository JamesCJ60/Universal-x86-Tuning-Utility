// FidelityFX-FSR RCAS Channel

//!MAGPIE EFFECT
//!VERSION 2
//!OUTPUT_WIDTH INPUT_WIDTH
//!OUTPUT_HEIGHT INPUT_HEIGHT

//!PARAMETER
//!DEFAULT 0.87
//!MIN 1e-5
float sharpness;

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

// This is set at the limit of providing unnatural results for sharpening.
#define FSR_RCAS_LIMIT (0.25-(1.0/16.0))

float3 FsrRcasF(float3 b, float3 d, float3 e, float3 f, float3 h) {
    float bR = b.r, bG = b.g, bB = b.b;
    float dR = d.r, dG = d.g, dB = d.b;
    float eR = e.r, eG = e.g, eB = e.b;
    float fR = f.r, fG = f.g, fB = f.b;
    float hR = h.r, hG = h.g, hB = h.b;

    // Luma times 2
    float3 lumaWeights = float3(0.5, 1.0, 0.5);
    float bL = dot(b, lumaWeights);
    float dL = dot(d, lumaWeights);
    float eL = dot(e, lumaWeights);
    float fL = dot(f, lumaWeights);
    float hL = dot(h, lumaWeights);

    // Noise detection
    float nz = 0.25 * (bL + dL + fL + hL) - eL;
    nz = saturate(abs(nz) * rcp(max3(max3(bL, dL, eL), fL, hL) - min3(min3(bL, dL, eL), fL, hL)));
    nz = -0.5 * nz + 1.0;

    // Min and max of ring
    float3 mn4 = min(min3(b, d, f), h);
    float3 mx4 = max(max3(b, d, f), h);

    // Immediate constants for peak range
    float peakC0 = 1.0;
    float peakC1 = -4.0;

    // Limiters
    float3 hitMin = min(mn4, e) * rcp(4.0 * mx4);
    float3 hitMax = (peakC0 - max(mx4, e)) * rcp(4.0 * mn4 + peakC1);
    float3 lobe = max(-hitMin, hitMax);
    lobe = max(-FSR_RCAS_LIMIT, min(max3(lobe.r, lobe.g, lobe.b), 0)) * sharpness;

    // Apply noise removal
    lobe *= nz;

    // Resolve
    float rcpL = rcp(4.0 * lobe + 1.0);
    return (lobe * (b + d + h + f) + e) * rcpL;
}

void Pass1(uint2 blockStart, uint3 threadId) {
    uint2 gxy = blockStart + (Rmp8x8(threadId.x) << 1);
    if (!CheckViewport(gxy)) {
        return;
    }

    float3 src[4][4];
    [unroll]
        for (uint i = 1; i < 3; ++i) {
            [unroll]
                for (uint j = 0; j < 4; ++j) {
                    src[i][j] = INPUT.Load(int3(gxy.x + i - 1, gxy.y + j - 1, 0)).rgb;
                }
        }

    src[0][1] = INPUT.Load(int3(gxy.x - 1, gxy.y, 0)).rgb;
    src[0][2] = INPUT.Load(int3(gxy.x - 1, gxy.y + 1, 0)).rgb;
    src[3][1] = INPUT.Load(int3(gxy.x + 2, gxy.y, 0)).rgb;
    src[3][2] = INPUT.Load(int3(gxy.x + 2, gxy.y + 1, 0)).rgb;

    WriteToOutput(gxy, FsrRcasF(src[1][0], src[0][1], src[1][1], src[2][1], src[1][2]));

    ++gxy.x;
    if (CheckViewport(gxy)) {
        WriteToOutput(gxy, FsrRcasF(src[2][0], src[1][1], src[2][1], src[3][1], src[2][2]));
    }

    ++gxy.y;
    if (CheckViewport(gxy)) {
        WriteToOutput(gxy, FsrRcasF(src[2][1], src[1][2], src[2][2], src[3][2], src[2][3]));
    }

    --gxy.x;
    if (CheckViewport(gxy)) {
        WriteToOutput(gxy, FsrRcasF(src[1][1], src[0][2], src[1][2], src[2][2], src[1][3]));
    }
}