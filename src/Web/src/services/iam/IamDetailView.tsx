import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import type { ServiceDetailViewProps } from '../serviceViewRegistry';
import { IamUserDetailView } from './IamUserDetailView';
import { IamGroupDetailView } from './IamGroupDetailView';
import { IamRoleDetailView } from './IamRoleDetailView';
import { IamPolicyDetailView } from './IamPolicyDetailView';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
  padding: 16,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const messageStyle: CSSProperties = { fontSize: 14, opacity: 0.7 };

type ResourceKind = 'user' | 'group' | 'role' | 'policy';

interface ParsedResource {
  kind: ResourceKind | null;
  name: string;
}

/**
 * Split a type-prefixed IAM resource id (for example "role/MyRole") into its kind and name.
 */
function parseResource(resourceId: string): ParsedResource {
  const separator = resourceId.indexOf('/');
  if (separator <= 0) {
    return { kind: null, name: resourceId };
  }

  const prefix = resourceId.slice(0, separator);
  const name = resourceId.slice(separator + 1);
  if (prefix === 'user' || prefix === 'group' || prefix === 'role' || prefix === 'policy') {
    return { kind: prefix, name };
  }

  return { kind: null, name: resourceId };
}

export function IamDetailView({ serviceKey, resourceId }: ServiceDetailViewProps) {
  const { kind, name } = parseResource(resourceId);

  if (kind === 'user') {
    return <IamUserDetailView userName={name} />;
  }

  if (kind === 'group') {
    return <IamGroupDetailView groupName={name} serviceKey={serviceKey} />;
  }

  if (kind === 'role') {
    return <IamRoleDetailView roleName={name} />;
  }

  if (kind === 'policy') {
    return <IamPolicyDetailView policyArn={name} />;
  }

  return (
    <div data-testid="iam-detail-view" style={containerStyle}>
      <Heading as="h3" data-testid="iam-detail-name" style={{ fontSize: 16 }}>
        {name}
      </Heading>
      <p data-testid="iam-detail-unknown" style={messageStyle}>
        Unrecognised IAM resource.
      </p>
      <p data-testid="iam-detail-placeholder" style={messageStyle}>
        Details for this IAM resource are not available yet.
      </p>
    </div>
  );
}

export default IamDetailView;
