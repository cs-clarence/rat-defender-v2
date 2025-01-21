import { type HexColor, hexToRgb, rgbToHex } from "./colors";

export type PixelMatrix = HexColor[][]; // Matrix of hex color strings

/**
 * Resizes an image represented as a pixel matrix using bilinear interpolation.
 * @param matrix - The input pixel matrix (rows x columns) in hex format.
 * @param factor - The resizing factor (e.g., 0.5 for downscaling, 2 for upscaling).
 * @returns The resized pixel matrix in hex format.
 */
export function resizeImage(matrix: PixelMatrix, factor: number): PixelMatrix {
    if (factor <= 0) {
        throw new Error("Resize factor must be greater than 0.");
    }

    if (factor === 1) {
        return matrix;
    }

    const originalHeight = matrix.length;
    const originalWidth = matrix[0]?.length || 0;
    if (originalHeight === 0 || originalWidth === 0) {
        throw new Error("Input matrix cannot be empty.");
    }

    const newHeight = Math.max(1, Math.round(originalHeight * factor));
    const newWidth = Math.max(1, Math.round(originalWidth * factor));

    const resizedMatrix: PixelMatrix = Array.from({ length: newHeight }, () =>
        Array.from({ length: newWidth }, () => "#000000"),
    );

    for (let i = 0; i < newHeight; i++) {
        for (let j = 0; j < newWidth; j++) {
            // Map new coordinates back to original image coordinates
            const y = (i / (newHeight - 1)) * (originalHeight - 1);
            const x = (j / (newWidth - 1)) * (originalWidth - 1);

            const y0 = Math.floor(y);
            const y1 = Math.min(y0 + 1, originalHeight - 1);
            const x0 = Math.floor(x);
            const x1 = Math.min(x0 + 1, originalWidth - 1);

            const dy = y - y0;
            const dx = x - x0;

            // Bilinear interpolation for each channel
            const interpolate = (
                v00: number,
                v01: number,
                v10: number,
                v11: number,
            ) =>
                v00 * (1 - dx) * (1 - dy) +
                v01 * dx * (1 - dy) +
                v10 * (1 - dx) * dy +
                v11 * dx * dy;

            const rgb00 = hexToRgb(matrix[y0][x0]);
            const rgb01 = hexToRgb(matrix[y0][x1]);
            const rgb10 = hexToRgb(matrix[y1][x0]);
            const rgb11 = hexToRgb(matrix[y1][x1]);

            const interpolatedRGB = [0, 1, 2].map((channel) =>
                interpolate(
                    rgb00[channel],
                    rgb01[channel],
                    rgb10[channel],
                    rgb11[channel],
                ),
            ) as [number, number, number];

            resizedMatrix[i][j] = rgbToHex({
                r: Math.round(interpolatedRGB[0]),
                g: Math.round(interpolatedRGB[1]),
                b: Math.round(interpolatedRGB[2]),
            });
        }
    }

    return resizedMatrix;
}
