// Глобальные DOM-помощники для клиентского интерфейса.
window.App = {
    setStatus(message, type) {
        const region = document.getElementById('pageMessage');
        if (!region) return;
        region.textContent = message;
        region.className = 'page-message ' + (type || '');
        if (message) window.setTimeout(() => { region.textContent = ''; region.className = 'page-message'; }, 5000);
    },
    escapeHtml(value) {
        const node = document.createElement('span');
        node.textContent = value == null ? '' : String(value);
        return node.innerHTML;
    }
};
