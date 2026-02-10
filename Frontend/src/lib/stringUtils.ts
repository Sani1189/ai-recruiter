/**
 * High-performance string utilities for semantic name generation
 */

/**
 * Converts text to a URL-friendly slug (lowercase, underscores, alphanumeric only)
 * @param text - Input text to slugify
 * @param maxLength - Maximum length of the slug (default: 255)
 * @returns Slugified string or empty if input is null/empty
 */
export function slugify(text: string | null | undefined, maxLength = 255): string {
  if (!text?.trim()) return "";

  // Convert to lowercase and trim
  let slug = text.toLowerCase().trim();

  // Truncate if needed before processing
  if (slug.length > maxLength) {
    slug = slug.slice(0, maxLength);
  }

  // Replace non-alphanumeric characters with underscores
  slug = slug.replace(/[^a-z0-9]+/g, "_");

  // Replace multiple consecutive underscores with single underscore
  slug = slug.replace(/_{2,}/g, "_");

  // Remove leading/trailing underscores
  slug = slug.replace(/^_+|_+$/g, "");

  return slug;
}

/**
 * Ensures name uniqueness by appending a numeric suffix if needed
 * @param baseName - Base name to make unique
 * @param existingNames - Set of existing names to check against
 * @param maxLength - Maximum length of the final name
 * @returns Unique name with suffix if necessary
 */
export function ensureUniqueName(
  baseName: string,
  existingNames: Set<string>,
  maxLength = 255
): string {
  if (!baseName?.trim()) return "";

  if (!existingNames.has(baseName)) {
    return baseName;
  }

  // Calculate max length for base name to leave room for suffix
  const suffixLength = 4; // "_999" worst case
  const maxBaseLength = maxLength - suffixLength;
  const truncatedBase = baseName.length > maxBaseLength 
    ? baseName.slice(0, maxBaseLength) 
    : baseName;

  // Find next available number
  for (let i = 2; i < 1000; i++) {
    const candidateName = `${truncatedBase}_${i}`;
    if (!existingNames.has(candidateName)) {
      return candidateName;
    }
  }

  // Fallback: append timestamp
  return `${truncatedBase}_${Date.now()}`;
}

/**
 * Validates that a name conforms to the expected format
 * @param name - Name to validate
 * @returns True if valid, false otherwise
 */
export function isValidName(name: string | null | undefined): boolean {
  if (!name?.trim()) return false;

  // Only lowercase letters, numbers, and underscores
  return /^[a-z0-9_]+$/.test(name);
}
