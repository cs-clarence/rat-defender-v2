export type HexColor = string; // Representing a pixel as a 6-digit hex color (e.g., "#RRGGBB")

export type Rgb = { r: number; g: number; b: number };

/**
 * Converts RGB values to a 6-digit hex color string.
 * @param r - Red value (0-255).
 * @param g - Green value (0-255).
 * @param b - Blue value (0-255).
 * @returns Hexadecimal color string (e.g., "#RRGGBB").
 */
export function rgbToHex({ r, g, b }: Rgb): HexColor {
    const toHex = (value: number) => value.toString(16).padStart(2, "0");
    return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
}

export function hexToRgb(hex: HexColor) {
    return [
        Number.parseInt(hex.slice(1, 3), 16),
        Number.parseInt(hex.slice(3, 5), 16),
        Number.parseInt(hex.slice(5, 7), 16),
    ];
}

export function getContrastingColor(hex: HexColor) {
    const rgb = hexToRgb(hex);
    const brightness = (rgb[0] + rgb[1] + rgb[2]) / 3;

    if (brightness > 127) {
        return "#000000";
    }
    return "#FFFFFF";
}
