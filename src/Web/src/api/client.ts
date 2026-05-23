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

export interface ReferenceListResult {
  references: string[];
}

export async function getRecentlyViewed(signal?: AbortSignal): Promise<ReferenceListResult> {
  const response = await fetch('/api/user/recently-viewed', { signal });
  if (!response.ok) {
    throw new Error(`Recently viewed request failed with status ${response.status}`);
  }
  return (await response.json()) as ReferenceListResult;
}

export async function recordRecentlyViewed(reference: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch('/api/user/recently-viewed', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ reference }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`Record recently viewed request failed with status ${response.status}`);
  }
}

export async function getFavourites(signal?: AbortSignal): Promise<ReferenceListResult> {
  const response = await fetch('/api/user/favourites', { signal });
  if (!response.ok) {
    throw new Error(`Favourites request failed with status ${response.status}`);
  }
  return (await response.json()) as ReferenceListResult;
}

export async function addFavourite(reference: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch('/api/user/favourites', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ reference }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`Add favourite request failed with status ${response.status}`);
  }
}

export async function removeFavourite(reference: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch('/api/user/favourites', {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ reference }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`Remove favourite request failed with status ${response.status}`);
  }
}

export interface DiagnosticsConfigItem {
  name: string;
  value: string;
  source: string;
  isSensitive: boolean;
}

export interface DiagnosticsResult {
  configuration: DiagnosticsConfigItem[];
  endpoint: string;
  region: string;
  connectivityStatus: string;
  connectivityError: string | null;
  revealAllowed: boolean;
}

export async function getDiagnostics(reveal = false, signal?: AbortSignal): Promise<DiagnosticsResult> {
  const query = new URLSearchParams({ reveal: String(reveal) });
  const response = await fetch(`/api/system/diagnostics?${query}`, { signal });
  if (!response.ok) {
    throw new Error(`Diagnostics request failed with status ${response.status}`);
  }
  return (await response.json()) as DiagnosticsResult;
}

export interface CliSnippetParameter {
  name: string;
  value: string;
  isSensitive: boolean;
}

export interface CliSnippetRequest {
  service: string;
  operation: string;
  parameters: CliSnippetParameter[];
}

export interface CliSnippetResult {
  command: string;
}

export async function getCliSnippet(request: CliSnippetRequest, signal?: AbortSignal): Promise<CliSnippetResult> {
  const response = await fetch('/api/system/cli-snippet', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`CLI snippet request failed with status ${response.status}`);
  }
  return (await response.json()) as CliSnippetResult;
}

export interface BulkActionItemResult {
  resourceId: string;
  succeeded: boolean;
  error: string | null;
}

export interface BulkActionResult {
  operationId: string;
  action: string;
  totalCount: number;
  succeededCount: number;
  failedCount: number;
  overallState: string;
  items: BulkActionItemResult[];
}

export async function executeBulkAction(
  action: string,
  resourceIds: string[],
  signal?: AbortSignal,
): Promise<BulkActionResult> {
  const response = await fetch(`/api/bulk/${encodeURIComponent(action)}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ resourceIds }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`Bulk action request failed with status ${response.status}`);
  }
  return (await response.json()) as BulkActionResult;
}

export interface LambdaFunctionSummaryItem {
  functionName: string;
  runtime: string;
  description: string;
  lastModified: string;
  memorySize: number;
  timeout: number;
}

export interface LambdaFunctionListResult {
  functions: LambdaFunctionSummaryItem[];
}

export async function getLambdaFunctions(signal?: AbortSignal): Promise<LambdaFunctionListResult> {
  const response = await fetch('/api/services/lambda/functions', { signal });
  if (!response.ok) {
    throw new Error(`Lambda functions request failed with status ${response.status}`);
  }
  return (await response.json()) as LambdaFunctionListResult;
}

export interface LambdaFunctionResult {
  functionName: string;
  functionArn: string;
  runtime: string;
  handler: string;
  description: string;
  lastModified: string;
  memorySize: number;
  timeout: number;
  role: string;
}

export async function getLambdaFunction(
  functionName: string,
  signal?: AbortSignal,
): Promise<LambdaFunctionResult> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Lambda function request failed with status ${response.status}`);
  }
  return (await response.json()) as LambdaFunctionResult;
}

export interface LambdaEnvironmentVariableItem {
  name: string;
  value: string;
  isSensitive: boolean;
}

export interface LambdaEnvironmentResult {
  variables: LambdaEnvironmentVariableItem[];
  revealAllowed: boolean;
}

export async function getLambdaEnvironment(
  functionName: string,
  reveal = false,
  signal?: AbortSignal,
): Promise<LambdaEnvironmentResult> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/environment?reveal=${reveal}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Lambda environment request failed with status ${response.status}`);
  }
  return (await response.json()) as LambdaEnvironmentResult;
}

export async function updateLambdaEnvironment(
  functionName: string,
  variables: { name: string; value: string }[],
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/environment`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ variables }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Lambda environment update request failed with status ${response.status}`);
  }
}

export interface LambdaInvocationResult {
  statusCode: number;
  payload: string;
  logTail: string;
  functionError: string;
  durationMs: number;
}

export async function invokeLambdaFunction(
  functionName: string,
  payload: string,
  signal?: AbortSignal,
): Promise<LambdaInvocationResult> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/invocations`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ payload }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Lambda invoke request failed with status ${response.status}`);
  }
  return (await response.json()) as LambdaInvocationResult;
}

export interface LambdaFunctionCreatePayload {
  functionName: string;
  runtime: string;
  handler: string;
  role: string;
  description: string;
  memorySize: number;
  timeout: number;
  zipFileBase64: string;
}

export async function createLambdaFunction(
  payload: LambdaFunctionCreatePayload,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/lambda/functions', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
    signal,
  });
  if (!response.ok) {
    throw new Error(`Lambda create request failed with status ${response.status}`);
  }
}

export interface LambdaFunctionUpdatePayload {
  runtime: string;
  handler: string;
  role: string;
  description: string;
  memorySize: number;
  timeout: number;
  zipFileBase64: string | null;
}

export async function updateLambdaFunction(
  functionName: string,
  payload: LambdaFunctionUpdatePayload,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Lambda update request failed with status ${response.status}`);
  }
}

export async function deleteLambdaFunction(
  functionName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Lambda delete request failed with status ${response.status}`);
  }
}

export interface LambdaTestEventItem {
  name: string;
  payload: string;
}

export interface LambdaTestEventListResult {
  events: LambdaTestEventItem[];
  templates: LambdaTestEventItem[];
}

export async function getLambdaTestEvents(
  functionName: string,
  signal?: AbortSignal,
): Promise<LambdaTestEventListResult> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/test-events`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Lambda test events request failed with status ${response.status}`);
  }
  return (await response.json()) as LambdaTestEventListResult;
}

export async function saveLambdaTestEvent(
  functionName: string,
  name: string,
  payload: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/test-events`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, payload }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Lambda test event save request failed with status ${response.status}`);
  }
}

export async function deleteLambdaTestEvent(
  functionName: string,
  name: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/test-events/${encodeURIComponent(name)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Lambda test event delete request failed with status ${response.status}`);
  }
}

export interface LambdaEventSourceMappingItem {
  uuid: string;
  eventSourceArn: string;
  functionArn: string;
  state: string;
  batchSize: number;
  lastModified: string;
}

export interface LambdaEventSourceMappingListResult {
  mappings: LambdaEventSourceMappingItem[];
}

export async function getLambdaEventSourceMappings(
  functionName: string,
  signal?: AbortSignal,
): Promise<LambdaEventSourceMappingListResult> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/event-source-mappings`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Lambda event source mappings request failed with status ${response.status}`);
  }
  return (await response.json()) as LambdaEventSourceMappingListResult;
}

export async function setLambdaEventSourceMappingState(
  functionName: string,
  uuid: string,
  enabled: boolean,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/event-source-mappings/${encodeURIComponent(uuid)}/state`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ enabled }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(
      `Lambda event source mapping state request failed with status ${response.status}`,
    );
  }
}

export interface LambdaLogEventItem {
  timestamp: string;
  message: string;
  logStreamName: string;
}

export interface LambdaLogEventListResult {
  logGroupName: string;
  events: LambdaLogEventItem[];
}

export async function getLambdaLogEvents(
  functionName: string,
  limit?: number,
  signal?: AbortSignal,
): Promise<LambdaLogEventListResult> {
  const query = limit === undefined ? '' : `?limit=${limit}`;
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/logs${query}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Lambda log events request failed with status ${response.status}`);
  }
  return (await response.json()) as LambdaLogEventListResult;
}

export interface LambdaInvocationMetrics {
  invocationCount: number;
  errorCount: number;
  averageDurationMs: number;
  maxDurationMs: number;
}

export interface LambdaRecentInvocationItem {
  requestId: string;
  timestamp: string;
  durationMs: number;
  hasError: boolean;
}

export interface LambdaInvocationInsightsResult {
  logGroupName: string;
  metrics: LambdaInvocationMetrics;
  recentInvocations: LambdaRecentInvocationItem[];
}

export async function getLambdaInvocationInsights(
  functionName: string,
  limit?: number,
  signal?: AbortSignal,
): Promise<LambdaInvocationInsightsResult> {
  const query = limit === undefined ? '' : `?limit=${limit}`;
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/invocation-insights${query}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Lambda invocation insights request failed with status ${response.status}`);
  }
  return (await response.json()) as LambdaInvocationInsightsResult;
}

export interface LambdaLayerItem {
  arn: string;
  name: string;
  version: string;
}

export interface LambdaLayerListResult {
  layers: LambdaLayerItem[];
}

export async function getLambdaLayers(
  functionName: string,
  signal?: AbortSignal,
): Promise<LambdaLayerListResult> {
  const response = await fetch(
    `/api/services/lambda/functions/${encodeURIComponent(functionName)}/layers`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Lambda layers request failed with status ${response.status}`);
  }
  return (await response.json()) as LambdaLayerListResult;
}

export interface S3BucketSummaryItem {
  name: string;
  creationDate: string;
}

export interface S3BucketListResult {
  buckets: S3BucketSummaryItem[];
}

export async function getS3Buckets(signal?: AbortSignal): Promise<S3BucketListResult> {
  const response = await fetch('/api/services/s3/buckets', { signal });
  if (!response.ok) {
    throw new Error(`S3 buckets request failed with status ${response.status}`);
  }
  return (await response.json()) as S3BucketListResult;
}

export async function createS3Bucket(bucketName: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch('/api/services/s3/buckets', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ bucketName }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`S3 bucket create request failed with status ${response.status}`);
  }
}

export async function deleteS3Bucket(bucketName: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`S3 bucket delete request failed with status ${response.status}`);
  }
}

export interface S3ObjectItem {
  key: string;
  size: number;
  lastModified: string;
}

export interface S3ObjectListingResult {
  prefixes: string[];
  objects: S3ObjectItem[];
}

export async function getS3Objects(
  bucketName: string,
  prefix: string,
  signal?: AbortSignal,
): Promise<S3ObjectListingResult> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/objects?prefix=${encodeURIComponent(prefix)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`S3 objects request failed with status ${response.status}`);
  }
  return (await response.json()) as S3ObjectListingResult;
}

export async function createS3Folder(
  bucketName: string,
  folderKey: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/folders`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ folderKey }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`S3 folder create request failed with status ${response.status}`);
  }
}

export async function uploadS3Object(
  bucketName: string,
  prefix: string,
  file: File,
  signal?: AbortSignal,
): Promise<void> {
  const form = new FormData();
  form.append('file', file);
  form.append('prefix', prefix);
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/objects`,
    {
      method: 'POST',
      body: form,
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`S3 object upload request failed with status ${response.status}`);
  }
}

export async function deleteS3Object(
  bucketName: string,
  key: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/objects?key=${encodeURIComponent(key)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`S3 object delete request failed with status ${response.status}`);
  }
}

export function s3ObjectDownloadUrl(bucketName: string, key: string): string {
  return `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/objects/content?key=${encodeURIComponent(key)}`;
}

export interface S3ObjectPreviewResult {
  kind: string;
  contentType: string;
  truncated: boolean;
  totalSize: number;
  text: string | null;
  dataUrl: string | null;
}

export async function getS3ObjectPreview(
  bucketName: string,
  key: string,
  signal?: AbortSignal,
): Promise<S3ObjectPreviewResult> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/objects/preview?key=${encodeURIComponent(key)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`S3 object preview request failed with status ${response.status}`);
  }
  return (await response.json()) as S3ObjectPreviewResult;
}

export interface S3PresignedUrlResult {
  url: string;
  expirySeconds: number;
}

export async function getS3PresignedUrl(
  bucketName: string,
  key: string,
  expirySeconds: number,
  signal?: AbortSignal,
): Promise<S3PresignedUrlResult> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/objects/presign?key=${encodeURIComponent(key)}&expirySeconds=${expirySeconds}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`S3 presigned URL request failed with status ${response.status}`);
  }
  return (await response.json()) as S3PresignedUrlResult;
}

export interface S3MetadataEntry {
  key: string;
  value: string;
}

export interface S3ObjectMetadataResult {
  contentType: string;
  contentLength: number;
  lastModified: string;
  eTag: string;
  metadata: S3MetadataEntry[];
  tags: S3MetadataEntry[];
}

export async function getS3ObjectMetadata(
  bucketName: string,
  key: string,
  signal?: AbortSignal,
): Promise<S3ObjectMetadataResult> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/objects/metadata?key=${encodeURIComponent(key)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`S3 object metadata request failed with status ${response.status}`);
  }
  return (await response.json()) as S3ObjectMetadataResult;
}

export async function updateS3ObjectTags(
  bucketName: string,
  key: string,
  tags: Record<string, string>,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/objects/tags?key=${encodeURIComponent(key)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ tags }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`S3 object tags update request failed with status ${response.status}`);
  }
}

export async function copyS3Object(
  bucketName: string,
  key: string,
  destinationBucketName: string,
  destinationKey: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/objects/copy?key=${encodeURIComponent(key)}`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ destinationBucketName, destinationKey }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`S3 object copy request failed with status ${response.status}`);
  }
}

export async function moveS3Object(
  bucketName: string,
  key: string,
  destinationBucketName: string,
  destinationKey: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/objects/move?key=${encodeURIComponent(key)}`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ destinationBucketName, destinationKey }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`S3 object move request failed with status ${response.status}`);
  }
}

export interface S3LifecycleRuleResult {
  id: string;
  status: string;
  prefix: string;
}

export interface S3NotificationResult {
  type: string;
  targetArn: string;
  events: string[];
}

export interface S3BucketConfigurationResult {
  versioningStatus: string;
  encryptionAlgorithm: string;
  encryptionKeyId: string;
  lifecycleRules: S3LifecycleRuleResult[];
  notifications: S3NotificationResult[];
  policy: string;
}

export async function getS3BucketConfiguration(
  bucketName: string,
  signal?: AbortSignal,
): Promise<S3BucketConfigurationResult> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/configuration`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`S3 bucket configuration request failed with status ${response.status}`);
  }
  return (await response.json()) as S3BucketConfigurationResult;
}

export interface S3BucketStorageSummaryResult {
  objectCount: number;
  totalSizeBytes: number;
}

export async function getS3BucketStorageSummary(
  bucketName: string,
  signal?: AbortSignal,
): Promise<S3BucketStorageSummaryResult> {
  const response = await fetch(
    `/api/services/s3/buckets/${encodeURIComponent(bucketName)}/storage-summary`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`S3 bucket storage summary request failed with status ${response.status}`);
  }
  return (await response.json()) as S3BucketStorageSummaryResult;
}