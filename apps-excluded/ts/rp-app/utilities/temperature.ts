import { rgbToHex, type Rgb } from "./colors";

export function celsiusToThermalColor(celsius: number): string {
    return rgbToHex(temperatureToColor(celsius, 15, 40));
}

function temperatureToColor(temp: number, minTemp = -20, maxTemp = 40): Rgb {
    // Clamp the temperature to the range
    const clampedTemp = Math.max(minTemp, Math.min(temp, maxTemp));

    // Normalize the temperature to a 0-1 range
    const normalizedTemp = (clampedTemp - minTemp) / (maxTemp - minTemp);

    // Interpolate between blue (0, 0, 255) and red (255, 0, 0)
    const red = Math.round(255 * normalizedTemp);
    const green = 0; // Constant for this gradient
    const blue = Math.round(255 * (1 - normalizedTemp));

    // Convert RGB values to a hex color string
    return {
        r: red,
        g: green,
        b: blue,
    };
}

// Helper function to interpolate between colors
function interpolateColor(colors: Rgb[], t: number): Rgb {
    const step = 1 / (colors.length - 1);
    const idx = Math.floor(t / step);
    const localT = (t - idx * step) / step;

    const startColor = colors[idx];
    const endColor = colors[Math.min(idx + 1, colors.length - 1)];

    return {
        r: Math.round(startColor.r + (endColor.r - startColor.r) * localT),
        g: Math.round(startColor.g + (endColor.g - startColor.g) * localT),
        b: Math.round(startColor.b + (endColor.b - startColor.b) * localT),
    };
}
