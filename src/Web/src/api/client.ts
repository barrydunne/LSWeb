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

export interface LambdaS3TriggerItem {
  bucketArn: string;
}

export interface LambdaEventSourceMappingListResult {
  mappings: LambdaEventSourceMappingItem[];
  s3Triggers: LambdaS3TriggerItem[];
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

export interface SqsConsumerLambdaItem {
  functionName: string;
  functionArn: string;
  state: string;
}

export interface SqsConsumerLambdaListResult {
  lambdas: SqsConsumerLambdaItem[];
}

export async function getSqsQueueConsumerLambdas(
  queueName: string,
  signal?: AbortSignal,
): Promise<SqsConsumerLambdaListResult> {
  const response = await fetch(
    `/api/services/sqs/queues/${encodeURIComponent(queueName)}/lambda-triggers`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`SQS consumer Lambdas request failed with status ${response.status}`);
  }
  return (await response.json()) as SqsConsumerLambdaListResult;
}

export interface SqsQueueAttributesItem {
  visibilityTimeoutSeconds: number;
  messageRetentionPeriodSeconds: number;
  delaySeconds: number;
  receiveMessageWaitTimeSeconds: number;
  maximumMessageSizeBytes: number;
  queueArn: string;
  fifoQueue: boolean;
  approximateMessageCount: number;
  approximateInFlightCount: number;
  approximateDelayedCount: number;
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
  prefix: string;
  suffix: string;
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

export interface SnsPublishMessageInput {
  subject?: string;
  message: string;
  messageAttributes?: Record<string, string>;
}

export async function publishSnsMessage(
  topicArn: string,
  input: SnsPublishMessageInput,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/sns/topics/messages', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      topicArn,
      subject: input.subject,
      message: input.message,
      messageAttributes: input.messageAttributes,
    }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`SNS publish request failed with status ${response.status}`);
  }
}

export interface SnsSubscriptionFilterPolicyResult {
  filterPolicy: string;
}

export async function getSnsSubscriptionFilterPolicy(
  subscriptionArn: string,
  signal?: AbortSignal,
): Promise<SnsSubscriptionFilterPolicyResult> {
  const response = await fetch(
    `/api/services/sns/subscriptions/filter-policy?arn=${encodeURIComponent(subscriptionArn)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`SNS filter policy request failed with status ${response.status}`);
  }
  return (await response.json()) as SnsSubscriptionFilterPolicyResult;
}

export async function setSnsSubscriptionFilterPolicy(
  subscriptionArn: string,
  filterPolicy: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/sns/subscriptions/filter-policy', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ subscriptionArn, filterPolicy }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`SNS filter policy update request failed with status ${response.status}`);
  }
}

export interface IamUserSummary {
  userName: string;
  arn: string;
  userId: string;
  path: string;
  createDate: string | null;
}

export interface IamUserListResult {
  users: IamUserSummary[];
}

export async function getIamUsers(signal?: AbortSignal): Promise<IamUserListResult> {
  const response = await fetch('/api/services/iam/users', { signal });
  if (!response.ok) {
    throw new Error(`IAM users request failed with status ${response.status}`);
  }
  return (await response.json()) as IamUserListResult;
}

export interface IamAttachedPolicy {
  policyName: string;
  policyArn: string;
}

export interface IamTag {
  key: string;
  value: string;
}

export interface IamAccessKey {
  accessKeyId: string;
  status: string;
  createDate: string | null;
  lastUsedDate: string | null;
  lastUsedService: string | null;
  lastUsedRegion: string | null;
}

export interface IamUserDetail {
  userName: string;
  arn: string;
  userId: string;
  path: string;
  createDate: string | null;
  groups: string[];
  attachedPolicies: IamAttachedPolicy[];
  inlinePolicyNames: string[];
  accessKeys: IamAccessKey[];
  tags: IamTag[];
  permissionsBoundaryArn: string | null;
}

export async function getIamUser(userName: string, signal?: AbortSignal): Promise<IamUserDetail> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`IAM user request failed with status ${response.status}`);
  }
  return (await response.json()) as IamUserDetail;
}

export interface IamUserCreateRequest {
  userName: string;
  path: string | null;
}

export async function createIamUser(
  request: IamUserCreateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/iam/users', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`IAM user create request failed with status ${response.status}`);
  }
}

export async function deleteIamUser(userName: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM user delete request failed with status ${response.status}`);
  }
}

export async function addIamUserToGroup(
  userName: string,
  groupName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/groups`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ groupName }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM add-user-to-group request failed with status ${response.status}`);
  }
}

export async function removeIamUserFromGroup(
  userName: string,
  groupName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/groups/${encodeURIComponent(groupName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM remove-user-from-group request failed with status ${response.status}`);
  }
}

export async function attachIamUserPolicy(
  userName: string,
  policyArn: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/attached-policies`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyArn }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM attach-user-policy request failed with status ${response.status}`);
  }
}

export async function detachIamUserPolicy(
  userName: string,
  policyArn: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/attached-policies?policyArn=${encodeURIComponent(policyArn)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM detach-user-policy request failed with status ${response.status}`);
  }
}

export async function putIamUserInlinePolicy(
  userName: string,
  policyName: string,
  policyDocument: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/inline-policies/${encodeURIComponent(policyName)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyDocument }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM put-user-inline-policy request failed with status ${response.status}`);
  }
}

export async function deleteIamUserInlinePolicy(
  userName: string,
  policyName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/inline-policies/${encodeURIComponent(policyName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM delete-user-inline-policy request failed with status ${response.status}`);
  }
}

export interface IamAccessKeySecret {
  accessKeyId: string;
  secretAccessKey: string;
  status: string;
  createDate: string | null;
}

export async function createIamAccessKey(
  userName: string,
  signal?: AbortSignal,
): Promise<IamAccessKeySecret> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/access-keys`,
    {
      method: 'POST',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM create-access-key request failed with status ${response.status}`);
  }
  return (await response.json()) as IamAccessKeySecret;
}

export async function updateIamAccessKeyStatus(
  userName: string,
  accessKeyId: string,
  status: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/access-keys/${encodeURIComponent(accessKeyId)}/status`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM update-access-key-status request failed with status ${response.status}`);
  }
}

export async function deleteIamAccessKey(
  userName: string,
  accessKeyId: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/access-keys/${encodeURIComponent(accessKeyId)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM delete-access-key request failed with status ${response.status}`);
  }
}

export interface IamGroupSummary {
  groupName: string;
  arn: string;
  groupId: string;
  path: string;
  createDate: string | null;
}

export interface IamGroupListResult {
  groups: IamGroupSummary[];
}

export async function getIamGroups(signal?: AbortSignal): Promise<IamGroupListResult> {
  const response = await fetch('/api/services/iam/groups', { signal });
  if (!response.ok) {
    throw new Error(`IAM groups request failed with status ${response.status}`);
  }
  return (await response.json()) as IamGroupListResult;
}

export interface IamInlinePolicy {
  policyName: string;
  policyDocument: string;
}

export interface IamGroupDetail {
  groupName: string;
  arn: string;
  groupId: string;
  path: string;
  createDate: string | null;
  members: string[];
  attachedPolicies: IamAttachedPolicy[];
  inlinePolicies: IamInlinePolicy[];
}

export async function getIamGroup(groupName: string, signal?: AbortSignal): Promise<IamGroupDetail> {
  const response = await fetch(
    `/api/services/iam/groups/${encodeURIComponent(groupName)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`IAM group request failed with status ${response.status}`);
  }
  return (await response.json()) as IamGroupDetail;
}

export interface IamGroupCreateRequest {
  groupName: string;
  path: string | null;
}

export async function createIamGroup(
  request: IamGroupCreateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/iam/groups', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`IAM group create request failed with status ${response.status}`);
  }
}

export async function deleteIamGroup(groupName: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(
    `/api/services/iam/groups/${encodeURIComponent(groupName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM group delete request failed with status ${response.status}`);
  }
}

export async function addIamGroupMember(
  groupName: string,
  userName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/groups/${encodeURIComponent(groupName)}/members`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ userName }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM add-group-member request failed with status ${response.status}`);
  }
}

export async function removeIamGroupMember(
  groupName: string,
  userName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/groups/${encodeURIComponent(groupName)}/members/${encodeURIComponent(userName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM remove-group-member request failed with status ${response.status}`);
  }
}

export async function attachIamGroupPolicy(
  groupName: string,
  policyArn: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/groups/${encodeURIComponent(groupName)}/attached-policies`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyArn }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM attach-group-policy request failed with status ${response.status}`);
  }
}

export async function detachIamGroupPolicy(
  groupName: string,
  policyArn: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/groups/${encodeURIComponent(groupName)}/attached-policies?policyArn=${encodeURIComponent(policyArn)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM detach-group-policy request failed with status ${response.status}`);
  }
}

export async function putIamGroupInlinePolicy(
  groupName: string,
  policyName: string,
  policyDocument: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/groups/${encodeURIComponent(groupName)}/inline-policies/${encodeURIComponent(policyName)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyDocument }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM put-group-inline-policy request failed with status ${response.status}`);
  }
}

export async function deleteIamGroupInlinePolicy(
  groupName: string,
  policyName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/groups/${encodeURIComponent(groupName)}/inline-policies/${encodeURIComponent(policyName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM delete-group-inline-policy request failed with status ${response.status}`);
  }
}

export interface IamRoleSummary {
  roleName: string;
  arn: string;
  roleId: string;
  path: string;
  createDate: string | null;
  description: string | null;
}

export interface IamRoleListResult {
  roles: IamRoleSummary[];
}

export async function getIamRoles(signal?: AbortSignal): Promise<IamRoleListResult> {
  const response = await fetch('/api/services/iam/roles', { signal });
  if (!response.ok) {
    throw new Error(`IAM roles request failed with status ${response.status}`);
  }
  return (await response.json()) as IamRoleListResult;
}

export interface IamRoleDetail {
  roleName: string;
  arn: string;
  roleId: string;
  path: string;
  createDate: string | null;
  description: string | null;
  maxSessionDuration: number | null;
  assumeRolePolicyDocument: string;
  attachedPolicies: IamAttachedPolicy[];
  inlinePolicies: IamInlinePolicy[];
  tags: IamTag[];
  permissionsBoundaryArn: string | null;
}

export async function getIamRole(roleName: string, signal?: AbortSignal): Promise<IamRoleDetail> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`IAM role request failed with status ${response.status}`);
  }
  return (await response.json()) as IamRoleDetail;
}

export interface IamRoleConsumer {
  consumerType: string;
  resourceName: string;
  serviceKey: string;
}

export interface IamRoleConsumersResult {
  consumers: IamRoleConsumer[];
}

export async function getIamRoleUsedBy(
  roleName: string,
  signal?: AbortSignal,
): Promise<IamRoleConsumersResult> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}/used-by`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`IAM role used-by request failed with status ${response.status}`);
  }
  return (await response.json()) as IamRoleConsumersResult;
}

export interface IamRoleCreateRequest {
  roleName: string;
  assumeRolePolicyDocument: string;
  path: string | null;
  description: string | null;
  maxSessionDuration: number | null;
}

export async function createIamRole(
  request: IamRoleCreateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/iam/roles', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`IAM role create request failed with status ${response.status}`);
  }
}

export interface IamRoleUpdateRequest {
  description: string | null;
  maxSessionDuration: number | null;
  trustPolicyDocument: string | null;
}

export async function updateIamRole(
  roleName: string,
  request: IamRoleUpdateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM role update request failed with status ${response.status}`);
  }
}

export async function deleteIamRole(roleName: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM role delete request failed with status ${response.status}`);
  }
}

export async function attachIamRolePolicy(
  roleName: string,
  policyArn: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}/attached-policies`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyArn }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM attach-role-policy request failed with status ${response.status}`);
  }
}

export async function detachIamRolePolicy(
  roleName: string,
  policyArn: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}/attached-policies?policyArn=${encodeURIComponent(policyArn)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM detach-role-policy request failed with status ${response.status}`);
  }
}

export async function putIamRoleInlinePolicy(
  roleName: string,
  policyName: string,
  policyDocument: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}/inline-policies/${encodeURIComponent(policyName)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyDocument }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM put-role-inline-policy request failed with status ${response.status}`);
  }
}

export async function deleteIamRoleInlinePolicy(
  roleName: string,
  policyName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}/inline-policies/${encodeURIComponent(policyName)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM delete-role-inline-policy request failed with status ${response.status}`);
  }
}

export interface IamPolicySummary {
  policyName: string;
  arn: string;
  policyId: string;
  path: string;
  defaultVersionId: string;
  attachmentCount: number;
  isAttachable: boolean;
  description: string | null;
  createDate: string | null;
  updateDate: string | null;
}

export interface IamPolicyListResult {
  policies: IamPolicySummary[];
}

export type IamPolicyScope = 'local' | 'aws';

export async function getIamPolicies(
  scope: IamPolicyScope,
  signal?: AbortSignal,
): Promise<IamPolicyListResult> {
  const response = await fetch(`/api/services/iam/policies?scope=${scope}`, { signal });
  if (!response.ok) {
    throw new Error(`IAM policies request failed with status ${response.status}`);
  }
  return (await response.json()) as IamPolicyListResult;
}

export interface IamPolicyVersion {
  versionId: string;
  isDefaultVersion: boolean;
  createDate: string | null;
}

export interface IamPolicyDetail {
  policyName: string;
  arn: string;
  policyId: string;
  path: string;
  defaultVersionId: string;
  attachmentCount: number;
  isAttachable: boolean;
  description: string | null;
  createDate: string | null;
  updateDate: string | null;
  defaultVersionDocument: string;
  versions: IamPolicyVersion[];
  tags: IamTag[];
}

export async function getIamPolicy(
  policyArn: string,
  signal?: AbortSignal,
): Promise<IamPolicyDetail> {
  const response = await fetch(
    `/api/services/iam/policies/detail?policyArn=${encodeURIComponent(policyArn)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`IAM policy request failed with status ${response.status}`);
  }
  return (await response.json()) as IamPolicyDetail;
}

export interface IamPolicyCreateRequest {
  policyName: string;
  policyDocument: string;
  description: string | null;
  path: string | null;
}

export async function createIamPolicy(
  request: IamPolicyCreateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/iam/policies', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`IAM policy create request failed with status ${response.status}`);
  }
}

export async function createIamPolicyVersion(
  policyArn: string,
  policyDocument: string,
  setAsDefault: boolean,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/iam/policies/versions', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ policyArn, policyDocument, setAsDefault }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`IAM policy version create request failed with status ${response.status}`);
  }
}

export async function setIamPolicyDefaultVersion(
  policyArn: string,
  versionId: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/iam/policies/default-version', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ policyArn, versionId }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`IAM policy default-version request failed with status ${response.status}`);
  }
}

export async function deleteIamPolicyVersion(
  policyArn: string,
  versionId: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/policies/versions?policyArn=${encodeURIComponent(policyArn)}&versionId=${encodeURIComponent(versionId)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM policy version delete request failed with status ${response.status}`);
  }
}

export async function deleteIamPolicy(policyArn: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(
    `/api/services/iam/policies?policyArn=${encodeURIComponent(policyArn)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM policy delete request failed with status ${response.status}`);
  }
}

/**
 * Build a query string of repeated `key=` parameters for the IAM untag endpoints.
 */
function buildTagKeyQuery(keys: readonly string[]): string {
  return keys.map((key) => `key=${encodeURIComponent(key)}`).join('&');
}

export async function tagIamUser(
  userName: string,
  tags: readonly IamTag[],
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/tags`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ tags }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM user tag request failed with status ${response.status}`);
  }
}

export async function untagIamUser(
  userName: string,
  keys: readonly string[],
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/tags?${buildTagKeyQuery(keys)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM user untag request failed with status ${response.status}`);
  }
}

export async function putIamUserPermissionsBoundary(
  userName: string,
  permissionsBoundaryArn: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/permissions-boundary`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ permissionsBoundaryArn }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM user permissions-boundary request failed with status ${response.status}`);
  }
}

export async function deleteIamUserPermissionsBoundary(
  userName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/users/${encodeURIComponent(userName)}/permissions-boundary`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(
      `IAM user permissions-boundary delete request failed with status ${response.status}`,
    );
  }
}

export async function tagIamRole(
  roleName: string,
  tags: readonly IamTag[],
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}/tags`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ tags }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM role tag request failed with status ${response.status}`);
  }
}

export async function untagIamRole(
  roleName: string,
  keys: readonly string[],
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}/tags?${buildTagKeyQuery(keys)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM role untag request failed with status ${response.status}`);
  }
}

export async function putIamRolePermissionsBoundary(
  roleName: string,
  permissionsBoundaryArn: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}/permissions-boundary`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ permissionsBoundaryArn }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM role permissions-boundary request failed with status ${response.status}`);
  }
}

export async function deleteIamRolePermissionsBoundary(
  roleName: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/roles/${encodeURIComponent(roleName)}/permissions-boundary`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(
      `IAM role permissions-boundary delete request failed with status ${response.status}`,
    );
  }
}

export async function tagIamPolicy(
  policyArn: string,
  tags: readonly IamTag[],
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/policies/tags?policyArn=${encodeURIComponent(policyArn)}`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ tags }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM policy tag request failed with status ${response.status}`);
  }
}

export async function untagIamPolicy(
  policyArn: string,
  keys: readonly string[],
  signal?: AbortSignal,
): Promise<void> {
  const query = [`policyArn=${encodeURIComponent(policyArn)}`, buildTagKeyQuery(keys)].join('&');
  const response = await fetch(`/api/services/iam/policies/tags?${query}`, {
    method: 'DELETE',
    signal,
  });
  if (!response.ok) {
    throw new Error(`IAM policy untag request failed with status ${response.status}`);
  }
}

/**
 * Error raised when an IAM account-level operation is not supported by the current backend
 * (for example LocalStack Community edition returning HTTP 501).
 */
export class IamNotSupportedError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'IamNotSupportedError';
  }
}

export interface IamAccountSummary {
  entries: Record<string, number>;
}

export async function getIamAccountSummary(signal?: AbortSignal): Promise<IamAccountSummary> {
  const response = await fetch('/api/services/iam/account/summary', { signal });
  if (response.status === 501) {
    throw new IamNotSupportedError('IAM account summary is not supported by the current backend.');
  }
  if (!response.ok) {
    throw new Error(`IAM account summary request failed with status ${response.status}`);
  }
  return (await response.json()) as IamAccountSummary;
}

export interface IamPasswordPolicy {
  minimumPasswordLength: number;
  requireSymbols: boolean;
  requireNumbers: boolean;
  requireUppercaseCharacters: boolean;
  requireLowercaseCharacters: boolean;
  allowUsersToChangePassword: boolean;
  expirePasswords: boolean;
  maxPasswordAge: number | null;
  passwordReusePrevention: number | null;
  hardExpiry: boolean;
}

/**
 * Gets the account password policy. Returns `null` when no policy is set (HTTP 404).
 */
export async function getIamAccountPasswordPolicy(
  signal?: AbortSignal,
): Promise<IamPasswordPolicy | null> {
  const response = await fetch('/api/services/iam/account/password-policy', { signal });
  if (response.status === 404) {
    return null;
  }
  if (response.status === 501) {
    throw new IamNotSupportedError(
      'IAM account password policy is not supported by the current backend.',
    );
  }
  if (!response.ok) {
    throw new Error(`IAM account password policy request failed with status ${response.status}`);
  }
  return (await response.json()) as IamPasswordPolicy;
}

export interface IamPasswordPolicyUpdateRequest {
  minimumPasswordLength: number;
  requireSymbols: boolean;
  requireNumbers: boolean;
  requireUppercaseCharacters: boolean;
  requireLowercaseCharacters: boolean;
  allowUsersToChangePassword: boolean;
  maxPasswordAge: number | null;
  passwordReusePrevention: number | null;
  hardExpiry: boolean;
}

export async function updateIamAccountPasswordPolicy(
  request: IamPasswordPolicyUpdateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/iam/account/password-policy', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(
      `IAM account password policy update request failed with status ${response.status}`,
    );
  }
}

export async function deleteIamAccountPasswordPolicy(signal?: AbortSignal): Promise<void> {
  const response = await fetch('/api/services/iam/account/password-policy', {
    method: 'DELETE',
    signal,
  });
  if (!response.ok) {
    throw new Error(
      `IAM account password policy delete request failed with status ${response.status}`,
    );
  }
}

export interface IamAccountAliasList {
  aliases: string[];
}

export async function getIamAccountAliases(signal?: AbortSignal): Promise<IamAccountAliasList> {
  const response = await fetch('/api/services/iam/account/aliases', { signal });
  if (response.status === 501) {
    throw new IamNotSupportedError('IAM account aliases are not supported by the current backend.');
  }
  if (!response.ok) {
    throw new Error(`IAM account aliases request failed with status ${response.status}`);
  }
  return (await response.json()) as IamAccountAliasList;
}

export async function createIamAccountAlias(
  accountAlias: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/iam/account/aliases', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ accountAlias }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`IAM account alias create request failed with status ${response.status}`);
  }
}

export async function deleteIamAccountAlias(
  accountAlias: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/iam/account/aliases/${encodeURIComponent(accountAlias)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`IAM account alias delete request failed with status ${response.status}`);
  }
}

export interface StateMachineItem {
  name: string;
  stateMachineArn: string;
  type: string;
  creationDate: string;
}

export interface StateMachineListResult {
  stateMachines: StateMachineItem[];
}

export async function getStateMachines(signal?: AbortSignal): Promise<StateMachineListResult> {
  const response = await fetch('/api/services/step-functions/state-machines', { signal });
  if (!response.ok) {
    throw new Error(`Step Functions state machines request failed with status ${response.status}`);
  }
  return (await response.json()) as StateMachineListResult;
}

export interface StateMachineDetailResult {
  name: string;
  stateMachineArn: string;
  type: string;
  status: string;
  roleArn: string;
  definition: string;
  creationDate: string;
}

export async function getStateMachine(
  arn: string,
  signal?: AbortSignal,
): Promise<StateMachineDetailResult> {
  const response = await fetch(
    `/api/services/step-functions/state-machine?arn=${encodeURIComponent(arn)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Step Functions state machine request failed with status ${response.status}`);
  }
  return (await response.json()) as StateMachineDetailResult;
}

export interface ExecutionSummary {
  executionArn: string;
  name: string;
  stateMachineArn: string;
  status: string;
  startDate: string;
  stopDate: string | null;
}

export interface ExecutionListResult {
  executions: ExecutionSummary[];
}

export async function getExecutions(
  arn: string,
  signal?: AbortSignal,
): Promise<ExecutionListResult> {
  const response = await fetch(
    `/api/services/step-functions/executions?arn=${encodeURIComponent(arn)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Step Functions executions request failed with status ${response.status}`);
  }
  return (await response.json()) as ExecutionListResult;
}

export interface StartExecutionRequest {
  stateMachineArn: string;
  name: string | null;
  input: string | null;
}

export interface StartExecutionResult {
  executionArn: string;
  startDate: string;
}

export async function startExecution(
  request: StartExecutionRequest,
  signal?: AbortSignal,
): Promise<StartExecutionResult> {
  const response = await fetch('/api/services/step-functions/executions', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`Step Functions start execution request failed with status ${response.status}`);
  }
  return (await response.json()) as StartExecutionResult;
}

export interface ExecutionHistoryEvent {
  id: number;
  previousEventId: number | null;
  type: string;
  timestamp: string;
  name: string | null;
  input: string | null;
  output: string | null;
  error: string | null;
  cause: string | null;
}

export interface ExecutionHistoryResult {
  events: ExecutionHistoryEvent[];
}

export async function getExecutionHistory(
  executionArn: string,
  signal?: AbortSignal,
): Promise<ExecutionHistoryResult> {
  const response = await fetch(
    `/api/services/step-functions/execution-history?arn=${encodeURIComponent(executionArn)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Step Functions execution history request failed with status ${response.status}`);
  }
  return (await response.json()) as ExecutionHistoryResult;
}

export interface CloudFormationStackItem {
  stackName: string;
  stackId: string;
  stackStatus: string;
  description: string | null;
  creationTime: string;
  lastUpdatedTime: string | null;
}

export interface CloudFormationStackListResult {
  stacks: CloudFormationStackItem[];
}

export async function getStacks(signal?: AbortSignal): Promise<CloudFormationStackListResult> {
  const response = await fetch('/api/services/cloudformation/stacks', { signal });
  if (!response.ok) {
    throw new Error(`CloudFormation stacks request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationStackListResult;
}

export interface StackParameter {
  parameterKey: string;
  parameterValue: string;
}

export interface StackOutput {
  outputKey: string;
  outputValue: string;
  description: string | null;
  exportName: string | null;
}

export interface StackTag {
  key: string;
  value: string;
}

export interface CloudFormationStackDetailResult {
  stackName: string;
  stackId: string;
  stackStatus: string;
  stackStatusReason: string | null;
  description: string | null;
  creationTime: string;
  lastUpdatedTime: string | null;
  parameters: StackParameter[];
  outputs: StackOutput[];
  tags: StackTag[];
  capabilities: string[];
}

export async function getStack(
  name: string,
  signal?: AbortSignal,
): Promise<CloudFormationStackDetailResult> {
  const response = await fetch(
    `/api/services/cloudformation/stack?name=${encodeURIComponent(name)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation stack request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationStackDetailResult;
}

export interface CloudFormationStackTemplateResult {
  templateBody: string;
  format: string;
}

export async function getStackTemplate(
  name: string,
  signal?: AbortSignal,
): Promise<CloudFormationStackTemplateResult> {
  const response = await fetch(
    `/api/services/cloudformation/stack/template?name=${encodeURIComponent(name)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation stack template request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationStackTemplateResult;
}

export interface CloudFormationStackResource {
  logicalResourceId: string;
  physicalResourceId: string | null;
  resourceType: string;
  resourceStatus: string;
  resourceStatusReason: string | null;
  lastUpdatedTime: string;
}

export interface CloudFormationStackResourceListResult {
  resources: CloudFormationStackResource[];
}

export async function getStackResources(
  name: string,
  signal?: AbortSignal,
): Promise<CloudFormationStackResourceListResult> {
  const response = await fetch(
    `/api/services/cloudformation/stack/resources?name=${encodeURIComponent(name)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation stack resources request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationStackResourceListResult;
}

export interface CloudFormationStackEvent {
  eventId: string;
  timestamp: string;
  logicalResourceId: string;
  physicalResourceId: string | null;
  resourceType: string;
  resourceStatus: string;
  resourceStatusReason: string | null;
}

export interface CloudFormationStackEventListResult {
  events: CloudFormationStackEvent[];
}

export async function getStackEvents(
  name: string,
  signal?: AbortSignal,
): Promise<CloudFormationStackEventListResult> {
  const response = await fetch(
    `/api/services/cloudformation/stack/events?name=${encodeURIComponent(name)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation stack events request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationStackEventListResult;
}

export interface CloudFormationStackOperationResult {
  stackId: string;
}

export async function createStack(
  stackName: string,
  templateBody: string,
  parameters: StackParameter[],
  capabilities: string[],
  signal?: AbortSignal,
): Promise<CloudFormationStackOperationResult> {
  const response = await fetch('/api/services/cloudformation/stack', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ stackName, templateBody, parameters, capabilities }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`CloudFormation stack create request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationStackOperationResult;
}

export async function updateStack(
  name: string,
  templateBody: string,
  parameters: StackParameter[],
  capabilities: string[],
  signal?: AbortSignal,
): Promise<CloudFormationStackOperationResult> {
  const response = await fetch(
    `/api/services/cloudformation/stack?name=${encodeURIComponent(name)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ templateBody, parameters, capabilities }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation stack update request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationStackOperationResult;
}

export async function deleteStack(name: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(
    `/api/services/cloudformation/stack?name=${encodeURIComponent(name)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation stack delete request failed with status ${response.status}`);
  }
}

export interface CloudFormationChangeSetSummary {
  changeSetId: string;
  changeSetName: string;
  stackName: string;
  status: string;
  statusReason: string | null;
  executionStatus: string;
  description: string | null;
  creationTime: string;
}

export interface CloudFormationChangeSetListResult {
  changeSets: CloudFormationChangeSetSummary[];
}

export async function getChangeSets(
  name: string,
  signal?: AbortSignal,
): Promise<CloudFormationChangeSetListResult> {
  const response = await fetch(
    `/api/services/cloudformation/change-sets?name=${encodeURIComponent(name)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation change sets request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationChangeSetListResult;
}

export interface CloudFormationResourceChange {
  action: string;
  logicalResourceId: string;
  physicalResourceId: string | null;
  resourceType: string;
  replacement: string | null;
}

export interface CloudFormationChangeSetDetailResult {
  changeSetName: string;
  changeSetId: string;
  stackName: string;
  stackId: string;
  status: string;
  statusReason: string | null;
  executionStatus: string;
  description: string | null;
  creationTime: string;
  parameters: StackParameter[];
  capabilities: string[];
  changes: CloudFormationResourceChange[];
}

export async function getChangeSet(
  name: string,
  changeSet: string,
  signal?: AbortSignal,
): Promise<CloudFormationChangeSetDetailResult> {
  const response = await fetch(
    `/api/services/cloudformation/change-set?name=${encodeURIComponent(name)}&changeSet=${encodeURIComponent(changeSet)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation change set request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationChangeSetDetailResult;
}

export interface CloudFormationChangeSetOperationResult {
  changeSetId: string;
}

export async function createChangeSet(
  stackName: string,
  changeSetName: string,
  changeSetType: string,
  templateBody: string,
  parameters: StackParameter[],
  capabilities: string[],
  signal?: AbortSignal,
): Promise<CloudFormationChangeSetOperationResult> {
  const response = await fetch('/api/services/cloudformation/change-set', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      stackName,
      changeSetName,
      changeSetType,
      templateBody,
      parameters,
      capabilities,
    }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`CloudFormation change set create request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationChangeSetOperationResult;
}

export async function executeChangeSet(
  name: string,
  changeSet: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/cloudformation/change-set/execute?name=${encodeURIComponent(name)}&changeSet=${encodeURIComponent(changeSet)}`,
    {
      method: 'POST',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation change set execute request failed with status ${response.status}`);
  }
}

export async function deleteChangeSet(
  name: string,
  changeSet: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/cloudformation/change-set?name=${encodeURIComponent(name)}&changeSet=${encodeURIComponent(changeSet)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation change set delete request failed with status ${response.status}`);
  }
}

export interface CloudFormationDriftDetectionResult {
  stackDriftDetectionId: string;
}

export async function detectStackDrift(
  name: string,
  signal?: AbortSignal,
): Promise<CloudFormationDriftDetectionResult> {
  const response = await fetch(
    `/api/services/cloudformation/stack/drift?name=${encodeURIComponent(name)}`,
    {
      method: 'POST',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation drift detection request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationDriftDetectionResult;
}

export interface CloudFormationDriftStatusResult {
  stackDriftDetectionId: string;
  stackId: string;
  detectionStatus: string;
  detectionStatusReason: string | null;
  stackDriftStatus: string;
  driftedStackResourceCount: number;
  timestamp: string;
}

export async function getDriftStatus(
  driftDetectionId: string,
  signal?: AbortSignal,
): Promise<CloudFormationDriftStatusResult> {
  const response = await fetch(
    `/api/services/cloudformation/stack/drift?driftDetectionId=${encodeURIComponent(driftDetectionId)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation drift status request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationDriftStatusResult;
}

export interface CloudFormationResourceDrift {
  logicalResourceId: string;
  physicalResourceId: string | null;
  resourceType: string;
  driftStatus: string;
  expectedProperties: string | null;
  actualProperties: string | null;
  timestamp: string;
}

export interface CloudFormationResourceDriftListResult {
  drifts: CloudFormationResourceDrift[];
}

export async function getResourceDrifts(
  name: string,
  signal?: AbortSignal,
): Promise<CloudFormationResourceDriftListResult> {
  const response = await fetch(
    `/api/services/cloudformation/stack/drift/resources?name=${encodeURIComponent(name)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation resource drifts request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationResourceDriftListResult;
}

export interface CloudFormationExport {
  name: string;
  value: string;
  exportingStackId: string;
}

export interface CloudFormationExportListResult {
  exports: CloudFormationExport[];
}

export async function getExports(
  signal?: AbortSignal,
): Promise<CloudFormationExportListResult> {
  const response = await fetch('/api/services/cloudformation/exports', { signal });
  if (!response.ok) {
    throw new Error(`CloudFormation exports request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationExportListResult;
}

export interface CloudFormationImportListResult {
  importingStackNames: string[];
}

export async function getImports(
  exportName: string,
  signal?: AbortSignal,
): Promise<CloudFormationImportListResult> {
  const response = await fetch(
    `/api/services/cloudformation/exports/${encodeURIComponent(exportName)}/imports`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`CloudFormation imports request failed with status ${response.status}`);
  }
  return (await response.json()) as CloudFormationImportListResult;
}

export interface EventBridgeRuleItem {
  name: string;
  arn: string;
  eventBusName: string;
  state: string;
  description: string | null;
  scheduleExpression: string | null;
}

export interface EventBridgeRuleListResult {
  rules: EventBridgeRuleItem[];
}

export async function getEventBridgeRules(
  signal?: AbortSignal,
): Promise<EventBridgeRuleListResult> {
  const response = await fetch('/api/services/eventbridge/rules', { signal });
  if (!response.ok) {
    throw new Error(`EventBridge rules request failed with status ${response.status}`);
  }
  return (await response.json()) as EventBridgeRuleListResult;
}

export interface EventBridgeTargetItem {
  id: string;
  arn: string;
}

export interface EventBridgeTargetListResult {
  targets: EventBridgeTargetItem[];
}

export async function getEventBridgeTargets(
  ruleName: string,
  signal?: AbortSignal,
): Promise<EventBridgeTargetListResult> {
  const response = await fetch(
    `/api/services/eventbridge/targets?rule=${encodeURIComponent(ruleName)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`EventBridge targets request failed with status ${response.status}`);
  }
  return (await response.json()) as EventBridgeTargetListResult;
}

export interface PutEventBridgeEventRequest {
  source: string;
  detailType: string;
  detail: string;
  eventBusName: string | null;
}

export interface PutEventBridgeEventResult {
  accepted: boolean;
  eventId: string | null;
  errorCode: string | null;
  errorMessage: string | null;
}

export async function putEventBridgeEvent(
  request: PutEventBridgeEventRequest,
  signal?: AbortSignal,
): Promise<PutEventBridgeEventResult> {
  const response = await fetch('/api/services/eventbridge/events', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`EventBridge put event request failed with status ${response.status}`);
  }
  return (await response.json()) as PutEventBridgeEventResult;
}

export interface ScheduledRuleListResult {
  rules: EventBridgeRuleItem[];
}

export async function getScheduledRules(
  signal?: AbortSignal,
): Promise<ScheduledRuleListResult> {
  const response = await fetch('/api/services/eventbridge/scheduled-rules', { signal });
  if (!response.ok) {
    throw new Error(`EventBridge scheduled rules request failed with status ${response.status}`);
  }
  return (await response.json()) as ScheduledRuleListResult;
}

export interface ScheduledRuleDetail {
  name: string;
  arn: string;
  eventBusName: string;
  state: string;
  scheduleExpression: string | null;
  description: string | null;
  roleArn: string | null;
  managedBy: string | null;
}

export async function getScheduledRule(
  name: string,
  bus?: string,
  signal?: AbortSignal,
): Promise<ScheduledRuleDetail> {
  const query = bus ? `?bus=${encodeURIComponent(bus)}` : '';
  const response = await fetch(
    `/api/services/eventbridge/scheduled-rules/${encodeURIComponent(name)}${query}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`EventBridge scheduled rule request failed with status ${response.status}`);
  }
  return (await response.json()) as ScheduledRuleDetail;
}

export interface PutScheduledRuleRequest {
  name: string;
  scheduleExpression: string;
  state: string;
  description?: string | null;
  eventBusName?: string | null;
}

export async function putScheduledRule(
  request: PutScheduledRuleRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/eventbridge/scheduled-rules', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`EventBridge create scheduled rule request failed with status ${response.status}`);
  }
}

export interface UpdateScheduledRuleRequest {
  scheduleExpression: string;
  state: string;
  description?: string | null;
}

export async function updateScheduledRule(
  name: string,
  request: UpdateScheduledRuleRequest,
  bus?: string,
  signal?: AbortSignal,
): Promise<void> {
  const query = bus ? `?bus=${encodeURIComponent(bus)}` : '';
  const response = await fetch(
    `/api/services/eventbridge/scheduled-rules/${encodeURIComponent(name)}${query}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`EventBridge update scheduled rule request failed with status ${response.status}`);
  }
}

export async function deleteScheduledRule(
  name: string,
  bus?: string,
  signal?: AbortSignal,
): Promise<void> {
  const query = bus ? `?bus=${encodeURIComponent(bus)}` : '';
  const response = await fetch(
    `/api/services/eventbridge/scheduled-rules/${encodeURIComponent(name)}${query}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`EventBridge delete scheduled rule request failed with status ${response.status}`);
  }
}

export async function setScheduledRuleState(
  name: string,
  state: string,
  bus?: string,
  signal?: AbortSignal,
): Promise<void> {
  const query = bus ? `?bus=${encodeURIComponent(bus)}` : '';
  const response = await fetch(
    `/api/services/eventbridge/scheduled-rules/${encodeURIComponent(name)}/state${query}`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ state }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`EventBridge set scheduled rule state request failed with status ${response.status}`);
  }
}

export interface ScheduledRuleTargetInput {
  id: string;
  arn: string;
  roleArn?: string | null;
  input?: string | null;
}

export async function putScheduledRuleTargets(
  name: string,
  targets: ScheduledRuleTargetInput[],
  bus?: string,
  signal?: AbortSignal,
): Promise<void> {
  const query = bus ? `?bus=${encodeURIComponent(bus)}` : '';
  const response = await fetch(
    `/api/services/eventbridge/scheduled-rules/${encodeURIComponent(name)}/targets${query}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ targets }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`EventBridge put scheduled rule targets request failed with status ${response.status}`);
  }
}

export async function removeScheduledRuleTargets(
  name: string,
  ids: string[],
  bus?: string,
  signal?: AbortSignal,
): Promise<void> {
  const query = bus ? `?bus=${encodeURIComponent(bus)}` : '';
  const response = await fetch(
    `/api/services/eventbridge/scheduled-rules/${encodeURIComponent(name)}/targets${query}`,
    {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ids }),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`EventBridge remove scheduled rule targets request failed with status ${response.status}`);
  }
}

export interface AcmCertificateItem {
  arn: string;
  domainName: string;
  status: string;
  type: string | null;
}

export interface AcmCertificateListResult {
  certificates: AcmCertificateItem[];
}

export async function getAcmCertificates(
  signal?: AbortSignal,
): Promise<AcmCertificateListResult> {
  const response = await fetch('/api/services/acm/certificates', { signal });
  if (!response.ok) {
    throw new Error(`ACM certificates request failed with status ${response.status}`);
  }
  return (await response.json()) as AcmCertificateListResult;
}

export interface ApiGatewayRestApiItem {
  id: string;
  name: string;
  description: string | null;
  createdDate: string | null;
}

export interface ApiGatewayRestApiListResult {
  restApis: ApiGatewayRestApiItem[];
}

export async function getApiGatewayRestApis(
  signal?: AbortSignal,
): Promise<ApiGatewayRestApiListResult> {
  const response = await fetch('/api/services/apigateway/restapis', { signal });
  if (!response.ok) {
    throw new Error(`API Gateway REST APIs request failed with status ${response.status}`);
  }
  return (await response.json()) as ApiGatewayRestApiListResult;
}

export interface Route53HostedZoneItem {
  id: string;
  name: string;
  recordCount: number;
  privateZone: boolean;
}

export interface Route53HostedZoneListResult {
  hostedZones: Route53HostedZoneItem[];
}

export async function getRoute53HostedZones(
  signal?: AbortSignal,
): Promise<Route53HostedZoneListResult> {
  const response = await fetch('/api/services/route53/hostedzones', { signal });
  if (!response.ok) {
    throw new Error(`Route 53 hosted zones request failed with status ${response.status}`);
  }
  return (await response.json()) as Route53HostedZoneListResult;
}

export interface SesIdentityItem {
  identity: string;
  identityType: string;
  verificationStatus: string;
}

export interface SesIdentityListResult {
  identities: SesIdentityItem[];
}

export async function getSesIdentities(
  signal?: AbortSignal,
): Promise<SesIdentityListResult> {
  const response = await fetch('/api/services/ses/identities', { signal });
  if (!response.ok) {
    throw new Error(`SES identities request failed with status ${response.status}`);
  }
  return (await response.json()) as SesIdentityListResult;
}

export interface ScheduleSummaryItem {
  name: string;
  groupName: string;
  state: string;
  targetArn: string;
  arn: string;
}

export interface ScheduleListResult {
  schedules: ScheduleSummaryItem[];
}

export async function getSchedules(signal?: AbortSignal): Promise<ScheduleListResult> {
  const response = await fetch('/api/services/scheduler/schedules', { signal });
  if (!response.ok) {
    throw new Error(`EventBridge Scheduler schedules request failed with status ${response.status}`);
  }
  return (await response.json()) as ScheduleListResult;
}

export interface ScheduleDetailResult {
  name: string;
  groupName: string;
  state: string;
  scheduleExpression: string;
  scheduleExpressionTimezone: string | null;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  targetArn: string;
  roleArn: string;
  flexibleTimeWindowMode: string;
  maximumWindowInMinutes: number | null;
  arn: string;
  creationDate: string | null;
  lastModificationDate: string | null;
}

export async function getSchedule(
  name: string,
  group: string,
  signal?: AbortSignal,
): Promise<ScheduleDetailResult> {
  const response = await fetch(
    `/api/services/scheduler/schedule?name=${encodeURIComponent(name)}&group=${encodeURIComponent(group)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`EventBridge Scheduler schedule request failed with status ${response.status}`);
  }
  return (await response.json()) as ScheduleDetailResult;
}

export interface ScheduleCreateRequest {
  name: string;
  groupName: string;
  scheduleExpression: string;
  scheduleExpressionTimezone: string | null;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  targetArn: string;
  roleArn: string;
  flexibleTimeWindowMode: string;
  maximumWindowInMinutes: number | null;
  state: string;
}

export async function createSchedule(
  request: ScheduleCreateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/scheduler/schedules', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`EventBridge Scheduler schedule create request failed with status ${response.status}`);
  }
}

export interface ScheduleUpdateRequest {
  scheduleExpression: string;
  scheduleExpressionTimezone: string | null;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  targetArn: string;
  roleArn: string;
  flexibleTimeWindowMode: string;
  maximumWindowInMinutes: number | null;
  state: string;
}

export async function updateSchedule(
  name: string,
  group: string,
  request: ScheduleUpdateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/scheduler/schedules/${encodeURIComponent(name)}?group=${encodeURIComponent(group)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`EventBridge Scheduler schedule update request failed with status ${response.status}`);
  }
}

export async function deleteSchedule(
  name: string,
  group: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/scheduler/schedules/${encodeURIComponent(name)}?group=${encodeURIComponent(group)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`EventBridge Scheduler schedule delete request failed with status ${response.status}`);
  }
}

export interface ScheduleGroupItem {
  name: string;
  state: string;
  arn: string;
  creationDate: string | null;
  lastModificationDate: string | null;
}

export interface ScheduleGroupListResult {
  groups: ScheduleGroupItem[];
}

export async function getScheduleGroups(signal?: AbortSignal): Promise<ScheduleGroupListResult> {
  const response = await fetch('/api/services/scheduler/groups', { signal });
  if (!response.ok) {
    throw new Error(`EventBridge Scheduler schedule groups request failed with status ${response.status}`);
  }
  return (await response.json()) as ScheduleGroupListResult;
}

export interface ScheduleGroupCreateRequest {
  name: string;
}

export async function createScheduleGroup(
  request: ScheduleGroupCreateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/services/scheduler/groups', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`EventBridge Scheduler schedule group create request failed with status ${response.status}`);
  }
}

export async function deleteScheduleGroup(name: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(`/api/services/scheduler/groups/${encodeURIComponent(name)}`, {
    method: 'DELETE',
    signal,
  });
  if (!response.ok) {
    throw new Error(`EventBridge Scheduler schedule group delete request failed with status ${response.status}`);
  }
}

export interface UserPoolSummaryItem {
  id: string;
  name: string;
  creationDate: string | null;
}

export interface UserPoolListResult {
  userPools: UserPoolSummaryItem[];
}

export async function getUserPools(signal?: AbortSignal): Promise<UserPoolListResult> {
  const response = await fetch('/api/services/cognito/user-pools', { signal });
  if (!response.ok) {
    throw new Error(`Cognito user pools request failed with status ${response.status}`);
  }
  return (await response.json()) as UserPoolListResult;
}

export interface UserPoolDetailResult {
  id: string;
  name: string;
  arn: string | null;
  mfaConfiguration: string | null;
  estimatedNumberOfUsers: number | null;
  usernameAttributes: string[];
  autoVerifiedAttributes: string[];
  creationDate: string | null;
  lastModifiedDate: string | null;
}

export async function getUserPool(
  id: string,
  signal?: AbortSignal,
): Promise<UserPoolDetailResult> {
  const response = await fetch(
    `/api/services/cognito/user-pools/${encodeURIComponent(id)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Cognito user pool request failed with status ${response.status}`);
  }
  return (await response.json()) as UserPoolDetailResult;
}

export interface UserPoolCreateRequest {
  name: string;
  mfaConfiguration: string | null;
  usernameAttributes: string[];
  autoVerifiedAttributes: string[];
}

export interface UserPoolCreatedResult {
  id: string;
}

export async function createUserPool(
  request: UserPoolCreateRequest,
  signal?: AbortSignal,
): Promise<UserPoolCreatedResult> {
  const response = await fetch('/api/services/cognito/user-pools', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  });
  if (!response.ok) {
    throw new Error(`Cognito user pool create request failed with status ${response.status}`);
  }
  return (await response.json()) as UserPoolCreatedResult;
}

export async function deleteUserPool(id: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(`/api/services/cognito/user-pools/${encodeURIComponent(id)}`, {
    method: 'DELETE',
    signal,
  });
  if (!response.ok) {
    throw new Error(`Cognito user pool delete request failed with status ${response.status}`);
  }
}

export interface UserPoolClientSummaryItem {
  clientId: string;
  clientName: string;
  userPoolId: string;
}

export interface UserPoolClientListResult {
  clients: UserPoolClientSummaryItem[];
}

export async function getUserPoolClients(
  poolId: string,
  signal?: AbortSignal,
): Promise<UserPoolClientListResult> {
  const response = await fetch(
    `/api/services/cognito/user-pools/${encodeURIComponent(poolId)}/clients`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Cognito user pool clients request failed with status ${response.status}`);
  }
  return (await response.json()) as UserPoolClientListResult;
}

export interface UserPoolClientDetailResult {
  clientId: string;
  clientName: string;
  userPoolId: string;
  clientSecret: string | null;
  generateSecret: boolean;
  explicitAuthFlows: string[];
  allowedOAuthFlows: string[];
  allowedOAuthScopes: string[];
  callbackURLs: string[];
  allowedOAuthFlowsUserPoolClient: boolean;
  creationDate: string | null;
  lastModifiedDate: string | null;
}

export async function getUserPoolClient(
  poolId: string,
  clientId: string,
  signal?: AbortSignal,
): Promise<UserPoolClientDetailResult> {
  const response = await fetch(
    `/api/services/cognito/user-pools/${encodeURIComponent(poolId)}/clients/${encodeURIComponent(clientId)}`,
    { signal },
  );
  if (!response.ok) {
    throw new Error(`Cognito user pool client request failed with status ${response.status}`);
  }
  return (await response.json()) as UserPoolClientDetailResult;
}

export interface UserPoolClientCreateRequest {
  clientName: string;
  generateSecret: boolean;
  explicitAuthFlows: string[];
  allowedOAuthFlows: string[];
  allowedOAuthScopes: string[];
  callbackURLs: string[];
  allowedOAuthFlowsUserPoolClient: boolean;
}

export async function createUserPoolClient(
  poolId: string,
  request: UserPoolClientCreateRequest,
  signal?: AbortSignal,
): Promise<UserPoolClientDetailResult> {
  const response = await fetch(
    `/api/services/cognito/user-pools/${encodeURIComponent(poolId)}/clients`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Cognito user pool client create request failed with status ${response.status}`);
  }
  return (await response.json()) as UserPoolClientDetailResult;
}

export interface UserPoolClientUpdateRequest {
  clientName: string;
  explicitAuthFlows: string[];
  allowedOAuthFlows: string[];
  allowedOAuthScopes: string[];
  callbackURLs: string[];
  allowedOAuthFlowsUserPoolClient: boolean;
}

export async function updateUserPoolClient(
  poolId: string,
  clientId: string,
  request: UserPoolClientUpdateRequest,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/cognito/user-pools/${encodeURIComponent(poolId)}/clients/${encodeURIComponent(clientId)}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Cognito user pool client update request failed with status ${response.status}`);
  }
}

export async function deleteUserPoolClient(
  poolId: string,
  clientId: string,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(
    `/api/services/cognito/user-pools/${encodeURIComponent(poolId)}/clients/${encodeURIComponent(clientId)}`,
    {
      method: 'DELETE',
      signal,
    },
  );
  if (!response.ok) {
    throw new Error(`Cognito user pool client delete request failed with status ${response.status}`);
  }
}