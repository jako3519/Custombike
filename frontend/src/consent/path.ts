export const COOKIE_POLICY_PATH = "/cookiepolitik";

export function isCookiePolicyPath(path: string) {
  return path.replace(/\/+$/, "").toLowerCase() === COOKIE_POLICY_PATH;
}
