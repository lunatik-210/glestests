uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

attribute vec3 a_vertex;

void main() {
    gl_Position = uProjection * uView * uModel * vec4(a_vertex, 1.0);
}
