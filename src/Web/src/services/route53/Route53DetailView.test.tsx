import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { Route53DetailView } from './Route53DetailView';
import {
  deleteRoute53Record,
  getRoute53Records,
  upsertRoute53Record,
} from '../../api/client';
import type { Route53RecordListResult } from '../../api/client';

vi.mock('../../api/client');

const getRecordsMock = vi.mocked(getRoute53Records);
const upsertRecordMock = vi.mocked(upsertRoute53Record);
const deleteRecordMock = vi.mocked(deleteRoute53Record);

const records: Route53RecordListResult = {
  records: [
    { name: 'example.com.', type: 'NS', ttl: 172800, values: ['ns-1.example.org.'] },
    { name: 'www.example.com.', type: 'A', ttl: 300, values: ['1.2.3.4'] },
  ],
};

function renderView() {
  return render(<Route53DetailView serviceKey="route53" resourceId="/hostedzone/Z1" />);
}

describe('Route53DetailView', () => {
  beforeEach(() => {
    getRecordsMock.mockResolvedValue(records);
    upsertRecordMock.mockResolvedValue();
    deleteRecordMock.mockResolvedValue();
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('shows a loading state before records arrive', () => {
    getRecordsMock.mockReturnValue(new Promise(() => {}));

    renderView();

    expect(screen.getByTestId('route53-detail-loading')).toBeInTheDocument();
  });

  it('shows an error state when the request fails', async () => {
    getRecordsMock.mockRejectedValue(new Error('boom'));

    renderView();

    await waitFor(() => expect(screen.getByTestId('route53-detail-error')).toBeInTheDocument());
  });

  it('renders a row per record', async () => {
    renderView();

    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());
    expect(screen.getAllByTestId('route53-record-row')).toHaveLength(2);
    expect(screen.getAllByTestId('route53-record-values')[1]).toHaveTextContent('1.2.3.4');
  });

  it('shows an empty state when there are no records', async () => {
    getRecordsMock.mockResolvedValue({ records: [] });

    renderView();

    await waitFor(() =>
      expect(screen.getByTestId('route53-detail-records-empty')).toBeInTheDocument(),
    );
  });

  it('creates a record from the form', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('route53-record-form-name'), {
      target: { value: 'api.example.com.' },
    });
    fireEvent.change(screen.getByTestId('route53-record-form-type'), { target: { value: 'CNAME' } });
    fireEvent.change(screen.getByTestId('route53-record-form-ttl'), { target: { value: '600' } });
    fireEvent.change(screen.getByTestId('route53-record-form-values'), {
      target: { value: 'target.example.com.\n' },
    });
    fireEvent.click(screen.getByTestId('route53-record-form-submit'));

    await waitFor(() => expect(screen.getByTestId('route53-record-status')).toBeInTheDocument());
    expect(upsertRecordMock).toHaveBeenCalledWith('/hostedzone/Z1', {
      name: 'api.example.com.',
      type: 'CNAME',
      ttl: 600,
      values: ['target.example.com.'],
    });
  });

  it('shows a saving label while the save is in flight', async () => {
    upsertRecordMock.mockReturnValue(new Promise(() => {}));
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('route53-record-form-name'), {
      target: { value: 'api.example.com.' },
    });
    fireEvent.change(screen.getByTestId('route53-record-form-values'), {
      target: { value: '1.2.3.4' },
    });
    fireEvent.click(screen.getByTestId('route53-record-form-submit'));

    expect(screen.getByTestId('route53-record-form-submit')).toBeDisabled();
    expect(screen.getByTestId('route53-record-form-submit')).toHaveTextContent('Saving');
  });

  it('blocks the save when the name is empty', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getByTestId('route53-record-form-submit'));

    expect(screen.getByTestId('route53-record-form-error')).toHaveTextContent('Enter a record name.');
    expect(upsertRecordMock).not.toHaveBeenCalled();
  });

  it('blocks the save when the TTL is not a positive whole number', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('route53-record-form-name'), {
      target: { value: 'api.example.com.' },
    });
    fireEvent.change(screen.getByTestId('route53-record-form-ttl'), { target: { value: 'abc' } });
    fireEvent.click(screen.getByTestId('route53-record-form-submit'));

    expect(screen.getByTestId('route53-record-form-error')).toHaveTextContent('positive whole number');
    expect(upsertRecordMock).not.toHaveBeenCalled();
  });

  it('blocks the save when the TTL is zero', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('route53-record-form-name'), {
      target: { value: 'api.example.com.' },
    });
    fireEvent.change(screen.getByTestId('route53-record-form-ttl'), { target: { value: '0' } });
    fireEvent.click(screen.getByTestId('route53-record-form-submit'));

    expect(screen.getByTestId('route53-record-form-error')).toHaveTextContent('positive whole number');
    expect(upsertRecordMock).not.toHaveBeenCalled();
  });

  it('blocks the save when there are no values', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('route53-record-form-name'), {
      target: { value: 'api.example.com.' },
    });
    fireEvent.click(screen.getByTestId('route53-record-form-submit'));

    expect(screen.getByTestId('route53-record-form-error')).toHaveTextContent('at least one record value');
    expect(upsertRecordMock).not.toHaveBeenCalled();
  });

  it('shows an error when the save fails', async () => {
    upsertRecordMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());

    fireEvent.change(screen.getByTestId('route53-record-form-name'), {
      target: { value: 'api.example.com.' },
    });
    fireEvent.change(screen.getByTestId('route53-record-form-values'), {
      target: { value: '1.2.3.4' },
    });
    fireEvent.click(screen.getByTestId('route53-record-form-submit'));

    await waitFor(() =>
      expect(screen.getByTestId('route53-record-form-error')).toHaveTextContent(
        'Unable to save the record.',
      ),
    );
  });

  it('prefills the form when editing a record', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('route53-record-edit')[1]);

    expect(screen.getByTestId('route53-record-form-name')).toHaveValue('www.example.com.');
    expect(screen.getByTestId('route53-record-form-type')).toHaveValue('A');
    expect(screen.getByTestId('route53-record-form-ttl')).toHaveValue('300');
    expect(screen.getByTestId('route53-record-form-values')).toHaveValue('1.2.3.4');
  });

  it('deletes a record', async () => {
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('route53-record-delete')[1]);

    await waitFor(() =>
      expect(deleteRecordMock).toHaveBeenCalledWith('/hostedzone/Z1', {
        name: 'www.example.com.',
        type: 'A',
        ttl: 300,
        values: ['1.2.3.4'],
      }),
    );
  });

  it('shows an error when the delete fails', async () => {
    deleteRecordMock.mockRejectedValue(new Error('boom'));
    renderView();
    await waitFor(() => expect(screen.getByTestId('route53-detail-view')).toBeInTheDocument());

    fireEvent.click(screen.getAllByTestId('route53-record-delete')[1]);

    await waitFor(() =>
      expect(screen.getByTestId('route53-record-form-error')).toBeInTheDocument(),
    );
  });
});
