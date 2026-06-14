import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { TrustPolicyBuilder } from './TrustPolicyBuilder';

function renderBuilder(value: unknown, onSave = vi.fn()) {
  render(<TrustPolicyBuilder value={value} onSave={onSave} testId="trust" />);
  return onSave;
}

describe('TrustPolicyBuilder', () => {
  it('initializes a row from a service trust document and previews the generated JSON', () => {
    renderBuilder({
      Version: '2012-10-17',
      Statement: [
        { Effect: 'Allow', Principal: { Service: 'lambda.amazonaws.com' }, Action: 'sts:AssumeRole' },
      ],
    });

    expect(screen.getByTestId('trust-row-value')).toHaveValue('lambda.amazonaws.com');
    expect(screen.getByTestId('trust-row-action')).toHaveValue('sts:AssumeRole');
    expect(screen.getByTestId('trust-preview')).toHaveTextContent('lambda.amazonaws.com');
  });

  it('expands string, array and federated principals into one row each', () => {
    renderBuilder({
      Statement: [
        { Principal: '*', Action: 'sts:AssumeRole' },
        {
          Principal: { AWS: ['arn:a', 'arn:b'], Federated: 'saml-provider' },
          Action: ['sts:AssumeRole', 'sts:TagSession'],
        },
      ],
    });

    expect(screen.getAllByTestId('trust-row')).toHaveLength(4);
  });

  it('ignores malformed statements and non-string principals', () => {
    renderBuilder({
      Statement: ['not-an-object', { Principal: 42, Action: 'x' }, { Principal: { Service: [123, 'ok'] } }],
    });

    const rows = screen.getAllByTestId('trust-row');
    expect(rows).toHaveLength(1);
    expect(screen.getByTestId('trust-row-value')).toHaveValue('ok');
    expect(screen.getByTestId('trust-row-action')).toHaveValue('sts:AssumeRole');
  });

  it('accepts a single (non-array) statement object', () => {
    renderBuilder({
      Statement: { Principal: { Service: 'ec2.amazonaws.com' }, Action: 'sts:AssumeRole' },
    });

    expect(screen.getAllByTestId('trust-row')).toHaveLength(1);
    expect(screen.getByTestId('trust-row-value')).toHaveValue('ec2.amazonaws.com');
  });

  it('renders the empty state for a non-object document', () => {
    renderBuilder(null);

    expect(screen.getByTestId('trust-empty')).toBeInTheDocument();
    expect(screen.queryByTestId('trust-row')).not.toBeInTheDocument();
  });

  it('renders the empty state when the document has no statements', () => {
    renderBuilder({});

    expect(screen.getByTestId('trust-empty')).toBeInTheDocument();
  });

  it('adds a principal, edits its type and value, and saves the generated document', () => {
    const onSave = renderBuilder({});

    fireEvent.click(screen.getByTestId('trust-add'));
    fireEvent.change(screen.getByTestId('trust-row-type'), { target: { value: 'AWS' } });
    fireEvent.change(screen.getByTestId('trust-row-value'), {
      target: { value: 'arn:aws:iam::123456789012:root' },
    });
    fireEvent.click(screen.getByTestId('trust-save'));

    expect(onSave).toHaveBeenCalledWith({
      Version: '2012-10-17',
      Statement: [
        {
          Effect: 'Allow',
          Principal: { AWS: 'arn:aws:iam::123456789012:root' },
          Action: 'sts:AssumeRole',
        },
      ],
    });
  });

  it('shows an error and does not save when there are no principals', () => {
    const onSave = renderBuilder({});

    fireEvent.click(screen.getByTestId('trust-save'));

    expect(screen.getByTestId('trust-errors')).toBeInTheDocument();
    expect(onSave).not.toHaveBeenCalled();
  });

  it('shows an error when a principal value is blank', () => {
    const onSave = renderBuilder({});

    fireEvent.click(screen.getByTestId('trust-add'));
    fireEvent.click(screen.getByTestId('trust-save'));

    expect(screen.getByTestId('trust-errors')).toBeInTheDocument();
    expect(onSave).not.toHaveBeenCalled();
  });

  it('removes a principal row', () => {
    renderBuilder({
      Statement: [{ Principal: { Service: 'lambda.amazonaws.com' }, Action: 'sts:AssumeRole' }],
    });

    fireEvent.click(screen.getByTestId('trust-row-remove'));

    expect(screen.queryByTestId('trust-row')).not.toBeInTheDocument();
    expect(screen.getByTestId('trust-empty')).toBeInTheDocument();
  });

  it('edits only the targeted row when several principals exist', () => {
    const onSave = renderBuilder({
      Statement: [
        { Principal: { Service: 'lambda.amazonaws.com' }, Action: 'sts:AssumeRole' },
        { Principal: { Service: 'ec2.amazonaws.com' }, Action: 'sts:AssumeRole' },
      ],
    });

    const types = screen.getAllByTestId('trust-row-type');
    const values = screen.getAllByTestId('trust-row-value');
    const actions = screen.getAllByTestId('trust-row-action');

    fireEvent.change(types[1], { target: { value: 'AWS' } });
    fireEvent.change(values[1], { target: { value: 'arn:aws:iam::999:root' } });
    fireEvent.change(actions[1], { target: { value: 'sts:AssumeRoleWithSAML' } });
    fireEvent.click(screen.getByTestId('trust-save'));

    expect(onSave).toHaveBeenCalledWith({
      Version: '2012-10-17',
      Statement: [
        { Effect: 'Allow', Principal: { Service: 'lambda.amazonaws.com' }, Action: 'sts:AssumeRole' },
        { Effect: 'Allow', Principal: { AWS: 'arn:aws:iam::999:root' }, Action: 'sts:AssumeRoleWithSAML' },
      ],
    });
  });

  it('defaults a cleared action back to sts:AssumeRole when saving', () => {
    const onSave = renderBuilder({
      Statement: [{ Principal: { Service: 'lambda.amazonaws.com' }, Action: 'custom:Action' }],
    });

    fireEvent.change(screen.getByTestId('trust-row-action'), { target: { value: '   ' } });
    fireEvent.click(screen.getByTestId('trust-save'));

    expect(onSave).toHaveBeenCalledWith({
      Version: '2012-10-17',
      Statement: [
        { Effect: 'Allow', Principal: { Service: 'lambda.amazonaws.com' }, Action: 'sts:AssumeRole' },
      ],
    });
  });
});
