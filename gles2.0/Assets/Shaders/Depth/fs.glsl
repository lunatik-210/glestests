precision mediump float;

uniform float uObjectIndex;

void main()
{
    gl_FragColor = vec4(uObjectIndex, uObjectIndex, uObjectIndex, uObjectIndex);
}
