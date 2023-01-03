#version 450
#pragma shader_stage(compute)

layout(set = 0, binding = 0) readonly buffer CircuitBuffer {
    int size;
    bool[] circuit;
};

layout(set = 0, binding = 1) readonly buffer InputBuffer {
    bool[] inputs;
};

layout(set = 0, binding = 2) buffer OutputBuffer {
    bool[] outputs;
};

void main() {
    uint id = gl_GlobalInvocationID.x;
    bool d = true;
    for (int i = 0; i < size; i++){
        uint index = i + (id * size);
        bool c = !circuit[index] || inputs[i];
        d = d && c;
    }
    outputs[id] = !d;
}