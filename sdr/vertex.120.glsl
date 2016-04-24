#version 120

uniform mat4 WMPMatrix;

attribute vec3 vertexPosition;
attribute vec4 vertexColor;
attribute vec2 vertexUV;
attribute vec3 vertexNormal;

varying vec3 transferPosition;
varying vec4 transferColor;
varying vec2 transferUV;
varying vec3 transferNormal;

void main() {
	gl_Position = WMPMatrix * vec4(vertexPosition, 1.0f);
	
	transferPosition = vertexPosition;
	transferColor = vertexColor;
	transferUV = vertexUV;
	transferNormal = vertexNormal;
}
