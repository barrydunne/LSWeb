export interface LivenessResult {
  status: string;
}

export async function getLiveness(signal?: AbortSignal): Promise<LivenessResult> {
  const response = await fetch('/api/system/liveness', { signal });
  if (!response.ok) {
    throw new Error(`Liveness request failed with status ${response.status}`);
  }
  return (await response.json()) as LivenessResult;
}

export interface ServiceHealthItem {
  key: string;
  availability: string;
}

export interface HealthResult {
  services: ServiceHealthItem[];
}

export async function getHealth(signal?: AbortSignal): Promise<HealthResult> {
  const response = await fetch('/api/system/health', { signal });
  if (!response.ok) {
    throw new Error(`Health request failed with status ${response.status}`);
  }
  return (await response.json()) as HealthResult;
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

export interface CatalogueServiceItem {
  key: string;
  displayName: string;
  category: string;
  iconHint: string;
  route: string;
  supported: boolean;
  supportDetail: string | null;
}

export interface CatalogueResult {
  services: CatalogueServiceItem[];
}

export async function getCatalogue(signal?: AbortSignal): Promise<CatalogueResult> {
  const response = await fetch('/api/system/catalogue', { signal });
  if (!response.ok) {
    throw new Error(`Catalogue request failed with status ${response.status}`);
  }
  return (await response.json()) as CatalogueResult;
}

export async function refreshCatalogue(signal?: AbortSignal): Promise<void> {
  const response = await fetch('/api/system/catalogue/refresh', { method: 'POST', signal });
  if (!response.ok) {
    throw new Error(`Catalogue refresh request failed with status ${response.status}`);
  }
}

export interface ActivityEntryItem {
  operationId: string;
  operation: string;
  state: string;
  message: string;
  occurredAt: string;
}

export interface ActivityResult {
  entries: ActivityEntryItem[];
}

export async function getActivity(signal?: AbortSignal): Promise<ActivityResult> {
  const response = await fetch('/api/system/activity', { signal });
  if (!response.ok) {
    throw new Error(`Activity request failed with status ${response.status}`);
  }
  return (await response.json()) as ActivityResult;
}

export interface ResolvedReferenceResult {
  serviceKey: string;
  resourceId: string;
  route: string;
}

export async function resolveReference(
  reference: string,
  service?: string,
  signal?: AbortSignal,
): Promise<ResolvedReferenceResult> {
  const params = new URLSearchParams({ ref: reference });
  if (service) {
    params.set('service', service);
  }
  const response = await fetch(`/api/navigation/resolve?${params.toString()}`, { signal });
  if (!response.ok) {
    throw new Error(`Reference resolution failed with status ${response.status}`);
  }
  return (await response.json()) as ResolvedReferenceResult;
}

export interface SearchMatchItem {
  serviceKey: string;
  resourceId: string;
  displayName: string;
  route: string;
}

export interface SearchResult {
  matches: SearchMatchItem[];
}

export async function getSearch(query: string, signal?: AbortSignal): Promise<SearchResult> {
  const params = new URLSearchParams({ q: query });
  const response = await fetch(`/api/search?${params.toString()}`, { signal });
  if (!response.ok) {
    throw new Error(`Search request failed with status ${response.status}`);
  }
  return (await response.json()) as SearchResult;
}

export interface SearchStateResult {
  builtAt: string;
  entryCount: number;
  isBuilding: boolean;
}

export async function getSearchState(signal?: AbortSignal): Promise<SearchStateResult> {
  const response = await fetch('/api/search/state', { signal });
  if (!response.ok) {
    throw new Error(`Search state request failed with status ${response.status}`);
  }
  return (await response.json()) as SearchStateResult;
}

export async function refreshSearch(signal?: AbortSignal): Promise<void> {
  const response = await fetch('/api/search/refresh', { method: 'POST', signal });
  if (!response.ok) {
    throw new Error(`Search refresh request failed with status ${response.status}`);
  }
}
