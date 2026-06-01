import { useState } from 'react';
import type { CSSProperties } from 'react';
import type { StackParameter } from '../../api/client';

const AVAILABLE_CAPABILITIES = [
  'CAPABILITY_IAM',
  'CAPABILITY_NAMED_IAM',
  'CAPABILITY_AUTO_EXPAND',
] as const;

const formStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 10,
  padding: 12,
  marginBottom: 12,
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#161b22',
};

const fieldRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
};

const labelStyle: CSSProperties = { fontSize: 12, opacity: 0.7 };

const inputStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 8px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#0d1117',
  color: 'inherit',
};

const textareaStyle: CSSProperties = {
  ...inputStyle,
  minHeight: 160,
  fontFamily: 'monospace',
  resize: 'vertical',
};

const parameterRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'row',
  gap: 6,
  alignItems: 'center',
};

const checkboxRowStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'row',
  alignItems: 'center',
  gap: 6,
};

const buttonStyle: CSSProperties = {
  fontSize: 13,
  padding: '4px 10px',
  borderRadius: 6,
  border: '1px solid #30363d',
  background: '#21262d',
  color: 'inherit',
  cursor: 'pointer',
  alignSelf: 'flex-start',
};

const smallButtonStyle: CSSProperties = {
  ...buttonStyle,
  padding: '2px 8px',
  fontSize: 12,
};

export interface StackFormValue {
  stackName: string;
  templateBody: string;
  parameters: StackParameter[];
  capabilities: string[];
}

interface CloudFormationStackFormProps {
  testIdPrefix: string;
  submitLabel: string;
  saving: boolean;
  requireName?: boolean;
  initialTemplateBody?: string;
  initialParameters?: StackParameter[];
  initialCapabilities?: string[];
  onSubmit: (value: StackFormValue) => void;
}

export function CloudFormationStackForm({
  testIdPrefix,
  submitLabel,
  saving,
  requireName = false,
  initialTemplateBody = '',
  initialParameters = [],
  initialCapabilities = [],
  onSubmit,
}: CloudFormationStackFormProps) {
  const [stackName, setStackName] = useState('');
  const [templateBody, setTemplateBody] = useState(initialTemplateBody);
  const [parameters, setParameters] = useState<StackParameter[]>(initialParameters);
  const [capabilities, setCapabilities] = useState<string[]>(initialCapabilities);

  const addParameter = () => {
    setParameters((current) => [...current, { parameterKey: '', parameterValue: '' }]);
  };

  const updateParameter = (index: number, field: keyof StackParameter, value: string) => {
    setParameters((current) =>
      current.map((parameter, current_index) =>
        current_index === index ? { ...parameter, [field]: value } : parameter,
      ),
    );
  };

  const removeParameter = (index: number) => {
    setParameters((current) => current.filter((_parameter, current_index) => current_index !== index));
  };

  const toggleCapability = (capability: string, checked: boolean) => {
    setCapabilities((current) =>
      checked ? [...current, capability] : current.filter((value) => value !== capability),
    );
  };

  const submitDisabled =
    saving || templateBody.trim() === '' || (requireName && stackName.trim() === '');

  return (
    <div data-testid={`${testIdPrefix}-form`} style={formStyle}>
      {requireName ? (
        <div style={fieldRowStyle}>
          <label style={labelStyle} htmlFor={`${testIdPrefix}-stackName`}>
            Stack name
          </label>
          <input
            id={`${testIdPrefix}-stackName`}
            type="text"
            data-testid={`${testIdPrefix}-stackName`}
            style={inputStyle}
            value={stackName}
            onChange={(event) => setStackName(event.target.value)}
          />
        </div>
      ) : null}
      <div style={fieldRowStyle}>
        <label style={labelStyle} htmlFor={`${testIdPrefix}-templateBody`}>
          Template body (JSON or YAML)
        </label>
        <textarea
          id={`${testIdPrefix}-templateBody`}
          data-testid={`${testIdPrefix}-templateBody`}
          style={textareaStyle}
          value={templateBody}
          onChange={(event) => setTemplateBody(event.target.value)}
        />
      </div>
      <div style={fieldRowStyle}>
        <span style={labelStyle}>Parameters</span>
        {parameters.map((parameter, index) => (
          <div key={index} style={parameterRowStyle}>
            <input
              type="text"
              data-testid={`${testIdPrefix}-parameter-key-${index}`}
              style={inputStyle}
              placeholder="Key"
              value={parameter.parameterKey}
              onChange={(event) => updateParameter(index, 'parameterKey', event.target.value)}
            />
            <input
              type="text"
              data-testid={`${testIdPrefix}-parameter-value-${index}`}
              style={inputStyle}
              placeholder="Value"
              value={parameter.parameterValue}
              onChange={(event) => updateParameter(index, 'parameterValue', event.target.value)}
            />
            <button
              type="button"
              data-testid={`${testIdPrefix}-parameter-remove-${index}`}
              style={smallButtonStyle}
              onClick={() => removeParameter(index)}
            >
              Remove
            </button>
          </div>
        ))}
        <button
          type="button"
          data-testid={`${testIdPrefix}-parameter-add`}
          style={smallButtonStyle}
          onClick={addParameter}
        >
          Add parameter
        </button>
      </div>
      <div style={fieldRowStyle}>
        <span style={labelStyle}>Capabilities</span>
        {AVAILABLE_CAPABILITIES.map((capability) => (
          <div key={capability} style={checkboxRowStyle}>
            <input
              id={`${testIdPrefix}-capability-${capability}`}
              type="checkbox"
              data-testid={`${testIdPrefix}-capability-${capability}`}
              checked={capabilities.includes(capability)}
              onChange={(event) => toggleCapability(capability, event.target.checked)}
            />
            <label style={labelStyle} htmlFor={`${testIdPrefix}-capability-${capability}`}>
              {capability}
            </label>
          </div>
        ))}
      </div>
      <button
        type="button"
        data-testid={`${testIdPrefix}-submit`}
        style={buttonStyle}
        disabled={submitDisabled}
        onClick={() => onSubmit({ stackName, templateBody, parameters, capabilities })}
      >
        {saving ? `${submitLabel}\u2026` : submitLabel}
      </button>
    </div>
  );
}

export default CloudFormationStackForm;
