// FidelityFX-FSR 中 EASU 通道
// 移植自 https://github.com/GPUOpen-Effects/FidelityFX-FSR/blob/master/ffx-fsr/ffx_fsr1.h

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
	inout float3 aC, // Accumulated color, with negative lobe.
	inout float aW,  // Accumulated weight.
	float2 off,      // Pixel offset from resolve position to tap.
	float2 dir,      // Gradient direction.
	float2 len,      // Length.
	float lob,       // Negative lobe strength.
	float clp,       // Clipping point.
	float3 c        // Tap color.
) {
	// Rotate offset by direction.
	float2 rotatedOffset = float2(
		dot(off, dir),
		dot(off, float2(-dir.y, dir.x))
	);

	// Anisotropy.
	rotatedOffset *= len;

	// Compute distance squared.
	float d2 = dot(rotatedOffset, rotatedOffset);

	// Limit to the clipping point.
	d2 = min(d2, clp);

	// Approximation of lanczos2 without sin() or rcp(), or sqrt() to get x.
	float wB = 2.0f / 5.0f * d2 - 1.0f;
	float wA = lob * d2 - 1.0f;
	wB = (25.0f / 16.0f) * wB * wB - (25.0f / 16.0f - 1.0f);
	float w = wB * wA;

	// Do weighted average.
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
	// Compute bilinear weight.
	float w = 0.0f;
	if (biS) w = (1.0f - pp.x) * (1.0f - pp.y);
	if (biT) w = pp.x * (1.0f - pp.y);
	if (biU) w = (1.0f - pp.x) * pp.y;
	if (biV) w = pp.x * pp.y;

	// Compute horizontal and vertical gradient differences.
	float dc = lD - lC;
	float cb = lC - lB;
	float ec = lE - lC;
	float ca = lC - lA;

	// Compute horizontal and vertical lengths.
	float lenX = max(abs(dc), abs(cb));
	float lenY = max(abs(ec), abs(ca));

	// Compute direction and length contributions.
	float dirX = lD - lB;
	float dirY = lE - lA;

	// Update direction and length.
	dir += float2(dirX, dirY) * w;
	len += saturate(abs(dirX) / lenX * abs(dirX) / lenX) * w;
	len += saturate(abs(dirY) / lenY * abs(dirY) / lenY) * w;
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

	float4 bczzL = 0.75f * (bczzR + bczzG) + 0.75f * bczzB;
	float4 ijfeL = 0.75f * (ijfeR + ijfeG) + 0.75f * ijfeB;
	float4 klhgL = 0.75f * (klhgR + klhgG) + 0.75f * klhgB;
	float4 zzonL = 0.75f * (zzonR + zzonG) + 0.75f * zzonB;

	float bL = bczzL.x;
	float cL = bczzL.y;
	float iL = ijfeL.x;
	float jL = ijfeL.y;
	float fL = ijfeL.z;
	float eL = ijfeL.w;
	float kL = klhgL.x;
	float lL = klhgL.y;
	float hL = klhgL.z;
	float gL = klhgL.w;
	float oL = zzonL.z;
	float nL = zzonL.w;

	float2 dir = 0;
	float len = 0;

	FsrEasuSet(dir, len, pp, true, false, false, false, bL, eL, fL, gL, jL);
	FsrEasuSet(dir, len, pp, false, true, false, false, cL, fL, gL, hL, kL);
	FsrEasuSet(dir, len, pp, false, false, true, false, fL, iL, jL, kL, nL);
	FsrEasuSet(dir, len, pp, false, false, false, true, gL, jL, kL, lL, oL);

//------------------------------------------------------------------------------------------------------------------------------
	// Normalize with approximation, and cleanup close to zero.
	float2 dir2 = dir * dir;
	float dirR = dir2.x + dir2.y;
	bool zro = dirR < 1.0f / 32768.0f;
	dirR = rsqrt(dirR);
	dirR = zro ? 1 : dirR;
	dir.x = zro ? 1 : dir.x;
	dir *= dirR;
	// Transform from {0 to 2} to {0 to 1} range, and shape with square.
	len = len * 0.5;
	len *= len;
	// Stretch kernel {1.0 vert|horz, to sqrt(2.0) on diagonal}.
	float stretch = (dir.x * dir.x + dir.y * dir.y) * rcp(max(abs(dir.x), abs(dir.y)));
	// Anisotropic length after rotation,
	//  x := 1.0 lerp to 'stretch' on edges
	//  y := 1.0 lerp to 2x on edges
	float2 len2 = { 1 + (stretch - 1) * len, 1 - 0.5 * len };
	// Based on the amount of 'edge',
	// the window shifts from +/-{sqrt(2.0) to slightly beyond 2.0}.
	float lob = 0.5 + ((1.0 / 4.0 - 0.04) - 0.5) * len;
	// Set distance^2 clipping point to the end of the adjustable window.
	float clp = rcp(lob);
//------------------------------------------------------------------------------------------------------------------------------
	// Accumulation mixed with min/max of 4 nearest.
	//    b c
	//  e f g h
	//  i j k l
	//    n o
	float3 min4 = min(min3(float3(ijfeR.z, ijfeG.z, ijfeB.z), float3(klhgR.w, klhgG.w, klhgB.w), float3(ijfeR.y, ijfeG.y, ijfeB.y)),
		float3(klhgR.x, klhgG.x, klhgB.x));
	float3 max4 = max(max3(float3(ijfeR.z, ijfeG.z, ijfeB.z), float3(klhgR.w, klhgG.w, klhgB.w), float3(ijfeR.y, ijfeG.y, ijfeB.y)),
		float3(klhgR.x, klhgG.x, klhgB.x));
	// Accumulation.
	float3 aC = 0;
	float aW = 0;
	FsrEasuTap(aC, aW, float2(0.0, -1.0) - pp, dir, len2, lob, clp, float3(bczzR.x, bczzG.x, bczzB.x)); // b
	FsrEasuTap(aC, aW, float2(1.0, -1.0) - pp, dir, len2, lob, clp, float3(bczzR.y, bczzG.y, bczzB.y)); // c
	FsrEasuTap(aC, aW, float2(-1.0, 1.0) - pp, dir, len2, lob, clp, float3(ijfeR.x, ijfeG.x, ijfeB.x)); // i
	FsrEasuTap(aC, aW, float2(0.0, 1.0) - pp, dir, len2, lob, clp, float3(ijfeR.y, ijfeG.y, ijfeB.y)); // j
	FsrEasuTap(aC, aW, float2(0.0, 0.0) - pp, dir, len2, lob, clp, float3(ijfeR.z, ijfeG.z, ijfeB.z)); // f
	FsrEasuTap(aC, aW, float2(-1.0, 0.0) - pp, dir, len2, lob, clp, float3(ijfeR.w, ijfeG.w, ijfeB.w)); // e
	FsrEasuTap(aC, aW, float2(1.0, 1.0) - pp, dir, len2, lob, clp, float3(klhgR.x, klhgG.x, klhgB.x)); // k
	FsrEasuTap(aC, aW, float2(2.0, 1.0) - pp, dir, len2, lob, clp, float3(klhgR.y, klhgG.y, klhgB.y)); // l
	FsrEasuTap(aC, aW, float2(2.0, 0.0) - pp, dir, len2, lob, clp, float3(klhgR.z, klhgG.z, klhgB.z)); // h
	FsrEasuTap(aC, aW, float2(1.0, 0.0) - pp, dir, len2, lob, clp, float3(klhgR.w, klhgG.w, klhgB.w)); // g
	FsrEasuTap(aC, aW, float2(1.0, 2.0) - pp, dir, len2, lob, clp, float3(zzonR.z, zzonG.z, zzonB.z)); // o
	FsrEasuTap(aC, aW, float2(0.0, 2.0) - pp, dir, len2, lob, clp, float3(zzonR.w, zzonG.w, zzonB.w)); // n
//------------------------------------------------------------------------------------------------------------------------------
	// Normalize and dering.
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
	// Output integer position to a pixel position in viewport.
	con0[0] = (float)inputSize.x / (float)outputSize.x;
	con0[1] = (float)inputSize.y / (float)outputSize.y;
	con0[2] = 0.5f * con0[0] - 0.5f;
	con0[3] = 0.5f * con0[1] - 0.5f;
	// Viewport pixel position to normalized image space.
	// This is used to get upper-left of 'F' tap.
	con1[0] = inputPt.x;
	con1[1] = inputPt.y;
	// Centers of gather4, first offset from upper-left of 'F'.
	//      +---+---+
	//      |   |   |
	//      +--(0)--+
	//      | b | c |
	//  +---F---+---+---+
	//  | e | f | g | h |
	//  +--(1)--+--(2)--+
	//  | i | j | k | l |
	//  +---+---+---+---+
	//      | n | o |
	//      +--(3)--+
	//      |   |   |
	//      +---+---+
	con1[2] = inputPt.x;
	con1[3] = -inputPt.y;
	// These are from (0) instead of 'F'.
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
