shader_type canvas_item;

// células: r = material, g = tonalidade, b = eletricidade (estado | brilho<<2)
uniform sampler2D cells : filter_nearest;
uniform sampler2D temp : filter_nearest;
uniform vec3 pal[128];
uniform float alpha_[128];
uniform float vary[128];
uniform ivec2 size;
uniform int view_mode = 0;
uniform int fire_id;
uniform int lamp_id;
uniform int virus_id;
uniform int spark_id;
uniform int switch_id;
uniform int neon_id;
uniform int plasma_id;
uniform int lit_id;
uniform float time_s;

float h(vec2 p) { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }

vec3 hue(float h) {
	vec3 k = clamp(abs(mod(h * 6.0 + vec3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
	return mix(vec3(1.0), k, 0.85);
}

vec3 bb(float t) {
	float k = clamp((t - 450.0) / 2000.0, 0.0, 1.0);
	vec3 c = mix(vec3(0.55, 0.03, 0.0), vec3(1.0, 0.38, 0.05), smoothstep(0.0, 0.25, k));
	return mix(c, vec3(1.0, 0.97, 0.85), smoothstep(0.25, 1.0, k));
}

vec3 heat(float t) {
	if (t < 20.0) return mix(vec3(0.08, 0.09, 0.12), vec3(0.25, 0.55, 1.0), clamp((20.0 - t) / 200.0, 0.0, 1.0));
	float k = clamp(log(1.0 + (t - 20.0) / 25.0) / log(1.0 + 3000.0 / 25.0), 0.0, 1.0);
	if (k < 0.33) return mix(vec3(0.08, 0.09, 0.12), vec3(0.85, 0.1, 0.1), k / 0.33);
	if (k < 0.66) return mix(vec3(0.85, 0.1, 0.1), vec3(1.0, 0.82, 0.1), (k - 0.33) / 0.33);
	return mix(vec3(1.0, 0.82, 0.1), vec3(1.0), (k - 0.66) / 0.34);
}

void fragment() {
	ivec2 p = clamp(ivec2(floor(UV * vec2(size))), ivec2(0), size - ivec2(1));
	vec4 c4 = texelFetch(cells, p, 0);
	int t = int(round(c4.r * 255.0));
	float s = c4.g;
	int e = int(round(c4.b * 255.0));
	float tm = texelFetch(temp, p, 0).r;
	vec3 bg = mix(vec3(0.03, 0.035, 0.05), vec3(0.055, 0.06, 0.08), float(p.y) / float(size.y));
	vec3 c;
	if (view_mode == 1) {
		c = t == 0 ? bg : heat(tm);
	} else if (t == 0) {
		c = bg;
	} else {
		vec3 base = pal[t] * (1.0 + vary[t] * (s - 0.5));
		float al = alpha_[t];
		if (t == switch_id) {
			base = s > 0.5 ? vec3(0.25, 0.85, 0.35) : vec3(0.75, 0.18, 0.16);
		} else if (t == neon_id && (e & 3) > 0) {
			base = vec3(1.0, 0.3, 0.6) * (1.2 + 0.25 * h(vec2(p) + floor(time_s * 8.0)));
			al = 0.95;
		} else if (t == plasma_id) {
			base = mix(vec3(0.75, 0.45, 1.0), vec3(1.0), h(vec2(p) + floor(time_s * 25.0))) * 1.3;
		} else if (t == lit_id) {
			base = pal[t] * (fract(time_s * 3.0) < 0.5 ? 1.3 : 0.7);
		} else if (t == spark_id) {
			base = hue(s) * (1.1 + 0.4 * h(vec2(p) + floor(time_s * 12.0)));
		} else if (t == virus_id) {
			base = pal[t] * (0.75 + 0.35 * sin(time_s * 5.0 + h(vec2(p)) * 6.283));
		} else if (t == fire_id) {
			base = bb(tm) * (0.75 + 0.5 * h(vec2(p) + floor(time_s * 20.0)));
		} else if (tm > 480.0) {
			base = mix(base, bb(tm) * 1.25, smoothstep(480.0, 1300.0, tm) * 0.9);
		}
		if (e > 0 && t != neon_id && t != switch_id) {
			if (t == lamp_id) { if ((e >> 2) > 0) base = vec3(1.0, 0.93, 0.6) * (1.1 + 0.1 * s); }
			else if ((e & 3) > 0) base = mix(base, vec3(0.75, 0.88, 1.0), 0.65);
			else if ((e >> 2) > 0) base = mix(base, vec3(0.6, 0.8, 1.0), 0.3);
		}
		c = mix(bg, base, al);
	}
	COLOR = vec4(c, 1.0);
}
