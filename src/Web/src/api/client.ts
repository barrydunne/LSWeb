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

export interface SqsQueueSummaryItem {
  name: string;
  url: string;
  approximateMessageCount: number;
  approximateInFlightCount: number;
  approximateDelayedCount: number;
}

export interface SqsQueueListResult {
  queues: SqsQueueSummaryItem[];
}

export async function getSqsQueues(signal?: AbortSignal): Promise<SqsQueueListResult> {
  const response = await fetch('/api/services/sqs/queues', { signal });
  if (!response.ok) {
    throw new Error(`SQS queues request failed with status ${response.status}`);
  }
  return (await response.json()) as SqsQueueListResult;
}

export async function createSqsQueue(
  queueName: string,
  fifoQueue: boolean,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/sqs/queues', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ queueName, fifoQueue }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`SQS queue create request failed with status ${response.status}`);
  }
}

export async function deleteSqsQueue(queueName: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`SQS queue delete request failed with status ${response.status}`);
  }
}

export type SqsPollMode = 'peek' | 'consume';

export interface SqsMessageItem {
  messageId: string;
  receiptHandle: string;
  body: string;
  attributes: Record<string, string>;
  messageAttributes: Record<string, string>;
}

export interface SqsMessageListResult {
  messages: SqsMessageItem[];
}

export async function pollSqsMessages(
  queueName: string,
  mode: SqsPollMode,
  maxMessages?: number,
  signal?: AbortSignal,
): Promise<SqsMessageListResult> {
  const params = new URLSearchParams({ mode });
  if (maxMessages !== undefined) {
    params.set('maxMessages', String(maxMessages));
  }
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}/messages?${params.toString()}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`SQS poll request failed with status ${response.status}`);
  }
  return (await response.json()) as SqsMessageListResult;
}

export async function deleteSqsMessage(
  queueName: string,
  receiptHandle: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}/messages?receiptHandle=${encodeURIComponent(receiptHandle)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`SQS delete request failed with status ${response.status}`);
  }
}

export async function purgeSqsQueue(queueName: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}/purge`,
    {
      method: 'POST',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`SQS purge request failed with status ${response.status}`);
  }
}

export interface SqsSendMessageInput {
  body: string;
  messageAttributes?: Record<string, string>;
  messageGroupId?: string;
  messageDeduplicationId?: string;
}

export async function sendSqsMessage(
  queueName: string,
  input: SqsSendMessageInput,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}/messages`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        body: input.body,
        messageAttributes: input.messageAttributes,
        messageGroupId: input.messageGroupId,
        messageDeduplicationId: input.messageDeduplicationId,
      }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`SQS send request failed with status ${response.status}`);
  }
}

export interface SqsSubscriptionItem {
  topicArn: string;
  topicName: string;
}

export interface SqsSubscriptionListResult {
  subscriptions: SqsSubscriptionItem[];
}

export async function getSqsQueueSubscriptions(
  queueName: string,
  signal?: AbortSignal,
): Promise<SqsSubscriptionListResult> {
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}/subscriptions`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`SQS subscriptions request failed with status ${response.status}`);
  }
  return (await response.json()) as SqsSubscriptionListResult;
}

export interface SqsQueueAttributesItem {
  visibilityTimeoutSeconds: number;
  messageRetentionPeriodSeconds: number;
  delaySeconds: number;
  receiveMessageWaitTimeSeconds: number;
  maximumMessageSizeBytes: number;
  queueArn: string;
  fifoQueue: boolean;
}

export interface SqsQueueAttributesUpdateInput {
  visibilityTimeoutSeconds: number;
  messageRetentionPeriodSeconds: number;
  delaySeconds: number;
  receiveMessageWaitTimeSeconds: number;
}

export async function getSqsQueueAttributes(
  queueName: string,
  signal?: AbortSignal,
): Promise<SqsQueueAttributesItem> {
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}/attributes`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`SQS attributes request failed with status ${response.status}`);
  }
  return (await response.json()) as SqsQueueAttributesItem;
}

export async function updateSqsQueueAttributes(
  queueName: string,
  input: SqsQueueAttributesUpdateInput,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}/attributes`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        visibilityTimeoutSeconds: input.visibilityTimeoutSeconds,
        messageRetentionPeriodSeconds: input.messageRetentionPeriodSeconds,
        delaySeconds: input.delaySeconds,
        receiveMessageWaitTimeSeconds: input.receiveMessageWaitTimeSeconds,
      }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`SQS attributes update request failed with status ${response.status}`);
  }
}

export interface SqsRedriveTarget {
  queueArn: string;
  queueName: string;
  maxReceiveCount: number;
}

export interface SqsRedriveSource {
  queueArn: string;
  queueName: string;
}

export interface SqsRedriveResult {
  deadLetterTarget: SqsRedriveTarget | null;
  sources: SqsRedriveSource[];
}

export async function getSqsQueueRedrive(
  queueName: string,
  signal?: AbortSignal,
): Promise<SqsRedriveResult> {
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}/redrive`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`SQS redrive request failed with status ${response.status}`);
  }
  return (await response.json()) as SqsRedriveResult;
}

export async function redriveSqsQueue(queueName: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}/redrive`,
    { method: 'POST', signal },
  );
  if (!response.ok) {
    throw new Error(`SQS redrive start request failed with status ${response.status}`);
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

export interface LogGroupItem {
  name: string;
  arn: string;
  storedBytes: number;
  retentionInDays: number | null;
  createdAt: string | null;
}

export interface LogGroupListResult {
  logGroups: LogGroupItem[];
}

export async function getLogGroups(signal?: AbortSignal): Promise<LogGroupListResult> {
  const response = await fetch('/api/services/cloudwatch-logs/groups', { signal });
  if (!response.ok) {
    throw new Error(`Log groups request failed with status ${response.status}`);
  }
  return (await response.json()) as LogGroupListResult;
}

export async function createLogGroup(
  logGroupName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/cloudwatch-logs/groups', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ logGroupName }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`Log group create request failed with status ${response.status}`);
  }
}

export async function deleteLogGroup(
  logGroupName: string,
  signal?: AbortSignal,
): Promise<void> {
  const params = new URLSearchParams({ logGroupName });
  const response = await fetch(
    `/api/services/cloudwatch-logs/groups?${params.toString()}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Log group delete request failed with status ${response.status}`);
  }
}

export interface LogStreamItem {
  name: string;
  lastEventTimestamp: string | null;
}

export interface LogStreamListResult {
  logStreams: LogStreamItem[];
}

export async function getLogStreams(
  logGroupName: string,
  signal?: AbortSignal,
): Promise<LogStreamListResult> {
  const params = new URLSearchParams({ logGroupName });
  const response = await fetch(
    `/api/services/cloudwatch-logs/streams?${params.toString()}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Log streams request failed with status ${response.status}`);
  }
  return (await response.json()) as LogStreamListResult;
}

export interface LogEventItem {
  timestamp: string;
  message: string;
}

export interface LogEventListResult {
  events: LogEventItem[];
}

export async function getLogEvents(
  logGroupName: string,
  logStreamName: string,
  signal?: AbortSignal,
): Promise<LogEventListResult> {
  const params = new URLSearchParams({ logGroupName, logStreamName });
  const response = await fetch(
    `/api/services/cloudwatch-logs/events?${params.toString()}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Log events request failed with status ${response.status}`);
  }
  return (await response.json()) as LogEventListResult;
}

export async function filterLogEvents(
  logGroupName: string,
  filterPattern?: string,
  startTime?: number,
  signal?: AbortSignal,
): Promise<LogEventListResult> {
  const params = new URLSearchParams({ logGroupName });
  if (filterPattern) {
    params.set('filterPattern', filterPattern);
  }
  if (startTime !== undefined) {
    params.set('startTime', String(startTime));
  }
  const response = await fetch(
    `/api/services/cloudwatch-logs/filter?${params.toString()}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Log filter request failed with status ${response.status}`);
  }
  return (await response.json()) as LogEventListResult;
}

export interface DynamoDbTableItem {
  name: string;
}

export interface DynamoDbTableListResult {
  tables: DynamoDbTableItem[];
}

export interface DynamoDbKeyAttribute {
  attributeName: string;
  keyType: string;
}

export interface DynamoDbAttributeDefinition {
  attributeName: string;
  attributeType: string;
}

export interface DynamoDbSecondaryIndex {
  name: string;
  status: string | null;
  keySchema: DynamoDbKeyAttribute[];
}

export interface DynamoDbTableDetail {
  name: string;
  arn: string;
  status: string;
  itemCount: number;
  tableSizeBytes: number;
  billingMode: string | null;
  readCapacityUnits: number | null;
  writeCapacityUnits: number | null;
  createdAt: string | null;
  keySchema: DynamoDbKeyAttribute[];
  attributes: DynamoDbAttributeDefinition[];
  globalSecondaryIndexes: DynamoDbSecondaryIndex[];
  localSecondaryIndexes: DynamoDbSecondaryIndex[];
  streamEnabled: boolean;
  streamViewType: string | null;
  latestStreamArn: string | null;
}

export async function getDynamoDbTables(signal?: AbortSignal): Promise<DynamoDbTableListResult> {
  const response = await fetch('/api/services/dynamodb/tables', { signal });
  if (!response.ok) {
    throw new Error(`DynamoDB tables request failed with status ${response.status}`);
  }
  return (await response.json()) as DynamoDbTableListResult;
}

export async function getDynamoDbTable(
  tableName: string,
  signal?: AbortSignal,
): Promise<DynamoDbTableDetail> {
  const response = await fetch(
    `/api/services/dynamodb/tables/${encodeURIComponent(tableName)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`DynamoDB table request failed with status ${response.status}`);
  }
  return (await response.json()) as DynamoDbTableDetail;
}

export interface DynamoDbTableCreateRequest {
  tableName: string;
  partitionKeyName: string;
  partitionKeyType: string;
  sortKeyName: string | null;
  sortKeyType: string | null;
  billingMode: string;
  readCapacityUnits: number | null;
  writeCapacityUnits: number | null;
}

export async function createDynamoDbTable(
  request: DynamoDbTableCreateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/dynamodb/tables', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`DynamoDB table create request failed with status ${response.status}`);
  }
}

export async function deleteDynamoDbTable(
  tableName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/dynamodb/tables/${encodeURIComponent(tableName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`DynamoDB table delete request failed with status ${response.status}`);
  }
}

export interface DynamoDbItem {
  json: string;
}

export interface DynamoDbItemListResult {
  items: DynamoDbItem[];
  truncated: boolean;
}

export async function scanDynamoDbItems(
  tableName: string,
  limit: number,
  signal?: AbortSignal,
): Promise<DynamoDbItemListResult> {
  const response = await fetch(
    `/api/services/dynamodb/tables/${encodeURIComponent(tableName)}/items?limit=${limit}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`DynamoDB item scan request failed with status ${response.status}`);
  }
  return (await response.json()) as DynamoDbItemListResult;
}

export async function getDynamoDbItem(
  tableName: string,
  keyJson: string,
  signal?: AbortSignal,
): Promise<DynamoDbItem> {
  const response = await fetch(
    `/api/services/dynamodb/tables/${encodeURIComponent(tableName)}/item?key=${encodeURIComponent(keyJson)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`DynamoDB item request failed with status ${response.status}`);
  }
  return (await response.json()) as DynamoDbItem;
}

export async function putDynamoDbItem(
  tableName: string,
  itemJson: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/dynamodb/tables/${encodeURIComponent(tableName)}/items`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ item: itemJson }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`DynamoDB item put request failed with status ${response.status}`);
  }
}

export async function deleteDynamoDbItem(
  tableName: string,
  keyJson: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/dynamodb/tables/${encodeURIComponent(tableName)}/item?key=${encodeURIComponent(keyJson)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`DynamoDB item delete request failed with status ${response.status}`);
  }
}

export interface DynamoDbQueryCondition {
  attributeName: string;
  operator: string;
  valueType: string;
  value: string;
  secondValue: string | null;
}

export interface DynamoDbQueryRequest {
  indexName: string | null;
  scan: boolean;
  partitionKey: DynamoDbQueryCondition | null;
  sortKey: DynamoDbQueryCondition | null;
  filters: DynamoDbQueryCondition[];
  limit: number;
  startToken: string | null;
}

export interface DynamoDbQueryResult {
  items: DynamoDbItem[];
  nextToken: string | null;
}

export async function queryDynamoDbTable(
  tableName: string,
  request: DynamoDbQueryRequest,
  signal?: AbortSignal,
): Promise<DynamoDbQueryResult> {
  const response = await fetch(
    `/api/services/dynamodb/tables/${encodeURIComponent(tableName)}/query`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`DynamoDB query request failed with status ${response.status}`);
  }
  return (await response.json()) as DynamoDbQueryResult;
}

export interface DynamoDbStatementRequest {
  statement: string;
  limit: number;
  nextToken: string | null;
}

export interface DynamoDbStatementResult {
  items: DynamoDbItem[];
  nextToken: string | null;
}

export async function executeDynamoDbStatement(
  request: DynamoDbStatementRequest,
  signal?: AbortSignal,
): Promise<DynamoDbStatementResult> {
  const response = await fetch('/api/services/dynamodb/statement', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`DynamoDB statement request failed with status ${response.status}`);
  }
  return (await response.json()) as DynamoDbStatementResult;
}

export interface SecretItem {
  name: string;
  arn: string;
  description: string | null;
  createdDate: string | null;
  lastChangedDate: string | null;
}

export interface SecretListResult {
  secrets: SecretItem[];
}

export async function getSecrets(signal?: AbortSignal): Promise<SecretListResult> {
  const response = await fetch('/api/services/secrets-manager/secrets', { signal });
  if (!response.ok) {
    throw new Error(`Secrets Manager secrets request failed with status ${response.status}`);
  }
  return (await response.json()) as SecretListResult;
}

export interface SecretCreateRequest {
  name: string;
  description: string | null;
  secretString: string;
}

export async function createSecret(
  request: SecretCreateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/secrets-manager/secrets', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`Secrets Manager secret create request failed with status ${response.status}`);
  }
}

export async function deleteSecret(
  secretId: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/secrets-manager/secrets/${encodeURIComponent(secretId)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Secrets Manager secret delete request failed with status ${response.status}`);
  }
}

export interface SecretValueResult {
  name: string;
  arn: string;
  versionId: string | null;
  value: string;
  revealAllowed: boolean;
}

export async function getSecretValue(
  secretId: string,
  reveal: boolean,
  signal?: AbortSignal,
): Promise<SecretValueResult> {
  const response = await fetch(
    `/api/services/secrets-manager/secrets/${encodeURIComponent(secretId)}/value?reveal=${reveal}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Secrets Manager secret value request failed with status ${response.status}`);
  }
  return (await response.json()) as SecretValueResult;
}

export interface SecretValueUpdateRequest {
  secretString: string;
}

export async function putSecretValue(
  secretId: string,
  request: SecretValueUpdateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/secrets-manager/secrets/${encodeURIComponent(secretId)}/value`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Secrets Manager secret value update request failed with status ${response.status}`);
  }
}

export interface SecretVersionItem {
  versionId: string;
  stages: string[];
  createdDate: string | null;
  lastAccessedDate: string | null;
}

export interface SecretVersionListResult {
  name: string;
  arn: string;
  versions: SecretVersionItem[];
}

export async function getSecretVersions(
  secretId: string,
  signal?: AbortSignal,
): Promise<SecretVersionListResult> {
  const response = await fetch(
    `/api/services/secrets-manager/secrets/${encodeURIComponent(secretId)}/versions`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Secrets Manager secret versions request failed with status ${response.status}`);
  }
  return (await response.json()) as SecretVersionListResult;
}

export interface ParameterItem {
  name: string;
  type: string;
  version: number;
  lastModifiedDate: string | null;
  arn: string;
}

export interface ParameterListResult {
  path: string;
  parameters: ParameterItem[];
}

export async function getParameters(
  path: string,
  recursive: boolean,
  signal?: AbortSignal,
): Promise<ParameterListResult> {
  const response = await fetch(
    `/api/services/ssm-parameter-store/parameters?path=${encodeURIComponent(path)}&recursive=${recursive}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`SSM parameters request failed with status ${response.status}`);
  }
  return (await response.json()) as ParameterListResult;
}

export interface ParameterCreateRequest {
  name: string;
  type: string;
  value: string;
  description: string | null;
}

export async function createParameter(
  request: ParameterCreateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/ssm-parameter-store/parameters', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`SSM parameter create request failed with status ${response.status}`);
  }
}

export async function deleteParameter(
  name: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/ssm-parameter-store/parameters?name=${encodeURIComponent(name)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`SSM parameter delete request failed with status ${response.status}`);
  }
}

export interface ParameterValueResult {
  name: string;
  type: string;
  version: number;
  value: string;
  isSensitive: boolean;
  revealAllowed: boolean;
}

export async function getParameterValue(
  name: string,
  reveal?: boolean,
  signal?: AbortSignal,
): Promise<ParameterValueResult> {
  const query = reveal === true
    ? `?name=${encodeURIComponent(name)}&reveal=true`
    : `?name=${encodeURIComponent(name)}`;
  const response = await fetch(
    `/api/services/ssm-parameter-store/parameters/value${query}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`SSM parameter value request failed with status ${response.status}`);
  }
  return (await response.json()) as ParameterValueResult;
}

export async function updateParameterValue(
  name: string,
  value: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/ssm-parameter-store/parameters/value?name=${encodeURIComponent(name)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ value }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`SSM parameter value update request failed with status ${response.status}`);
  }
}

export interface ParameterHistoryEntry {
  type: string;
  version: number;
  value: string;
  lastModifiedDate: string | null;
  lastModifiedUser: string;
  isSensitive: boolean;
}

export interface ParameterHistoryResult {
  name: string;
  revealAllowed: boolean;
  entries: ParameterHistoryEntry[];
}

export async function getParameterHistory(
  name: string,
  reveal?: boolean,
  signal?: AbortSignal,
): Promise<ParameterHistoryResult> {
  const query = reveal === true
    ? `?name=${encodeURIComponent(name)}&reveal=true`
    : `?name=${encodeURIComponent(name)}`;
  const response = await fetch(
    `/api/services/ssm-parameter-store/parameters/history${query}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`SSM parameter history request failed with status ${response.status}`);
  }
  return (await response.json()) as ParameterHistoryResult;
}

export interface SnsTopicItem {
  name: string;
  topicArn: string;
}

export interface SnsTopicListResult {
  topics: SnsTopicItem[];
}

export async function getSnsTopics(signal?: AbortSignal): Promise<SnsTopicListResult> {
  const response = await fetch('/api/services/sns/topics', { signal });
  if (!response.ok) {
    throw new Error(`SNS topics request failed with status ${response.status}`);
  }
  return (await response.json()) as SnsTopicListResult;
}

export interface SnsTopicCreateRequest {
  name: string;
}

export async function createSnsTopic(
  request: SnsTopicCreateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/sns/topics', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`SNS topic create request failed with status ${response.status}`);
  }
}

export async function deleteSnsTopic(arn: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(`/api/services/sns/topics?arn=${encodeURIComponent(arn)}`, {
    method: 'DELETE',
    signal,
  });
  if (!response.ok) {
    throw new Error(`SNS topic delete request failed with status ${response.status}`);
  }
}

export interface SnsSubscriptionItem {
  subscriptionArn: string;
  protocol: string;
  endpoint: string;
  owner: string;
}

export interface SnsSubscriptionListResult {
  subscriptions: SnsSubscriptionItem[];
}

export async function getSnsSubscriptions(
  topicArn: string,
  signal?: AbortSignal,
): Promise<SnsSubscriptionListResult> {
  const response = await fetch(
    `/api/services/sns/subscriptions?arn=${encodeURIComponent(topicArn)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`SNS subscriptions request failed with status ${response.status}`);
  }
  return (await response.json()) as SnsSubscriptionListResult;
}