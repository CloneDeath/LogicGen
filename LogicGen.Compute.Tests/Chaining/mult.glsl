#version 450
#pragma shader_stage(compute)

layout(set = 0, binding = 0) readonly buffer InputBuffer {
    int[] a;
} inputBuffer;

layout(set = 0, binding = 1) buffer OutputBuffer {
    int[] b;
} outputBuffer;

void main() {
    uint id = gl_GlobalInvocationID.x;
    outputBuffer.b[id] = inputBuffer.a[id] * 2;
}