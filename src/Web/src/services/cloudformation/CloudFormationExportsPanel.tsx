import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { Heading } from '@primer/react';
import { Link } from 'react-router-dom';
import { getExports, getImports } from '../../api/client';
import type { CloudFormationExport } from '../../api/client';

const sectionStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};
const sectionHeadingStyle: CSSProperties = { fontSize: 14 };
const messageStyle: CSSProperties = { fontSize: 14 };
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
  verticalAlign: 'top',
};
const headerCellStyle: CSSProperties = {
  ...cellStyle,
  fontFamily: 'inherit',
  opacity: 0.7,
};
const linkButtonStyle: CSSProperties = {
  fontSize: 13,
  fontFamily: 'monospace',
  padding: 0,
  border: 'none',
  background: 'none',
  color: '#58a6ff',
  cursor: 'pointer',
  textAlign: 'left',
};
const linkStyle: CSSProperties = {
  color: '#58a6ff',
  textDecoration: 'none',
};
const importListStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
  margin: 0,
  paddingLeft: 16,
};

/**
 * Extracts the stack name from a CloudFormation stack ARN so the exporting stack can deep-link to
 * its own detail view. Returns the original value when it is not a recognisable stack ARN.
 */
function stackNameFromArn(stackId: string): string | null {
  const match = /:stack\/([^/]+)\//.exec(stackId);
  return match ? match[1] : null;
}

type ExportsState =
  | { kind: 'loading' }
  | { kind: 'ready'; exports: CloudFormationExport[] }
  | { kind: 'error' };

type ImportsState =
  | { kind: 'loading' }
  | { kind: 'ready'; stackNames: string[] }
  | { kind: 'error' };

function StackLink({ name }: { name: string }) {
  return (
    <Link to={`/services/cloudformation/${encodeURIComponent(name)}`} style={linkStyle}>
      {name}
    </Link>
  );
}

export function CloudFormationExportsPanel() {
  const [state, setState] = useState<ExportsState>({ kind: 'loading' });
  const [selectedExport, setSelectedExport] = useState<string | null>(null);
  const [imports, setImports] = useState<ImportsState>({ kind: 'loading' });

  useEffect(() => {
    const controller = new AbortController();
    getExports(controller.signal)
      .then((result) => setState({ kind: 'ready', exports: result.exports }))
      .catch(() => setState({ kind: 'error' }));
    return () => controller.abort();
  }, []);

  const showImports = (exportName: string) => {
    setSelectedExport(exportName);
    setImports({ kind: 'loading' });
    getImports(exportName)
      .then((result) => setImports({ kind: 'ready', stackNames: result.importingStackNames }))
      .catch(() => setImports({ kind: 'error' }));
  };

  return (
    <div style={sectionStyle}>
      <Heading as="h3" data-testid="cloudformation-exports-heading" style={sectionHeadingStyle}>
        Exports
      </Heading>
      {state.kind === 'loading' ? (
        <p data-testid="cloudformation-exports-loading" style={messageStyle}>
          Loading exports&hellip;
        </p>
      ) : null}
      {state.kind === 'error' ? (
        <p data-testid="cloudformation-exports-error" style={messageStyle}>
          Unable to load exports.
        </p>
      ) : null}
      {state.kind === 'ready' && state.exports.length === 0 ? (
        <p data-testid="cloudformation-exports-empty" style={messageStyle}>
          No exports found.
        </p>
      ) : null}
      {state.kind === 'ready' && state.exports.length > 0 ? (
        <table data-testid="cloudformation-exports-table" style={tableStyle}>
          <thead>
            <tr>
              <th style={headerCellStyle}>Name</th>
              <th style={headerCellStyle}>Value</th>
              <th style={headerCellStyle}>Exporting stack</th>
              <th style={headerCellStyle}>Imported by</th>
            </tr>
          </thead>
          <tbody>
            {state.exports.map((item) => {
              const exportingStackName = stackNameFromArn(item.exportingStackId);
              return (
                <tr key={item.name}>
                  <td style={cellStyle}>{item.name}</td>
                  <td style={cellStyle}>{item.value}</td>
                  <td style={cellStyle}>
                    {exportingStackName === null ? (
                      item.exportingStackId
                    ) : (
                      <StackLink name={exportingStackName} />
                    )}
                  </td>
                  <td style={cellStyle}>
                    <button
                      type="button"
                      data-testid={`cloudformation-export-imports-${item.name}`}
                      style={linkButtonStyle}
                      onClick={() => showImports(item.name)}
                    >
                      Show importing stacks
                    </button>
                    {selectedExport === item.name ? (
                      <div>
                        {imports.kind === 'loading' ? (
                          <p
                            data-testid="cloudformation-imports-loading"
                            style={messageStyle}
                          >
                            Loading importing stacks&hellip;
                          </p>
                        ) : null}
                        {imports.kind === 'error' ? (
                          <p data-testid="cloudformation-imports-error" style={messageStyle}>
                            Unable to load importing stacks.
                          </p>
                        ) : null}
                        {imports.kind === 'ready' && imports.stackNames.length === 0 ? (
                          <p data-testid="cloudformation-imports-empty" style={messageStyle}>
                            No stacks import this export.
                          </p>
                        ) : null}
                        {imports.kind === 'ready' && imports.stackNames.length > 0 ? (
                          <ul
                            data-testid="cloudformation-imports-list"
                            style={importListStyle}
                          >
                            {imports.stackNames.map((stackName) => (
                              <li key={stackName}>
                                <StackLink name={stackName} />
                              </li>
                            ))}
                          </ul>
                        ) : null}
                      </div>
                    ) : null}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      ) : null}
    </div>
  );
}

export default CloudFormationExportsPanel;
