import { getHealth } from "./gen/endpoints/server";
import { customFetch } from "./mutator/custom-fetch";
import { dispenseFood } from "./gen/endpoints/food-dispenser";
import { getDailySummaries, getDetections } from "./gen/endpoints/detections";
import { getThermalImagerReadingsDegreesCelsius } from "./gen/endpoints/thermal-image-readings";

export class McuApi {
    constructor(
        private readonly config: {
            baseUrl: string;
        },
    ) {
        customFetch.extend = async (url, options) => {
            const newUrl = `${this.config.baseUrl}${url}`;
            return await fetch(newUrl, options);
        };
    }

    async health(init?: RequestInit) {
        const h = await getHealth(init);

        return h.status >= 200 && h.status < 300;
    }

    async dispense(servings: number) {
        return await dispenseFood({
            servings: servings,
        });
    }

    async getDailyStatistics() {
        return await getDailySummaries();
    }

    async getDetections() {
        return await getDetections();
    }

    async getThermalImageCelsius() {
        return await getThermalImagerReadingsDegreesCelsius();
    }
}
