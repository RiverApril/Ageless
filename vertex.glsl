#version 420

uniform mat4 ProjectionMatrix;
uniform mat4 ModelMatrix;
//uniform mat4 NormalMatrix;

in vec3 vertexPosition;
//in vec3 vertexNormal;
in vec4 vertexColor;
in vec2 vertexUV;

//out vec3 NormalTransfer;
out vec4 ColorTransfer;
out vec2 UVTransfer;

void main() {
	gl_Position = ProjectionMatrix * ModelMatrix * vec4(vertexPosition, 1.0f);

  
	//NormalTransfer = vertexNormal;
	ColorTransfer = vertexColor;
	UVTransfer = vertexUV;
}
