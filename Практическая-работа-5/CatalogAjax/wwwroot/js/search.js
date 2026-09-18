// ===== Задание 3. Live-поиск без перезагрузки =====

(function () {
    'use strict';

    const input = document.getElementById('searchInput');
    const grid = document.getElementById('catalogGrid');
    if (!input || !grid) return;

    const sentinel = document.getElementById('sentinel');
    const endOfList = document.getElementById('endOfList');

    let timeoutId;

    // Шаг 2-3: подписка на ввод + дебаунс 300 мс.
    // Без дебаунса запрос уходил бы на каждое нажатие клавиши.
    input.addEventListener('input', function () {
        clearTimeout(timeoutId); // отменяем предыдущий отложенный запрос
        const query = input.value.trim();

        if (query.length < 2) {
            // Поиск сброшен — возвращаем обычный каталог с постраничной подгрузкой
            if (Catalog.searchActive) {
                window.location.reload();
            }
            return;
        }

        timeoutId = setTimeout(function () {
            searchProducts(query);
        }, 300);
    });

    // Шаг 4: запрос с состояниями «загрузка / ошибка / результат»
    async function searchProducts(query) {
        // 1. Спиннер на время запроса
        grid.innerHTML =
            '<div class="col-12 text-center py-5">' +
            '<div class="spinner-border text-primary" role="status">' +
            '<span class="visually-hidden">Поиск…</span></div>' +
            '</div>';

        // В режиме поиска бесконечная подгрузка не нужна
        Catalog.searchActive = true;
        if (sentinel) sentinel.style.display = 'none';
        if (endOfList) endOfList.classList.add('d-none');

        try {
            // 2. Запрос к серверу.
            // encodeURIComponent экранирует пробелы, кириллицу и символы вроде & и #,
            // иначе они сломали бы строку запроса.
            const url = '/Catalog/Search?query=' + encodeURIComponent(query);
            const response = await fetch(url);

            if (!response.ok) {
                throw new Error('HTTP ' + response.status);
            }

            // 3. Сервер отдаёт HTML-фрагмент (partial), а не JSON
            const html = await response.text();

            // 4. Пустой ответ — ничего не нашлось
            if (!html.trim()) {
                grid.innerHTML =
                    '<div class="col-12">' +
                    '<div class="alert alert-info text-center mb-0">Ничего не найдено</div>' +
                    '</div>';
            } else {
                // 5. Вставляем готовые карточки в сетку
                grid.innerHTML = html;
            }
        } catch (error) {
            // 6. Сеть недоступна или сервер вернул ошибку.
            // Без try/catch ошибка «утекла» бы в необработанный промис,
            // а пользователь остался бы со спиннером навсегда.
            grid.innerHTML =
                '<div class="col-12">' +
                '<div class="alert alert-danger text-center mb-0">Ошибка поиска</div>' +
                '</div>';
            console.error(error);
        }
    }
})();
