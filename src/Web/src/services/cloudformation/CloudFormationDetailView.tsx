import { useCallback, useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import {
  getStack,
  getStackEvents,
  getStackResources,
  getStackTemplate,
  updateStack,
} from '../../api/client';
import type {
  CloudFormationStackDetailResult,
  CloudFormationStackEventListResult,
  CloudFormationStackResourceListResult,
  CloudFormationStackTemplateResult,
  StackParameter,
} from '../../api/client';
import { ResourceLink } from '../../components/ResourceLink';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { CloudFormationStackForm } from './CloudFormationStackForm';
import type { StackFormValue } from './CloudFormationStackForm';
import { CloudFormationChangeSetPanel } from './CloudFormationChangeSetPanel';
import { CloudFormationDriftPanel } from './CloudFormationDriftPanel';
import { CloudFormationExportsPanel } from './CloudFormationExportsPanel';

/**
 * Maps CloudFormation resource types to the service key of the view that manages them, so a
 * managed resource can deep-link to its own service detail view.
 */
const resourceTypeServiceKeys: Record<string, string> = {
  'AWS::SQS::Queue': 'sqs',
  'AWS::SNS::Topic': 'sns',
  'AWS::DynamoDB::Table': 'dynamodb',
  'AWS::S3::Bucket': 's3',
  'AWS::Lambda::Function': 'lambda',
  'AWS::SecretsManager::Secret': 'secrets-manager',
  'AWS::SSM::Parameter': 'ssm-parameter-store',
  'AWS::StepFunctions::StateMachine': 'step-functions',
  'AWS::Logs::LogGroup': 'cloudwatch-logs',
  'AWS::IAM::Role': 'iam',
};

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const rowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const valueStyle: CSSProperties = { fontSize: 14, fontFamily: 'monospace' };
const messageStyle: CSSProperties = { fontSize: 14 };
const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};
const sectionHeadingStyle: CSSProperties = { fontSize: 14 };
const tableStyle: CSSProperties = {
  borderCollapse: 'collapse',
  fontSize: 13,
  width: '100%',
};
const cellStyle: CSSProperties = {
  textAlign: 'left',
  padding: '4px 8px',
  border: '1px solid #30363d',
  fontFamily: 'monospace',
};
const headerCellStyle: CSSProperties = {
  ...cellStyle,
  fontFamily: 'inherit',
  opacity: 0.7,
};
const templateStyle: CSSProperties = {
  margin: 0,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  fontFamily: 'monospace',
  fontSize: 13,
  maxHeight: 360,
  overflow: 'auto',
  whiteSpace: 'pre',
};
const copyButtonStyle: CSSProperties = {
  alignSelf: 'flex-start',
  fontSize: 12,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
};

const refreshButtonStyle: CSSProperties = {
  ...copyButtonStyle,
};

type UpdateState = 'idle' | 'saving' | 'updated' | 'error';

function toParameters(value: StackFormValue): StackParameter[] {
  return value.parameters
    .filter((parameter) => parameter.parameterKey.trim() !== '')
    .map((parameter) => ({
      parameterKey: parameter.parameterKey,
      parameterValue: parameter.parameterValue,
    }));
}

/**
 * Maps a CloudFormation resource status to a colour, so the events timeline can convey progress
 * and failures at a glance.
 */
function statusColor(status: string): string {
  if (status.endsWith('FAILED') || status.endsWith('ROLLBACK_IN_PROGRESS')) {
    return '#f85149';
  }
  if (status.endsWith('COMPLETE')) {
    return '#3fb950';
  }
  return '#d29922';
}

function formatTemplate(template: CloudFormationStackTemplateResult): string {
  if (template.format !== 'json') {
    return template.templateBody;
  }
  try {
    return JSON.stringify(JSON.parse(template.templateBody), null, 2);
  } catch {
    return template.templateBody;
  }
}

type TemplateState =
  | { kind: 'loading' }
  | { kind: 'ready'; template: CloudFormationStackTemplateResult }
  | { kind: 'error' };

type ResourcesState =
  | { kind: 'loading' }
  | { kind: 'ready'; resources: CloudFormationStackResourceListResult }
  | { kind: 'error' };

type EventsState =
  | { kind: 'loading' }
  | { kind: 'ready'; events: CloudFormationStackEventListResult }
  | { kind: 'error' };

type LoadState =
  | { kind: 'loading' }
  | { kind: 'ready'; stack: CloudFormationStackDetailResult }
  | { kind: 'error' };

export function CloudFormationDetailView({ resourceId }: ServiceDetailViewProps) {
  const [state, setState] = useState<LoadState>({ kind: 'loading' });
  const [templateState, setTemplateState] = useState<TemplateState>({ kind: 'loading' });
  const [resourcesState, setResourcesState] = useState<ResourcesState>({ kind: 'loading' });
  const [eventsState, setEventsState] = useState<EventsState>({ kind: 'loading' });
  const [eventsRefreshKey, setEventsRefreshKey] = useState(0);
  const [copied, setCopied] = useState(false);
  const [showUpdate, setShowUpdate] = useState(false);
  const [updateState, setUpdateState] = useState<UpdateState>('idle');
  const [stackRefreshKey, setStackRefreshKey] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    getStack(resourceId, controller.signal)
      .then((stack) => setState({ kind: 'ready', stack }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, stackRefreshKey]);

  useEffect(() => {
    const controller = new AbortController();
    setResourcesState({ kind: 'loading' });
    getStackResources(resourceId, controller.signal)
      .then((resources) => setResourcesState({ kind: 'ready', resources }))
      .catch(() => setResourcesState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId]);

  useEffect(() => {
    const controller = new AbortController();
    setEventsState({ kind: 'loading' });
    getStackEvents(resourceId, controller.signal)
      .then((events) => setEventsState({ kind: 'ready', events }))
      .catch(() => setEventsState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId, eventsRefreshKey]);

  useEffect(() => {
    const controller = new AbortController();
    setCopied(false);
    setTemplateState({ kind: 'loading' });
    getStackTemplate(resourceId, controller.signal)
      .then((template) => setTemplateState({ kind: 'ready', template }))
      .catch(() => setTemplateState({ kind: 'error' }));
    return () => controller.abort();
  }, [resourceId]);

  const copyTemplate = useCallback((body: string) => {
    void navigator.clipboard.writeText(body).then(
      () => setCopied(true),
      () => setCopied(false),
    );
  }, []);

  const handleUpdate = useCallback(
    (value: StackFormValue) => {
      setUpdateState('saving');
      updateStack(resourceId, value.templateBody, toParameters(value), value.capabilities)
        .then(() => {
          setUpdateState('updated');
          setShowUpdate(false);
          setStackRefreshKey((key) => key + 1);
          setEventsRefreshKey((key) => key + 1);
        })
        .catch(() => setUpdateState('error'));
    },
    [resourceId],
  );

  if (state.kind === 'loading') {
    return (
      <p data-testid="cloudformation-detail-loading" style={messageStyle}>
        Loading stack&hellip;
      </p>
    );
  }

  if (state.kind === 'error') {
    return (
      <p data-testid="cloudformation-detail-error" style={messageStyle}>
        Unable to load the stack.
      </p>
    );
  }

  const stack = state.stack;

  return (
    <div data-testid="cloudformation-detail-view" style={containerStyle}>
      <Heading as="h2" data-testid="cloudformation-detail-name" style={{ fontSize: 18 }}>
        {stack.stackName}
      </Heading>
      <div style={rowStyle}>
        <span style={labelStyle}>Stack ID</span>
        <span data-testid="cloudformation-detail-id" style={valueStyle}>
          {stack.stackId}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Status</span>
        <span data-testid="cloudformation-detail-status" style={valueStyle}>
          {stack.stackStatus}
        </span>
      </div>
      {stack.stackStatusReason !== null ? (
        <div style={rowStyle}>
          <span style={labelStyle}>Status reason</span>
          <span data-testid="cloudformation-detail-status-reason" style={valueStyle}>
            {stack.stackStatusReason}
          </span>
        </div>
      ) : null}
      {stack.description !== null ? (
        <div style={rowStyle}>
          <span style={labelStyle}>Description</span>
          <span data-testid="cloudformation-detail-description" style={valueStyle}>
            {stack.description}
          </span>
        </div>
      ) : null}
      <div style={rowStyle}>
        <span style={labelStyle}>Created</span>
        <span data-testid="cloudformation-detail-created" style={valueStyle}>
          {stack.creationTime}
        </span>
      </div>
      <div style={rowStyle}>
        <span style={labelStyle}>Last updated</span>
        <span data-testid="cloudformation-detail-updated" style={valueStyle}>
          {stack.lastUpdatedTime ?? '\u2014'}
        </span>
      </div>
      <div style={sectionStyle}>
        <Heading
          as="h3"
          data-testid="cloudformation-detail-parameters-heading"
          style={sectionHeadingStyle}
        >
          Parameters
        </Heading>
        {stack.parameters.length === 0 ? (
          <p data-testid="cloudformation-detail-parameters-empty" style={messageStyle}>
            No parameters.
          </p>
        ) : (
          <table data-testid="cloudformation-detail-parameters" style={tableStyle}>
            <thead>
              <tr>
                <th style={headerCellStyle}>Key</th>
                <th style={headerCellStyle}>Value</th>
              </tr>
            </thead>
            <tbody>
              {stack.parameters.map((parameter) => (
                <tr key={parameter.parameterKey}>
                  <td style={cellStyle}>{parameter.parameterKey}</td>
                  <td style={cellStyle}>{parameter.parameterValue}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
      <div style={sectionStyle}>
        <Heading
          as="h3"
          data-testid="cloudformation-detail-outputs-heading"
          style={sectionHeadingStyle}
        >
          Outputs
        </Heading>
        {stack.outputs.length === 0 ? (
          <p data-testid="cloudformation-detail-outputs-empty" style={messageStyle}>
            No outputs.
          </p>
        ) : (
          <table data-testid="cloudformation-detail-outputs" style={tableStyle}>
            <thead>
              <tr>
                <th style={headerCellStyle}>Key</th>
                <th style={headerCellStyle}>Value</th>
                <th style={headerCellStyle}>Description</th>
                <th style={headerCellStyle}>Export name</th>
              </tr>
            </thead>
            <tbody>
              {stack.outputs.map((output) => (
                <tr key={output.outputKey}>
                  <td style={cellStyle}>{output.outputKey}</td>
                  <td style={cellStyle}>{output.outputValue}</td>
                  <td style={cellStyle}>{output.description ?? '\u2014'}</td>
                  <td style={cellStyle}>{output.exportName ?? '\u2014'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
      <div style={sectionStyle}>
        <Heading
          as="h3"
          data-testid="cloudformation-detail-tags-heading"
          style={sectionHeadingStyle}
        >
          Tags
        </Heading>
        {stack.tags.length === 0 ? (
          <p data-testid="cloudformation-detail-tags-empty" style={messageStyle}>
            No tags.
          </p>
        ) : (
          <table data-testid="cloudformation-detail-tags" style={tableStyle}>
            <thead>
              <tr>
                <th style={headerCellStyle}>Key</th>
                <th style={headerCellStyle}>Value</th>
              </tr>
            </thead>
            <tbody>
              {stack.tags.map((tag) => (
                <tr key={tag.key}>
                  <td style={cellStyle}>{tag.key}</td>
                  <td style={cellStyle}>{tag.value}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
      <div style={sectionStyle}>
        <Heading
          as="h3"
          data-testid="cloudformation-detail-capabilities-heading"
          style={sectionHeadingStyle}
        >
          Capabilities
        </Heading>
        {stack.capabilities.length === 0 ? (
          <p data-testid="cloudformation-detail-capabilities-empty" style={messageStyle}>
            No capabilities.
          </p>
        ) : (
          <ul data-testid="cloudformation-detail-capabilities" style={messageStyle}>
            {stack.capabilities.map((capability) => (
              <li key={capability} style={valueStyle}>
                {capability}
              </li>
            ))}
          </ul>
        )}
      </div>
      <div style={sectionStyle}>
        <Heading
          as="h3"
          data-testid="cloudformation-detail-resources-heading"
          style={sectionHeadingStyle}
        >
          Resources
        </Heading>
        {resourcesState.kind === 'loading' ? (
          <p data-testid="cloudformation-detail-resources-loading" style={messageStyle}>
            Loading resources&hellip;
          </p>
        ) : resourcesState.kind === 'error' ? (
          <p data-testid="cloudformation-detail-resources-error" style={messageStyle}>
            Unable to load the resources.
          </p>
        ) : resourcesState.resources.resources.length === 0 ? (
          <p data-testid="cloudformation-detail-resources-empty" style={messageStyle}>
            No resources.
          </p>
        ) : (
          <table data-testid="cloudformation-detail-resources" style={tableStyle}>
            <thead>
              <tr>
                <th style={headerCellStyle}>Logical ID</th>
                <th style={headerCellStyle}>Physical ID</th>
                <th style={headerCellStyle}>Type</th>
                <th style={headerCellStyle}>Status</th>
                <th style={headerCellStyle}>Last updated</th>
              </tr>
            </thead>
            <tbody>
              {resourcesState.resources.resources.map((resource) => {
                const serviceKey = resourceTypeServiceKeys[resource.resourceType];
                return (
                  <tr key={resource.logicalResourceId}>
                    <td style={cellStyle}>{resource.logicalResourceId}</td>
                    <td style={cellStyle}>
                      {resource.physicalResourceId === null ? (
                        '\u2014'
                      ) : serviceKey !== undefined ? (
                        <ResourceLink
                          reference={resource.physicalResourceId}
                          service={serviceKey}
                        />
                      ) : (
                        resource.physicalResourceId
                      )}
                    </td>
                    <td style={cellStyle}>{resource.resourceType}</td>
                    <td style={cellStyle}>{resource.resourceStatus}</td>
                    <td style={cellStyle}>{resource.lastUpdatedTime}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
      <div style={sectionStyle}>
        <Heading
          as="h3"
          data-testid="cloudformation-detail-events-heading"
          style={sectionHeadingStyle}
        >
          Events
        </Heading>
        <button
          type="button"
          data-testid="cloudformation-detail-events-refresh"
          style={refreshButtonStyle}
          onClick={() => setEventsRefreshKey((key) => key + 1)}
        >
          Refresh
        </button>
        {eventsState.kind === 'loading' ? (
          <p data-testid="cloudformation-detail-events-loading" style={messageStyle}>
            Loading events&hellip;
          </p>
        ) : eventsState.kind === 'error' ? (
          <p data-testid="cloudformation-detail-events-error" style={messageStyle}>
            Unable to load the events.
          </p>
        ) : eventsState.events.events.length === 0 ? (
          <p data-testid="cloudformation-detail-events-empty" style={messageStyle}>
            No events.
          </p>
        ) : (
          <table data-testid="cloudformation-detail-events" style={tableStyle}>
            <thead>
              <tr>
                <th style={headerCellStyle}>Timestamp</th>
                <th style={headerCellStyle}>Logical ID</th>
                <th style={headerCellStyle}>Type</th>
                <th style={headerCellStyle}>Status</th>
                <th style={headerCellStyle}>Reason</th>
              </tr>
            </thead>
            <tbody>
              {eventsState.events.events.map((event) => (
                <tr key={event.eventId}>
                  <td style={cellStyle}>{event.timestamp}</td>
                  <td style={cellStyle}>{event.logicalResourceId}</td>
                  <td style={cellStyle}>{event.resourceType}</td>
                  <td style={{ ...cellStyle, color: statusColor(event.resourceStatus) }}>
                    {event.resourceStatus}
                  </td>
                  <td style={cellStyle}>{event.resourceStatusReason ?? '\u2014'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
      <div style={sectionStyle}>
        <Heading
          as="h3"
          data-testid="cloudformation-detail-template-heading"
          style={sectionHeadingStyle}
        >
          Template
        </Heading>
        {templateState.kind === 'loading' ? (
          <p data-testid="cloudformation-detail-template-loading" style={messageStyle}>
            Loading template&hellip;
          </p>
        ) : templateState.kind === 'error' ? (
          <p data-testid="cloudformation-detail-template-error" style={messageStyle}>
            Unable to load the template.
          </p>
        ) : (
          <>
            <button
              type="button"
              data-testid="cloudformation-detail-template-copy"
              style={copyButtonStyle}
              onClick={() => copyTemplate(templateState.template.templateBody)}
            >
              {copied ? 'Copied' : 'Copy'}
            </button>
            <pre data-testid="cloudformation-detail-template" style={templateStyle}>
              {formatTemplate(templateState.template)}
            </pre>
          </>
        )}
      </div>
      <div style={sectionStyle}>
        <Heading
          as="h3"
          data-testid="cloudformation-detail-update-heading"
          style={sectionHeadingStyle}
        >
          Update stack
        </Heading>
        <button
          type="button"
          data-testid="cloudformation-detail-update-toggle"
          style={refreshButtonStyle}
          onClick={() => setShowUpdate((current) => !current)}
        >
          {showUpdate ? 'Cancel' : 'Update stack'}
        </button>
        {showUpdate && templateState.kind === 'ready' ? (
          <CloudFormationStackForm
            testIdPrefix="cloudformation-update"
            submitLabel="Update"
            saving={updateState === 'saving'}
            initialTemplateBody={templateState.template.templateBody}
            initialParameters={stack.parameters}
            initialCapabilities={stack.capabilities}
            onSubmit={handleUpdate}
          />
        ) : null}
        {updateState === 'updated' ? (
          <p data-testid="cloudformation-detail-update-status" style={messageStyle}>
            Stack update requested.
          </p>
        ) : null}
        {updateState === 'error' ? (
          <p data-testid="cloudformation-detail-update-error" style={messageStyle}>
            Unable to update the stack.
          </p>
        ) : null}
      </div>
      {templateState.kind === 'ready' ? (
        <CloudFormationChangeSetPanel
          stackName={stack.stackName}
          initialTemplateBody={templateState.template.templateBody}
          initialParameters={stack.parameters}
          initialCapabilities={stack.capabilities}
        />
      ) : null}
      <CloudFormationDriftPanel stackName={stack.stackName} />
      <CloudFormationExportsPanel />
    </div>
  );
}

export default CloudFormationDetailView;
