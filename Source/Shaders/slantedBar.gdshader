shader_type canvas_item;
uniform int width = 69;
uniform int height = 4;
uniform float percent : hint_range(0.0, 1.0) = 1;
varying float xsize;
varying float ysize;
void vertex() {
	xsize = VERTEX.x;
	ysize = VERTEX.y;
}
void fragment() {
	float h = float(height) - 1.0;
	float cur_y = ceil(ysize);
	int cur_i = int(xsize - h + cur_y);
	float cur = float(cur_i) / float(width);
	if (cur > percent) COLOR.a = 0.0;
}