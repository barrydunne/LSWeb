import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { PolicyDocumentEditor, PolicyDocumentViewer } from './PolicyDocumentEditor';
import { isPlainObject, validatePolicyDocument } from './policyDocument';

const validDocument = {
  Version: '2012-10-17',
  Statement: [
    {
      Effect: 'Allow',
      Action: 's3:GetObject',
      Resource: 'arn:aws:s3:::example/*',
    },
  ],
};

describe('isPlainObject', () => {
  it('accepts a plain object and rejects arrays, null and primitives', () => {
    expect(isPlainObject({})).toBe(true);
    expect(isPlainObject([])).toBe(false);
    expect(isPlainObject(null)).toBe(false);
    expect(isPlainObject(42)).toBe(false);
  });
});

describe('validatePolicyDocument', () => {
  it('returns no errors for a valid document', () => {
    expect(validatePolicyDocument(JSON.stringify(validDocument))).toEqual([]);
  });

  it('reports invalid JSON', () => {
    expect(validatePolicyDocument('{ not json')).toEqual(['Policy document must be valid JSON.']);
  });

  it('reports non-object roots (array, null, primitive)', () => {
    const message = 'Policy document must be a JSON object.';
    expect(validatePolicyDocument('[]')).toEqual([message]);
    expect(validatePolicyDocument('null')).toEqual([message]);
    expect(validatePolicyDocument('123')).toEqual([message]);
  });

  it('reports missing Version and Statement fields', () => {
    const errors = validatePolicyDocument(JSON.stringify({}));
    expect(errors).toContain('Policy document must include a "Version" field.');
    expect(errors).toContain('Policy document must include a "Statement" field.');
  });

  it('accepts a single statement object as well as an array', () => {
    const single = {
      Version: '2012-10-17',
      Statement: { Effect: 'Allow', Action: '*', Resource: '*' },
    };
    expect(validatePolicyDocument(JSON.stringify(single))).toEqual([]);
  });

  it('reports a statement that is not an object', () => {
    const document = { Version: '2012-10-17', Statement: ['nope'] };
    expect(validatePolicyDocument(JSON.stringify(document))).toEqual([
      'Statement 1 must be a JSON object.',
    ]);
  });

  it('reports missing Effect, Action and Resource on a statement', () => {
    const document = { Version: '2012-10-17', Statement: [{}] };
    const errors = validatePolicyDocument(JSON.stringify(document));
    expect(errors).toContain('Statement 1 must include an "Effect".');
    expect(errors).toContain('Statement 1 must include an "Action".');
    expect(errors).toContain('Statement 1 must include a "Resource".');
  });

  it('does not require a Resource when a statement declares a Principal (trust policy)', () => {
    const trustPolicy = {
      Version: '2012-10-17',
      Statement: [
        {
          Effect: 'Allow',
          Principal: { Service: 'lambda.amazonaws.com' },
          Action: 'sts:AssumeRole',
        },
      ],
    };
    expect(validatePolicyDocument(JSON.stringify(trustPolicy))).toEqual([]);
  });
});

describe('PolicyDocumentViewer', () => {  it('renders a pretty-printed read-only document', () => {
    render(<PolicyDocumentViewer value={validDocument} />);

    expect(screen.getByTestId('policy-document-viewer')).toBeInTheDocument();
    expect(screen.getByTestId('policy-document-viewer-content')).toHaveTextContent('2012-10-17');
  });
});

describe('PolicyDocumentEditor', () => {
  it('renders the read-only variant when readOnly is set', () => {
    render(<PolicyDocumentEditor value={validDocument} readOnly />);

    expect(screen.getByTestId('policy-document-editor-readonly')).toBeInTheDocument();
  });

  it('shows the document in view mode and switches to edit mode', () => {
    render(<PolicyDocumentEditor value={validDocument} />);

    expect(screen.getByTestId('policy-document-editor-content')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('policy-document-editor-edit'));

    expect(screen.getByTestId('policy-document-editor-input')).toBeInTheDocument();
    expect(screen.queryByTestId('policy-document-editor-errors')).not.toBeInTheDocument();
    expect(screen.getByTestId('policy-document-editor-save')).not.toBeDisabled();
  });

  it('surfaces validation errors and disables save while invalid', () => {
    render(<PolicyDocumentEditor value={validDocument} />);

    fireEvent.click(screen.getByTestId('policy-document-editor-edit'));
    fireEvent.change(screen.getByTestId('policy-document-editor-input'), {
      target: { value: '{ not json' },
    });

    expect(screen.getByTestId('policy-document-editor-errors')).toBeInTheDocument();
    expect(screen.getByTestId('policy-document-editor-save')).toBeDisabled();
  });

  it('invokes the save callback with the parsed document', () => {
    const onSave = vi.fn();
    render(<PolicyDocumentEditor value={validDocument} onSave={onSave} />);

    fireEvent.click(screen.getByTestId('policy-document-editor-edit'));
    fireEvent.click(screen.getByTestId('policy-document-editor-save'));

    expect(onSave).toHaveBeenCalledWith(validDocument);
    expect(screen.getByTestId('policy-document-editor-content')).toBeInTheDocument();
  });

  it('saves without a callback and reverts edits on cancel', () => {
    render(<PolicyDocumentEditor value={validDocument} />);

    fireEvent.click(screen.getByTestId('policy-document-editor-edit'));
    fireEvent.click(screen.getByTestId('policy-document-editor-save'));
    expect(screen.getByTestId('policy-document-editor-content')).toBeInTheDocument();

    fireEvent.click(screen.getByTestId('policy-document-editor-edit'));
    fireEvent.change(screen.getByTestId('policy-document-editor-input'), {
      target: { value: '{}' },
    });
    fireEvent.click(screen.getByTestId('policy-document-editor-cancel'));

    expect(screen.getByTestId('policy-document-editor-content')).toHaveTextContent('2012-10-17');
  });
});
