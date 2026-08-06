window.sessionTracker = {
  saveSession: function (sessionId, jsonData) {
    localStorage.setItem(sessionId, jsonData);
  },
  loadSession: function (sessionId) {
    return localStorage.getItem(sessionId);
  },
};
