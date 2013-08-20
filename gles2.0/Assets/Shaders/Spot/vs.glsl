uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4 uNormal;
uniform vec3 u_camera;

attribute vec3 a_vertex;
attribute vec3 a_normal;

varying vec3 v_vertex;
varying vec3 v_normal;
varying vec3 v_look;

void main() {
	vec4 vertex = uModel * vec4(a_vertex, 1.0);
	v_vertex=vertex.xyz;
	vec3 n_normal=normalize(a_normal);
	v_normal=(uNormal*vec4(n_normal,1.0)).xyz;
  v_look = u_camera - v_vertex;
	gl_Position = uProjection * uView * vertex;
}
