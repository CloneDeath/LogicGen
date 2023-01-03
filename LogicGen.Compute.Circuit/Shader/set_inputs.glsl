#version 450
#pragma shader_stage(compute)

layout(set = 0, binding = 0) readonly buffer InputBuffer {
    int size;
    bool[] inputs;
};

layout(set = 0, binding = 1) buffer OutputBuffer {
    bool[] state;
};

void main() {
    uint id = gl_GlobalInvocationID.x;
    if (id < size) {
        state[id] = inputs[id];
    }
}