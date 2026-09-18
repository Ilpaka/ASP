// ===== Средний уровень. Бесконечная подгрузка (Infinite Scroll) =====

(function () {
    'use strict';

    const grid = document.getElementById('catalogGrid');
    const sentinel = document.getElementById('sentinel');
    const spinner = document.getElementById('loadMoreSpinner');
    const endOfList = document.getElementById('endOfList');
    if (!grid || !sentinel) return;

    const tag = grid.dataset.tag || 'all';

    let currentPage = 1;
    let isLoading = false;   // защита от параллельных запросов
    let hasMore = true;

    // IntersectionObserver сообщает, когда «сентинель» появился в зоне видимости.
    // Это дешевле, чем слушать событие scroll и считать координаты вручную.
    const observer = new IntersectionObserver(async function (entries) {
        if (!entries[0].isIntersecting) return;
        if (isLoading || !hasMore) return;

        // В режиме поиска подгрузка отключена — там своя выдача
        if (Catalog.searchActive) return;

        isLoading = true;
        if (spinner) spinner.classList.remove('d-none');

        try {
            currentPage++;
            const url = '/Catalog/LoadMore?page=' + currentPage +
                        '&tag=' + encodeURIComponent(tag);

            const response = await fetch(url);
            if (!response.ok) throw new Error('HTTP ' + response.status);

            const html = await response.text();

            if (html.trim()) {
                // insertAdjacentHTML добавляет карточки в конец,
                // не перерисовывая уже отрисованные (в отличие от innerHTML +=).
                grid.insertAdjacentHTML('beforeend', html);
            } else {
                // Пустой ответ — товары закончились
                hasMore = false;
                observer.disconnect();
                if (endOfList) endOfList.classList.remove('d-none');
            }
        } catch (error) {
            console.error('Не удалось загрузить следующую порцию:', error);
            currentPage--; // откатываем счётчик, чтобы можно было повторить
        } finally {
            isLoading = false;
            if (spinner) spinner.classList.add('d-none');
        }
    }, { rootMargin: '150px' });

    observer.observe(sentinel);
})();
