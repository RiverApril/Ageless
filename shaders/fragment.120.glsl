struct Light{
    vec3 position;
    vec3 color;
};

uniform mat4 ModelMatrix;
uniform mat3 NormalMatrix;


uniform sampler2DArray Texture;
uniform int textureIndex;

uniform bool replaceColor;
uniform vec4 color;

uniform Light light;


varying vec3 transferPosition;
varying vec2 transferUV;
varying vec3 transferNormal;

void main() {

	vec3 normal = normalize(NormalMatrix * transferNormal);

	vec3 position = vec3(ModelMatrix * vec4(transferPosition, 1));

	vec3 surfaceToLight = light.position - position;

	float brightness = max(dot(normal, surfaceToLight) / (length(surfaceToLight) * length(normal)), 0.2);

	vec4 surfaceColor = texture2DArray(Texture, vec3(transferUV, textureIndex));

	gl_FragColor = replaceColor ? color : vec4(brightness * light.color * surfaceColor.rgb * color, surfaceColor.a);
}
