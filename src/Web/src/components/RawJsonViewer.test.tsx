import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { RawJsonViewer } from './RawJsonViewer';

const writeText = vi.fn<(text: string) => Promise<void>>();

beforeEach(() => {
  writeText.mockReset();
  writeText.mockResolvedValue(undefined);
  Object.defineProperty(navigator, 'clipboard', {
    configurable: true,
    value: { writeText },
  });
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe('RawJsonViewer', () => {
  it('renders the default title collapsed without copy or content', () => {
    render(<RawJsonViewer value={{ a: 1 }} />);

    expect(screen.getByTestId('raw-json-title')).toHaveTextContent('Raw JSON');
    expect(screen.getByTestId('raw-json-collapsed')).toBeInTheDocument();
    expect(screen.queryByTestId('raw-json-content')).not.toBeInTheDocument();
    expect(screen.queryByTestId('raw-json-copy')).not.toBeInTheDocument();
    expect(screen.getByTestId('raw-json-toggle')).toHaveTextContent('Show');
  });

  it('uses a custom title when provided', () => {
    render(<RawJsonViewer value={{}} title="Bucket response" />);

    expect(screen.getByTestId('raw-json-title')).toHaveTextContent('Bucket response');
  });

  it('expands to show pretty-printed JSON when initiallyExpanded', () => {
    render(<RawJsonViewer value={{ name: 's3', items: [1, 2] }} initiallyExpanded />);

    const content = screen.getByTestId('raw-json-content');
    expect(content).toHaveTextContent('"name": "s3"');
    expect(content).toHaveTextContent('"items"');
    expect(screen.getByTestId('raw-json-copy')).toHaveTextContent('Copy');
  });

  it('renders a plain string verbatim (without quotes) when renderStringAsText is set', () => {
    render(<RawJsonViewer value="test order 1" renderStringAsText initiallyExpanded />);

    const content = screen.getByTestId('raw-json-content');
    expect(content).toHaveTextContent('test order 1');
    expect(content.textContent).toBe('test order 1');
  });

  it('still JSON-stringifies a non-string value when renderStringAsText is set', () => {
    render(<RawJsonViewer value={{ a: 1 }} renderStringAsText initiallyExpanded />);

    expect(screen.getByTestId('raw-json-content')).toHaveTextContent('"a": 1');
  });

  it('toggles between hidden and shown', async () => {
    render(<RawJsonViewer value={{ a: 1 }} />);

    await userEvent.click(screen.getByTestId('raw-json-toggle'));
    expect(screen.getByTestId('raw-json-content')).toBeInTheDocument();
    expect(screen.getByTestId('raw-json-toggle')).toHaveTextContent('Hide');

    await userEvent.click(screen.getByTestId('raw-json-toggle'));
    expect(screen.queryByTestId('raw-json-content')).not.toBeInTheDocument();
    expect(screen.getByTestId('raw-json-collapsed')).toBeInTheDocument();
  });

  it('masks sensitive keys, including nested objects and arrays', () => {
    render(
      <RawJsonViewer
        value={{
          accessKey: 'AKIA123',
          region: 'eu-west-1',
          nested: { secretKey: 'shh', keep: 'visible' },
          items: [{ secretKey: 'also-hidden', label: 'ok' }],
        }}
        sensitiveKeys={['accessKey', 'secretKey']}
        initiallyExpanded
      />,
    );

    const content = screen.getByTestId('raw-json-content');
    expect(content).toHaveTextContent('"accessKey": "********"');
    expect(content).toHaveTextContent('"secretKey": "********"');
    expect(content).toHaveTextContent('"region": "eu-west-1"');
    expect(content).toHaveTextContent('"keep": "visible"');
    expect(content).not.toHaveTextContent('AKIA123');
    expect(content).not.toHaveTextContent('shh');
    expect(content).not.toHaveTextContent('also-hidden');
  });

  it('copies the JSON to the clipboard and reports success', async () => {
    render(<RawJsonViewer value={{ a: 1 }} initiallyExpanded />);

    await userEvent.click(screen.getByTestId('raw-json-copy'));

    expect(writeText).toHaveBeenCalledWith(JSON.stringify({ a: 1 }, null, 2));
    expect(screen.getByTestId('raw-json-copy')).toHaveTextContent('Copied');
  });

  it('keeps the copy label when the clipboard write fails', async () => {
    writeText.mockRejectedValue(new Error('denied'));
    render(<RawJsonViewer value={{ a: 1 }} initiallyExpanded />);

    await userEvent.click(screen.getByTestId('raw-json-copy'));

    expect(screen.getByTestId('raw-json-copy')).toHaveTextContent('Copy');
  });

  it('resets the copied state after hiding and showing again', async () => {
    render(<RawJsonViewer value={{ a: 1 }} initiallyExpanded />);

    await userEvent.click(screen.getByTestId('raw-json-copy'));
    expect(screen.getByTestId('raw-json-copy')).toHaveTextContent('Copied');

    await userEvent.click(screen.getByTestId('raw-json-toggle'));
    await userEvent.click(screen.getByTestId('raw-json-toggle'));

    expect(screen.getByTestId('raw-json-copy')).toHaveTextContent('Copy');
  });
});
