window.goiMonTheme = {
  storageKey: "goimon.theme.mode",

  initialize() {
    const savedMode = localStorage.getItem(this.storageKey);
    const prefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
    const isDarkMode = savedMode === null ? prefersDark : savedMode === "dark";

    document.documentElement.classList.toggle("dark", isDarkMode);
    return isDarkMode;
  },

  setDarkMode(isDarkMode) {
    const enabled = !!isDarkMode;
    document.documentElement.classList.toggle("dark", enabled);
    localStorage.setItem(this.storageKey, enabled ? "dark" : "light");
  },

  getDarkMode() {
    return document.documentElement.classList.contains("dark");
  }
};