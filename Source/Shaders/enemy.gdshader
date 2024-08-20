shader_type canvas_item;

varying float xsize;
varying float ysize;
uniform float percent: hint_range(0.0, 1.0) = 0;
uniform float disappear: hint_range(0.0, 1.0) = 0;
uniform float width = 32;
uniform float height= 32;
uniform bool blink = false;
uniform vec4 mixCol :source_color;
// <>
void vertex() {
	xsize = VERTEX.x;
	ysize = VERTEX.y;
}
void fragment() {
	// Mix
	if (blink) {
		COLOR.rgb = mix(COLOR.rgb, mixCol.rgb, mixCol.a);
	}
	float xs = floor(xsize) / width;
	float ys = floor(ysize) / width;
	float dx = xs;
	if (dx < 0.0) dx *= -1.0;
	float dy = ys;
	if (dy < 0.0) dy *= -1.0;
	//
	float dd = dx + dy;
	float a = COLOR.a;
	if (dd < disappear) a = 0.0;
	if (dd < percent) COLOR = vec4(1,1,1,a);
}
