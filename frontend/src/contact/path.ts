export const CONTACT_PATH = "/kontakt";
export const THANKS_PATH = "/tak";

export function isContactPath(path: string) {
  return path.replace(/\/+$/, "").toLowerCase() === CONTACT_PATH;
}

export function isThanksPath(path: string) {
  return path.replace(/\/+$/, "").toLowerCase() === THANKS_PATH;
}
