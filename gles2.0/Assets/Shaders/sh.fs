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

void refract(inout vec3 ray, vec3 normal) 
{   
    
    float eta = 1.0/0.3; // eta = in_IOR/out_IOR
    
    float cos_theta = -dot(normal, ray);

    vec3 m_normal = vec3(normal);

    if(cos_theta < 0.0)
    {
        cos_theta *= -1.0;
        m_normal *= -1.0;
        eta = 1.0/eta;
    }

    
    float k = 1.0 - eta*eta*(1.0-cos_theta*cos_theta);
    if(k >= 0.0)
        ray = normalize( eta*ray + (eta*cos_theta - sqrt(k))*normal);
    
}


void main() {
    float diffuse, specular, distance, attenuation;
    vec3 lightvector, dirvector, reflectvector;
    float ambient = 0.3, k_diffuse = 0.8, k_specular = 0.8;

    vec3 n_normal=normalize(v_normal);
    vec3 lookvector = normalize(u_camera - v_vertex);
    vec4 final_color = vec4(0.0, 0.0, 0.0, 0.0);

    for(int i=0; i<MAX_LIGHTS; i++) {
        if( i >= numLights )
            break;
        
        // additional parameters
        dirvector = u_lights[i].position - v_vertex;
        //refract(dirvector, n_normal);
        distance = length(dirvector);
        lightvector = normalize(dirvector);

        // calc diffuse lighting
        diffuse = k_diffuse * max(dot(n_normal, lightvector), 0.0);

        // calc specular lighting
        reflectvector = reflect(-lightvector, n_normal);
        specular = k_specular * pow(max(dot(lookvector, reflectvector), 0.0), 40.0);

        // calc attenuation for the light
        attenuation = 1.0 / (u_lights[i].attenuation[0] + u_lights[i].attenuation[1]*distance + u_lights[i].attenuation[2]*distance*distance);
        final_color += (ambient+diffuse+specular)*attenuation*u_lights[i].color;
    }
    gl_FragColor = final_color;
}
