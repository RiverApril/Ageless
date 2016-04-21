#version 420


uniform sampler2D Texture;

//in vec3 NormalTransfer;
in vec4 ColorTransfer;
in vec2 UVTransfer;

out vec4 fragColor;

void main() {
	fragColor = ColorTransfer * texture2D(Texture, UVTransfer);
}
