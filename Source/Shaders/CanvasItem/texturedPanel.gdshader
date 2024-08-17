shader_type canvas_item;

uniform bool useKeyColor;
uniform vec3 keyColor : source_color;
uniform sampler2D overlayTexture : source_color, repeat_enable;
uniform float percent : hint_range(0.0, 1.0, 0.1) = 0.5;
uniform float scale = 1;
uniform vec2 resolution = vec2(480,270);

void fragment() {
	if (!useKeyColor || COLOR.rgb == keyColor) {
		float xsize = UV.x;
		float ysize = UV.y;
		float x = floor(xsize * resolution.x) / resolution.x;
		float y = floor(ysize * resolution.y) / resolution.y;
		vec4 c = COLOR;
		vec4 t = texture(overlayTexture, vec2(x * scale,y * scale));
		COLOR = mix(c, t, percent);
		COLOR = c * COLOR;
		COLOR.a = c.a;
	}
}
