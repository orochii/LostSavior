shader_type canvas_item;

uniform float percent : hint_range(0.0, 1.0, 0.0);
uniform vec3 overlay : source_color;
uniform float height = 32;
varying float sizey;

void vertex() {
	sizey = round(VERTEX.y);
}

void fragment() {
	float d = floor(percent*height);
	if (sizey > d) {
		COLOR.rgb *= overlay;
	}
	//COLOR.r = sizey/height;
}