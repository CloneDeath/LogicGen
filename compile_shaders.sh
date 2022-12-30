shopt -s globstar
for i in **/*.glsl; do
    fileName="${i%.*}"
    glslc "$i" -o "${fileName}.spv"
done
#glslc copy.glsl -o copy.spv
#glslc copy_bits.glsl -o copy_bits.spv
#glslc circuit.glsl -o circuit.spv
