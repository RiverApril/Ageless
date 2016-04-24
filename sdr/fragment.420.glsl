﻿#version 420

struct Light{
    vec3 position;
    vec3 color;
};

uniform sampler2D Texture;

uniform Light light;

uniform mat4 ModelMatrix;
uniform mat3 NormalMatrix;

in vec3 transferPosition;
in vec4 transferColor;
in vec2 transferUV;
in vec3 transferNormal;

out vec4 fragColor;

void main() {

	vec3 normal = normalize(NormalMatrix * transferNormal);

	vec3 position = vec3(ModelMatrix * vec4(transferPosition, 1));

	vec3 surfaceToLight = light.position - position;

	float brightness = dot(normal, surfaceToLight) / (length(surfaceToLight) * length(normal));

	vec4 surfaceColor = texture2D(Texture, transferUV) * transferColor;

	fragColor = vec4(brightness * light.color * surfaceColor.rgb, surfaceColor.a);
}