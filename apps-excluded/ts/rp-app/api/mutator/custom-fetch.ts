export type CustomFetch = (
    url: string,
    options: RequestInit,
) => Promise<Response>;

export async function customFetch<T>(
    url: string,
    options: RequestInit,
): Promise<T> {
    const res = await customFetch.extend(url, options);

    return {
        status: res.status,
        data: res.status === 401 ? {} : await res.json(),
    } as Awaited<T>;
}

customFetch.extend = window.fetch.bind(window) as CustomFetch;
