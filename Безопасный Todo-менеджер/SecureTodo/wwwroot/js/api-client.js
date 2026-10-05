// Небольшой пример клиента API. Токены хранятся только в этой вкладке.
window.todoApi = {
  async login(email, password, twoFactorCode) {
    const response = await fetch('/api/auth/login', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password, twoFactorCode })
    });
    if (!response.ok) throw new Error(`Вход в API: HTTP ${response.status}`);
    const pair = await response.json();
    sessionStorage.setItem('todoAccess', pair.accessToken);
    sessionStorage.setItem('todoRefresh', pair.refreshToken);
    return pair;
  },
  async request(path, options = {}) {
    const send = token => fetch(path, {
      ...options,
      headers: { ...options.headers, Authorization: `Bearer ${token}` }
    });
    let response = await send(sessionStorage.getItem('todoAccess') || '');
    if (response.status !== 401) return response;
    const refreshToken = sessionStorage.getItem('todoRefresh');
    if (refreshToken) {
      const refresh = await fetch('/api/auth/refresh', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken })
      });
      if (refresh.ok) {
        const pair = await refresh.json();
        sessionStorage.setItem('todoAccess', pair.accessToken);
        sessionStorage.setItem('todoRefresh', pair.refreshToken);
        return send(pair.accessToken);
      }
    }
    sessionStorage.removeItem('todoAccess');
    sessionStorage.removeItem('todoRefresh');
    location.assign('/account/login');
    return response;
  },
  async logoutAll() {
    const response = await this.request('/api/auth/logout-all', { method: 'POST' });
    if (response.ok) {
      sessionStorage.removeItem('todoAccess');
      sessionStorage.removeItem('todoRefresh');
    }
    return response;
  }
};
