shader_type canvas_item;

varying float xsize;
varying float ysize;
uniform float percent: hint_range(0.0, 1.0) = 0;
uniform float disappear: hint_range(0.0, 1.0) = 0;
uniform float width = 32;
// <>
void vertex() {
	xsize = VERTEX.x / width;
	ysize = VERTEX.y / width;
}
void fragment() {
	float dx = xsize;
	if (dx < 0.0) dx *= -1.0;
	float dy = ysize;
	if (dy < 0.0) dy *= -1.0;
	//
	float dd = dx + dy;
	float a = COLOR.a;
	if (dd < disappear) a = 0.0;
	if (dd < percent) COLOR = vec4(1,1,1,a);
}
