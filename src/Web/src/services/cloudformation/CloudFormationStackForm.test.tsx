import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { CloudFormationStackForm } from './CloudFormationStackForm';

describe('CloudFormationStackForm', () => {
  it('shows the stack name field only when a name is required', () => {
    const { rerender } = render(
      <CloudFormationStackForm
        testIdPrefix="cfn"
        submitLabel="Create"
        saving={false}
        onSubmit={vi.fn()}
      />,
    );

    expect(screen.queryByTestId('cfn-stackName')).not.toBeInTheDocument();

    rerender(
      <CloudFormationStackForm
        testIdPrefix="cfn"
        submitLabel="Create"
        saving={false}
        requireName
        onSubmit={vi.fn()}
      />,
    );

    expect(screen.getByTestId('cfn-stackName')).toBeInTheDocument();
  });

  it('disables the submit button until a template body is provided', () => {
    render(
      <CloudFormationStackForm
        testIdPrefix="cfn"
        submitLabel="Update"
        saving={false}
        onSubmit={vi.fn()}
      />,
    );

    expect(screen.getByTestId('cfn-submit')).toBeDisabled();

    fireEvent.change(screen.getByTestId('cfn-templateBody'), {
      target: { value: '{}' },
    });

    expect(screen.getByTestId('cfn-submit')).toBeEnabled();
  });

  it('keeps submit disabled when a name is required but missing', () => {
    render(
      <CloudFormationStackForm
        testIdPrefix="cfn"
        submitLabel="Create"
        saving={false}
        requireName
        onSubmit={vi.fn()}
      />,
    );

    fireEvent.change(screen.getByTestId('cfn-templateBody'), {
      target: { value: '{}' },
    });
    expect(screen.getByTestId('cfn-submit')).toBeDisabled();

    fireEvent.change(screen.getByTestId('cfn-stackName'), {
      target: { value: 'my-stack' },
    });
    expect(screen.getByTestId('cfn-submit')).toBeEnabled();
  });

  it('disables submit and appends an ellipsis to the label while saving', () => {
    render(
      <CloudFormationStackForm
        testIdPrefix="cfn"
        submitLabel="Create"
        saving
        initialTemplateBody="{}"
        onSubmit={vi.fn()}
      />,
    );

    const submit = screen.getByTestId('cfn-submit');
    expect(submit).toBeDisabled();
    expect(submit).toHaveTextContent('Create\u2026');
  });

  it('adds, edits and removes parameter rows', () => {
    const onSubmit = vi.fn();
    render(
      <CloudFormationStackForm
        testIdPrefix="cfn"
        submitLabel="Create"
        saving={false}
        initialTemplateBody="{}"
        onSubmit={onSubmit}
      />,
    );

    fireEvent.click(screen.getByTestId('cfn-parameter-add'));
    fireEvent.click(screen.getByTestId('cfn-parameter-add'));
    fireEvent.change(screen.getByTestId('cfn-parameter-key-0'), {
      target: { value: 'First' },
    });
    fireEvent.change(screen.getByTestId('cfn-parameter-value-0'), {
      target: { value: 'one' },
    });
    fireEvent.change(screen.getByTestId('cfn-parameter-key-1'), {
      target: { value: 'Second' },
    });

    fireEvent.click(screen.getByTestId('cfn-parameter-remove-1'));
    fireEvent.click(screen.getByTestId('cfn-submit'));

    expect(onSubmit).toHaveBeenCalledWith({
      stackName: '',
      templateBody: '{}',
      parameters: [{ parameterKey: 'First', parameterValue: 'one' }],
      capabilities: [],
    });
  });

  it('toggles capabilities on and off', () => {
    const onSubmit = vi.fn();
    render(
      <CloudFormationStackForm
        testIdPrefix="cfn"
        submitLabel="Create"
        saving={false}
        initialTemplateBody="{}"
        onSubmit={onSubmit}
      />,
    );

    fireEvent.click(screen.getByTestId('cfn-capability-CAPABILITY_IAM'));
    fireEvent.click(screen.getByTestId('cfn-capability-CAPABILITY_AUTO_EXPAND'));
    fireEvent.click(screen.getByTestId('cfn-capability-CAPABILITY_IAM'));

    fireEvent.click(screen.getByTestId('cfn-submit'));

    expect(onSubmit).toHaveBeenCalledWith({
      stackName: '',
      templateBody: '{}',
      parameters: [],
      capabilities: ['CAPABILITY_AUTO_EXPAND'],
    });
  });

  it('seeds the form from the supplied initial values', () => {
    const onSubmit = vi.fn();
    render(
      <CloudFormationStackForm
        testIdPrefix="cfn"
        submitLabel="Update"
        saving={false}
        initialTemplateBody='{"seed":true}'
        initialParameters={[{ parameterKey: 'Env', parameterValue: 'dev' }]}
        initialCapabilities={['CAPABILITY_NAMED_IAM']}
        onSubmit={onSubmit}
      />,
    );

    expect(screen.getByTestId('cfn-templateBody')).toHaveValue('{"seed":true}');
    expect(screen.getByTestId('cfn-parameter-key-0')).toHaveValue('Env');
    expect(screen.getByTestId('cfn-capability-CAPABILITY_NAMED_IAM')).toBeChecked();

    fireEvent.click(screen.getByTestId('cfn-submit'));

    expect(onSubmit).toHaveBeenCalledWith({
      stackName: '',
      templateBody: '{"seed":true}',
      parameters: [{ parameterKey: 'Env', parameterValue: 'dev' }],
      capabilities: ['CAPABILITY_NAMED_IAM'],
    });
  });
});
