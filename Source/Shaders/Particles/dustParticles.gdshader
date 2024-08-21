// Based off Godot's autoconverted particle process shader
// Just cut off all things I'm not interested in for ease of yes.

shader_type particles;
render_mode disable_velocity;

uniform float direction = 1.0;
uniform float anim_speed_min = 2;
uniform float anim_speed_max = 2;
uniform float anim_offset_min = 0;
uniform float anim_offset_max = 0;
uniform float lifetime_randomness = 0;
uniform vec4 color_value : source_color = vec4(1.0,1.0,1.0,1.0);

float rand_from_seed(inout uint seed) {
	int k;
	int s = int(seed);
	if (s == 0) {
		s = 305420679;
	}
	k = s / 127773;
	s = 16807 * (s - k * 127773) - 2836 * k;
	if (s < 0) {
		s += 2147483647;
	}
	seed = uint(s);
	return float(seed % uint(65536)) / 65535.0;
}

uint hash(uint x) {
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = (x >> uint(16)) ^ x;
	return x;
}

struct DisplayParameters {
	float animation_speed;
	float animation_offset;
	float lifetime;
	vec4 color;
};

void calculate_initial_display_params(inout DisplayParameters params, inout uint alt_seed) {
	// -------------------- DO NOT REORDER OPERATIONS, IT BREAKS VISUAL COMPATIBILITY
	// -------------------- ADD NEW OPERATIONS AT THE BOTTOM
	float pi = 3.14159;
	params.animation_speed = mix(anim_speed_min, anim_speed_max, rand_from_seed(alt_seed));
	params.animation_offset = mix(anim_offset_min, anim_offset_max, rand_from_seed(alt_seed));
	params.lifetime = (1.0 - lifetime_randomness * rand_from_seed(alt_seed));
	params.color = color_value;
}

void start() {
	uint base_number = NUMBER;
	uint alt_seed = hash(base_number + uint(1) + RANDOM_SEED);
	DisplayParameters params;
	calculate_initial_display_params(params, alt_seed);
	if (rand_from_seed(alt_seed) > AMOUNT_RATIO) {
		ACTIVE = false;
	}
	// Direction and color
	if (RESTART_CUSTOM) {
		CUSTOM = vec4(0.0);
		CUSTOM.w = params.lifetime;
		CUSTOM.x = direction; // Flip H: 1 normal -1 flipped
	}
	if (RESTART_COLOR) {
		COLOR = params.color;
	}
	if (RESTART_POSITION) {
		TRANSFORM[3].xyz = vec3(0.0);
		TRANSFORM = EMISSION_TRANSFORM * TRANSFORM;
	}
	TRANSFORM[3].z = 0.0;
}

void process() {
	uint base_number = NUMBER;
	uint alt_seed = hash(base_number + uint(1) + RANDOM_SEED);
	DisplayParameters params;
	calculate_initial_display_params(params, alt_seed);
	// Lifetime update
	CUSTOM.y += DELTA / LIFETIME;
	CUSTOM.y = mix(CUSTOM.y, 1.0, INTERPOLATE_TO_END);
	float lifetime_percent = CUSTOM.y / params.lifetime;
	if (CUSTOM.y > CUSTOM.w) {
		ACTIVE = false;
	}
	// cos 0 = 1
	// sin 0 = 0
	// CUSTOM.x
	TRANSFORM[0] = vec4(CUSTOM.x, 0, 0.0, 0.0);
	TRANSFORM[1] = vec4(0, 1, 0.0, 0.0);
	TRANSFORM[2] = vec4(0.0, 0.0, 1.0, 0.0);
	TRANSFORM[3].z = 0.0;
	// Animation
	CUSTOM.z = params.animation_offset + lifetime_percent * params.animation_speed;
}
