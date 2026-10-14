shader_type canvas_item;

uniform vec4 vignette_color : source_color = vec4(0.04, 0.04, 0.08, 0.85);
uniform float inner_radius : hint_range(0.0, 1.0) = 0.38;
uniform float outer_radius : hint_range(0.0, 2.0) = 1.05;
uniform float vignette_opacity : hint_range(0.0, 1.0) = 0.88;

void fragment() {
    vec2 uv = UV - vec2(0.5);
    // Масштабування по осі X для правильного еліпсу під співвідношення 16:9
    uv.x *= 1.777;
    float dist = length(uv);
    float vignette = smoothstep(inner_radius, outer_radius, dist);
    COLOR = vec4(vignette_color.rgb, vignette * vignette_opacity);
}
