shader_type canvas_item;

uniform float percent : hint_range(0.0, 1.0) = 1;

void fragment() {
	vec4 origCol = COLOR;
	if (UV.x > percent) {
		COLOR = vec4(0.01,0.05,0.1,1);
	} else {
		COLOR = vec4(0.9,0.3,0.5,1);
	}
	COLOR *= origCol;
}
