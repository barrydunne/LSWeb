import { useState } from 'react';
import type { CSSProperties } from 'react';
import { Text } from '@primer/react';
import { ConfirmationHost } from '../../../components/ConfirmationHost';
import { ResourceLink } from '../../../components/ResourceLink';

const containerStyle: CSSProperties = { display: 'flex', flexDirection: 'column', gap: 8 };
const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };
const messageStyle: CSSProperties = { fontSize: 14 };

const currentRowStyle: CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  gap: 12,
  padding: 8,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
  padding: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
};

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
  color: 'inherit',
};

const buttonStyle: CSSProperties = {
  fontSize: 12,
  padding: '2px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

export interface PermissionsBoundaryControlProps {
  boundaryArn: string | null;
  onSet: (arn: string) => void;
  onRemove: () => void;
  testId: string;
}

/**
 * Reusable permissions-boundary control for the IAM user and role detail views. Shows the current
 * boundary (linked to the managed-policy detail) with a remove confirmation, and a form for setting
 * a boundary by managed-policy ARN.
 */
export function PermissionsBoundaryControl({
  boundaryArn,
  onSet,
  onRemove,
  testId,
}: PermissionsBoundaryControlProps) {
  const [arnInput, setArnInput] = useState('');
  const [arnError, setArnError] = useState(false);

  const handleSet = () => {
    const trimmed = arnInput.trim();
    if (trimmed === '') {
      setArnError(true);
      return;
    }
    setArnError(false);
    onSet(trimmed);
    setArnInput('');
  };

  return (
    <div data-testid={`${testId}`} style={containerStyle}>
      <Text style={labelStyle}>Permissions boundary</Text>
      {boundaryArn === null ? (
        <Text data-testid={`${testId}-empty`} style={messageStyle}>
          No permissions boundary set.
        </Text>
      ) : (
        <div data-testid={`${testId}-current`} style={currentRowStyle}>
          <ResourceLink reference={boundaryArn} service="iam" />
          <ConfirmationHost
            actionLabel="Remove"
            prompt="Remove the permissions boundary?"
            confirmLabel="Confirm"
            onConfirm={onRemove}
          />
        </div>
      )}

      <div data-testid={`${testId}-form`} style={formStyle}>
        <label style={labelStyle} htmlFor={`${testId}-arn`}>
          Managed policy ARN
        </label>
        <input
          id={`${testId}-arn`}
          type="text"
          data-testid={`${testId}-arn`}
          style={inputStyle}
          value={arnInput}
          onChange={(event) => setArnInput(event.target.value)}
        />
        {arnError ? (
          <Text data-testid={`${testId}-arn-error`} style={messageStyle}>
            Enter a managed policy ARN.
          </Text>
        ) : null}
        <button type="button" data-testid={`${testId}-submit`} style={buttonStyle} onClick={handleSet}>
          Set boundary
        </button>
      </div>
    </div>
  );
}

export default PermissionsBoundaryControl;
