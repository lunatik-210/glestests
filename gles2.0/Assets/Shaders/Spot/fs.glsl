precision mediump float;

const int MAX_LIGHTS = 8;

struct Light
{
    vec3 position;
    vec4 color;
    vec3 attenuation;
    vec3 direction;
    float ambient;
    float diffuse;
    float specular;
    float exponent;
    float cosCutoff;
};

uniform Light u_lights[MAX_LIGHTS];

uniform int numLights;
uniform vec3 u_camera;

varying vec3 v_vertex;
varying vec3 v_normal;

float calcAttenuation(float distance, vec3 lightvector, Light light)
{
    float spotEffect = dot(normalize(light.direction), -lightvector);
    float spot       = float(spotEffect > light.cosCutoff);
    spotEffect = max(pow(spotEffect, light.exponent), 0.0);
    return (spotEffect * spot) / (light.attenuation[0] + light.attenuation[1]*distance + light.attenuation[2]*distance*distance);
}

float calcPhong(vec3 lookvector, vec3 dirvector, vec3 n_normal, Light light)
{
    vec3 lightvector  = normalize(dirvector);
    float distance    = length(dirvector);
    
    // calc diffuse lighting
    float diffuse = light.diffuse * max(dot(n_normal, lightvector), 0.0);

    // calc specular lighting
    vec3 reflectvector = reflect(-lightvector, n_normal);
    float specular = light.specular * pow(max(dot(lookvector, reflectvector), 0.0), 40.0);
    
    return (light.ambient+diffuse+specular);
}

void main() {
    float diffuse, specular, distance, attenuation;
    vec3 lightvector, dirvector, reflectvector;
    float phong;

    vec3 n_normal = normalize(v_normal);
    vec3 lookvector = normalize(u_camera - v_vertex);
    vec4 final_color = vec4(0.0, 0.0, 0.0, 0.0);

    for(int i=0; i<MAX_LIGHTS; i++) {
        if( i >= numLights )
            break;
         
        // additional parameters
        dirvector    = u_lights[i].position - v_vertex;
        distance     = length(dirvector);
        lightvector  = normalize(dirvector);

        phong = calcPhong(lookvector, dirvector, n_normal, u_lights[i]);

        // calc attenuation for the light
        attenuation = calcAttenuation(distance, lightvector, u_lights[i]);
        final_color += phong*attenuation*u_lights[i].color;
    }
    gl_FragColor = final_color;
}
