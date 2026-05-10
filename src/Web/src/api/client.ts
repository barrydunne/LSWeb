export interface LivenessResult {
  status: string;
}

export async function getLiveness(signal?: AbortSignal): Promise<LivenessResult> {
  const response = await fetch('/api/system/health', { signal });
  if (!response.ok) {
    throw new Error(`Health request failed with status ${response.status}`);
  }
  return (await response.json()) as LivenessResult;
}

export interface ConnectivityResult {
  status: string;
  endpoint: string;
  region: string;
  error: string | null;
}

export async function getConnectivity(signal?: AbortSignal): Promise<ConnectivityResult> {
  const response = await fetch('/api/system/connectivity', { signal });
  if (!response.ok) {
    throw new Error(`Connectivity request failed with status ${response.status}`);
  }
  return (await response.json()) as ConnectivityResult;
}
