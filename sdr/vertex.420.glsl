#version 420

uniform mat4 WMPMatrix;

in vec3 vertexPosition;
in vec4 vertexColor;
in vec2 vertexUV;
in vec3 vertexNormal;

out vec3 transferPosition;
out vec4 transferColor;
out vec2 transferUV;
out vec3 transferNormal;

void main() {
	gl_Position = WMPMatrix * vec4(vertexPosition, 1);
	
	transferPosition = vertexPosition;
	transferColor = vertexColor;
	transferUV = vertexUV;
	transferNormal = vertexNormal;
}
