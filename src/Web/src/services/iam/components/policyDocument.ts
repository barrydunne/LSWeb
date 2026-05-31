/**
 * Narrow an unknown value to a plain JSON object (excluding arrays and null).
 */
export function isPlainObject(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

/**
 * Validate the textual representation of an IAM policy document, returning a list of
 * human-readable error messages. An empty list means the document is valid.
 */
export function validatePolicyDocument(text: string): string[] {
  let parsed: unknown;
  try {
    parsed = JSON.parse(text);
  } catch {
    return ['Policy document must be valid JSON.'];
  }

  if (!isPlainObject(parsed)) {
    return ['Policy document must be a JSON object.'];
  }

  const errors: string[] = [];
  if (parsed.Version === undefined) {
    errors.push('Policy document must include a "Version" field.');
  }

  if (parsed.Statement === undefined) {
    errors.push('Policy document must include a "Statement" field.');
  } else {
    const statements = Array.isArray(parsed.Statement) ? parsed.Statement : [parsed.Statement];
    statements.forEach((statement, index) => {
      const position = index + 1;
      if (!isPlainObject(statement)) {
        errors.push(`Statement ${position} must be a JSON object.`);
        return;
      }
      if (statement.Effect === undefined) {
        errors.push(`Statement ${position} must include an "Effect".`);
      }
      if (statement.Action === undefined) {
        errors.push(`Statement ${position} must include an "Action".`);
      }
      // Trust/resource policies identify the trusted entity with a "Principal" and have no
      // "Resource"; identity policies require a "Resource". Only enforce "Resource" when the
      // statement does not declare a "Principal".
      if (statement.Principal === undefined && statement.Resource === undefined) {
        errors.push(`Statement ${position} must include a "Resource".`);
      }
    });
  }

  return errors;
}

/**
 * Pretty-print a policy document as indented JSON.
 */
export function formatPolicyDocument(value: unknown): string {
  return JSON.stringify(value, null, 2);
}
