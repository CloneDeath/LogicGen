#version 450
#pragma shader_stage(compute)

layout(set = 0, binding = 0) readonly buffer InputBuffer {
    bool[] inputs;
};

layout(set = 0, binding = 1) buffer OutputBuffer {
    bool[] outputs;
};

void main() {
    uint id = gl_GlobalInvocationID.x;
    outputs[id] = inputs[id];
}