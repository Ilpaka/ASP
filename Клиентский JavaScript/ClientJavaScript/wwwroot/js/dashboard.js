(() => {
    'use strict';
    const metricsGrid = document.getElementById('metricsGrid');
    const activityList = document.getElementById('activityList');
    const form = document.getElementById('activityForm');
    const formMessage = document.getElementById('formMessage');
    const submitButton = document.getElementById('submitActivity');

    const formatNumber = value => new Intl.NumberFormat('ru-RU').format(value);
    const relativeTime = value => {
        const minutes = Math.max(0, Math.floor((Date.now() - new Date(value).getTime()) / 60000));
        if (minutes < 1) return 'только что';
        if (minutes < 60) return `${minutes} мин. назад`;
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return `${hours} ч. назад`;
        return new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'short' }).format(new Date(value));
    };

    // GET-запросы независимы друг от друга, поэтому отправляем их параллельно.
    async function loadDashboard() {
        metricsGrid.innerHTML = '<div class="loading-card"><span class="spinner"></span> Загружаем показатели…</div>';
        activityList.innerHTML = '<div class="empty-state">Загружаем события…</div>';
        try {
            const [metricsResponse, activityResponse] = await Promise.all([
                fetch('/api/dashboard/metrics', { headers: { Accept: 'application/json' } }),
                fetch('/api/dashboard/activity', { headers: { Accept: 'application/json' } })
            ]);
            if (!metricsResponse.ok) throw new Error(`Показатели: HTTP ${metricsResponse.status}`);
            if (!activityResponse.ok) throw new Error(`События: HTTP ${activityResponse.status}`);
            const [metrics, activities] = await Promise.all([metricsResponse.json(), activityResponse.json()]);
            renderMetrics(metrics);
            renderActivities(activities);
            document.getElementById('lastUpdated').textContent = 'Обновлено ' + new Intl.DateTimeFormat('ru-RU', { hour: '2-digit', minute: '2-digit' }).format(new Date());
        } catch (error) {
            metricsGrid.innerHTML = '<div class="error-card">Не удалось загрузить данные. Проверьте соединение и повторите попытку.</div>';
            activityList.innerHTML = '<div class="empty-state">Список событий временно недоступен.</div>';
            App.setStatus(error.message || 'Ошибка загрузки данных.', 'error');
            console.error(error);
        }
    }

    function renderMetrics(metrics) {
        if (!metrics.length) {
            metricsGrid.innerHTML = '<div class="empty-state">Показателей пока нет.</div>';
            return;
        }
        metricsGrid.replaceChildren(...metrics.map((metric, index) => {
            const article = document.createElement('article');
            article.className = 'metric-card';
            article.innerHTML = `<div class="metric-top"><span class="metric-icon icon-${index + 1}">${['◉', '▣', '₽', '↗'][index % 4]}</span><span class="metric-change">↗ ${[8.2, 5.4, 12.8, 3.1][index % 4]}%</span></div><div class="metric-label">${App.escapeHtml(metric.name)}</div><div class="metric-value">${formatNumber(metric.value)} <small>${App.escapeHtml(metric.unit)}</small></div><div class="metric-foot">${App.escapeHtml(metric.description)}</div>`;
            return article;
        }));
    }

    function renderActivities(activities) {
        if (!activities.length) {
            activityList.innerHTML = '<div class="empty-state">Событий пока нет. Добавьте первое ниже.</div>';
            return;
        }
        activityList.replaceChildren(...activities.map(item => {
            const row = document.createElement('div');
            row.className = 'activity-item';
            row.innerHTML = `<span class="activity-icon">${App.escapeHtml(item.category.slice(0, 1))}</span><span class="activity-copy"><strong>${App.escapeHtml(item.title)}</strong><small>${App.escapeHtml(item.category)}</small></span><time>${relativeTime(item.createdAt)}</time>`;
            return row;
        }));
    }

    // POST отправляет форму в JSON без обычной навигации браузера.
    form.addEventListener('submit', async event => {
        event.preventDefault();
        const titleInput = document.getElementById('activityTitle');
        const title = titleInput.value.trim();
        const category = document.getElementById('activityCategory').value;
        if (title.length < 3) {
            formMessage.textContent = 'Введите название длиной не менее 3 символов.';
            formMessage.className = 'form-message error';
            titleInput.focus();
            return;
        }
        submitButton.disabled = true;
        submitButton.textContent = 'Отправка…';
        formMessage.textContent = '';
        try {
            const response = await fetch('/api/dashboard/activity', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
                body: JSON.stringify({ title, category })
            });
            const payload = await response.json();
            if (!response.ok) throw new Error(payload.message || `HTTP ${response.status}`);
            titleInput.value = '';
            formMessage.textContent = 'Событие добавлено.';
            formMessage.className = 'form-message success';
            await loadActivity();
        } catch (error) {
            formMessage.textContent = error.message || 'Не удалось добавить событие.';
            formMessage.className = 'form-message error';
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = 'Добавить';
        }
    });

    async function loadActivity() {
        const response = await fetch('/api/dashboard/activity', { headers: { Accept: 'application/json' } });
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        renderActivities(await response.json());
    }

    document.getElementById('refreshButton').addEventListener('click', loadDashboard);
    document.getElementById('activityRefresh').addEventListener('click', async () => {
        try { await loadActivity(); } catch (error) { App.setStatus('Не удалось обновить список событий.', 'error'); }
    });

    loadDashboard();
})();
