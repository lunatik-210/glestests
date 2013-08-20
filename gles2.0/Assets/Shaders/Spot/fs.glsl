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
varying vec3 v_look;

float calcAttenuation(vec3 dirvector, Light light)
{
    float distance    = length(dirvector);
    vec3  lightvector = normalize(dirvector);
    
    float spotEffect  = dot(normalize(light.direction), -lightvector);
    float spot        = float(spotEffect > light.cosCutoff);
    
    spotEffect = max(pow(spotEffect, light.exponent), 0.0);
    return (spotEffect * spot) / (light.attenuation[0] + light.attenuation[1]*distance + light.attenuation[2]*distance*distance);
}

float calcPhong(vec3 dirvector, vec3 lookvector, vec3 n_normal, Light light)
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

float calcCookTorrance(float roughness, vec3 dirvector, vec3 lookvector, vec3 n_normal, Light light)
{
    float e = 2.7182818284;

    vec3 lightvector = normalize(dirvector); 
    vec3 half_vec    = normalize(lookvector + lightvector);
    
    float nh = max(dot(n_normal, half_vec), 1.0e-7);
    float nv = max(dot(n_normal, lookvector), 0.0);
    float nl = max(dot(n_normal, lightvector), 0.0);
    float vh = max(dot(lookvector, half_vec), 0.0);
    
    // calc geometric coefficient
    float gk = 2.0*nh/vh;
    float g = min(1.0, gk*min(nv,nl));
    
    // calc roughness coefficient
    float r2 = roughness * roughness;
    float nh2 = nh * nh;
    float nh2r = 1.0 / (nh2 * r2);
    float d = exp((nh2 - 1.0) * ( nh2r )) * nh2r / (4.0 * nh2 );
    
    // calc Fresnel coefficient
    float f =  1.0 / (1.0 + nv); 
    
    // calc cook torrance coefficient
    float k = (f * d * g) / (nv * nl + 1.0e-7);
    
    return (light.ambient + nl * ( light.diffuse + light.specular * k ) );
}

void main() {
    float attenuation, k;
    vec3 dirvector;

    vec3 n_normal = normalize(v_normal);
    vec3 lookvector = normalize(v_look);
    vec4 final_color = vec4(0.0, 0.0, 0.0, 0.0);

    for(int i=0; i<MAX_LIGHTS; i++) {
        if( i >= numLights )
            break;
         
        dirvector = u_lights[i].position - v_vertex;

        //k = calcPhong(dirvector, lookvector, n_normal, u_lights[i]);
        k = calcCookTorrance(0.1, dirvector, lookvector, n_normal, u_lights[i]);

        // calc attenuation for the light
        attenuation = calcAttenuation(dirvector, u_lights[i]);
        final_color += k*attenuation*u_lights[i].color;
    }
    gl_FragColor = final_color;
}
