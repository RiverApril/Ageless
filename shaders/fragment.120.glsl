struct Light{
    vec3 position;
    vec3 color;
};

uniform sampler2D Texture;

uniform Light light;

uniform mat4 ModelMatrix;
uniform mat3 NormalMatrix;

varying vec3 transferPosition;
varying vec2 transferUV;
varying vec3 transferNormal;

void main() {

	vec3 normal = normalize(NormalMatrix * transferNormal);

	vec3 position = vec3(ModelMatrix * vec4(transferPosition, 1));

	vec3 surfaceToLight = light.position - position;

	float brightness = max(dot(normal, surfaceToLight) / (length(surfaceToLight) * length(normal)), 0.2);

	vec4 surfaceColor = texture2D(Texture, transferUV);

	gl_FragColor = vec4(brightness * light.color * surfaceColor.rgb, surfaceColor.a);
}
