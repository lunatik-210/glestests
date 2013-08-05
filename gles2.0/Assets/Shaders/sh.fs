precision mediump float;

const int MAX_LIGHTS = 8;

struct Light
{
    vec3 position;
    vec4 color;
    vec3 attenuation;
};

uniform Light u_lights[MAX_LIGHTS];

uniform int numLights;
uniform vec3 u_camera;

varying vec3 v_vertex;
varying vec3 v_normal;

void main() {
    float diffuse, specular, distance, attenuation;
    vec3 lightvector, dirvector, reflectvector;
    float ambient = 0.2, k_diffuse = 0.8, k_specular = 0.8;

    vec3 n_normal=normalize(v_normal);
    vec3 lookvector = normalize(u_camera - v_vertex);
    vec4 final_color = vec4(0.0, 0.0, 0.0, 0.0);

    for(int i=0; i<MAX_LIGHTS; i++) {
        if( i >= numLights )
            break;

        dirvector = u_lights[i].position - v_vertex;

        distance = length(dirvector);
        lightvector = normalize(dirvector);

        attenuation = 1.0 / (u_lights[i].attenuation[0] + u_lights[i].attenuation[1]*distance + u_lights[i].attenuation[2]*distance*distance);

        diffuse = k_diffuse * max(dot(n_normal, lightvector), 0.0);

        reflectvector = reflect(-lightvector, n_normal);

        specular = k_specular * pow(max(dot(lookvector, reflectvector), 0.0), 40.0);

        final_color += (ambient+diffuse+specular)*attenuation*u_lights[i].color;
    }
    gl_FragColor = final_color;
}
