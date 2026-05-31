import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import type { ServiceListViewProps } from '../serviceViewRegistry';
import { IamUsersPanel } from './IamUsersPanel';
import { IamGroupsPanel } from './IamGroupsPanel';
import { IamRolesPanel } from './IamRolesPanel';
import { IamPoliciesPanel } from './IamPoliciesPanel';
import { IamAccountPanel } from './IamAccountPanel';

const containerStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 12,
};

const tabBarStyle: CSSProperties = {
  display: 'flex',
  gap: 8,
  borderBottom: '1px solid #30363d',
  paddingBottom: 8,
};

const tabButtonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid transparent',
  background: 'transparent',
  color: 'inherit',
  cursor: 'pointer',
};

const activeTabButtonStyle: CSSProperties = {
  ...tabButtonStyle,
  border: '1px solid #30363d',
  background: '#21262d',
};

type TabKey = 'users' | 'groups' | 'roles' | 'policies' | 'account';

interface TabDescriptor {
  key: TabKey;
  label: string;
}

const tabs: TabDescriptor[] = [
  { key: 'users', label: 'Users' },
  { key: 'groups', label: 'Groups' },
  { key: 'roles', label: 'Roles' },
  { key: 'policies', label: 'Policies' },
  { key: 'account', label: 'Account' },
];

export function IamListView({ serviceKey }: ServiceListViewProps) {
  const [tab, setTab] = useState<TabKey>('users');

  return (
    <div data-testid={`${serviceKey}-list-view`} style={containerStyle}>
      <Heading as="h3" data-testid="iam-list-heading" style={{ fontSize: 16 }}>
        Identity &amp; Access Management
      </Heading>
      <div role="tablist" style={tabBarStyle}>
        {tabs.map((descriptor) => (
          <button
            key={descriptor.key}
            type="button"
            role="tab"
            aria-selected={tab === descriptor.key}
            data-testid={`iam-list-tab-${descriptor.key}`}
            style={tab === descriptor.key ? activeTabButtonStyle : tabButtonStyle}
            onClick={() => setTab(descriptor.key)}
          >
            {descriptor.label}
          </button>
        ))}
      </div>
      {tabs.map((descriptor) =>
        tab === descriptor.key ? (
          descriptor.key === 'users' ? (
            <div key={descriptor.key} data-testid="iam-list-panel-users">
              <IamUsersPanel serviceKey={serviceKey} />
            </div>
          ) : descriptor.key === 'groups' ? (
            <div key={descriptor.key} data-testid="iam-list-panel-groups">
              <IamGroupsPanel serviceKey={serviceKey} />
            </div>
          ) : descriptor.key === 'roles' ? (
            <div key={descriptor.key} data-testid="iam-list-panel-roles">
              <IamRolesPanel serviceKey={serviceKey} />
            </div>
          ) : descriptor.key === 'policies' ? (
            <div key={descriptor.key} data-testid="iam-list-panel-policies">
              <IamPoliciesPanel serviceKey={serviceKey} />
            </div>
          ) : (
            <div key={descriptor.key} data-testid="iam-list-panel-account">
              <IamAccountPanel serviceKey={serviceKey} />
            </div>
          )
        ) : null,
      )}
    </div>
  );
}

export default IamListView;
