#version 330

struct Light{
	bool directional;
    vec3 position;
    vec3 color;
};

uniform mat4 ModelMatrix;
uniform mat3 NormalMatrix;


uniform sampler2DArray Texture;
uniform int textureIndexOffset;

uniform bool replaceColor;
uniform vec4 color;

uniform Light light;


in vec3 transferPosition;
in vec3 transferUV;
in vec3 transferNormal;

out vec4 fragColor;

void main() {

	vec3 normal = normalize(NormalMatrix * transferNormal);

	vec3 position = vec3(ModelMatrix * vec4(transferPosition, 1));

	vec3 surfaceToLight;

	if(light.directional){

		surfaceToLight = normalize(light.position);

	}else{
		surfaceToLight = light.position - position;
	}

	float brightness = max(dot(normal, surfaceToLight) / (length(surfaceToLight) * length(normal)), 0.2);

	vec4 surfaceColor = texture(Texture, transferUV+vec3(0, 0, textureIndexOffset));

	fragColor = replaceColor ? color : vec4(brightness * light.color * surfaceColor.rgb * color.rgb, surfaceColor.a * color.a);
}
