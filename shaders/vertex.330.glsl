#version 330

uniform mat4 ViewMatrix;

layout(location = 0) in vec3 vertexPosition;
layout(location = 1) in vec2 vertexUV;
layout(location = 2) in vec3 vertexNormal;

out vec3 transferPosition;
out vec2 transferUV;
out vec3 transferNormal;


void main() {
	gl_Position = ViewMatrix * vec4(vertexPosition, 1);
	
	transferPosition = vertexPosition;
	transferUV = vertexUV;
	transferNormal = vertexNormal;
}
