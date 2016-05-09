uniform mat4 ViewMatrix;

attribute vec3 vertexPosition;
attribute vec2 vertexUV;
attribute vec3 vertexNormal;

varying vec3 transferPosition;
varying vec2 transferUV;
varying vec3 transferNormal;

void main() {
	gl_Position = ViewMatrix * vec4(vertexPosition, 1);
	
	transferPosition = vertexPosition;
	transferUV = vertexUV;
	transferNormal = vertexNormal;
}
