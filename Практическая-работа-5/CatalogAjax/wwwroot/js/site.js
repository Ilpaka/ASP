// ===== Глобальный скрипт: подключается на ВСЕХ страницах (Задание 6.1) =====
// Здесь только то, что нужно везде: счётчик корзины в шапке и общие помощники.

window.Catalog = window.Catalog || {};

// Обновление бейджа корзины
Catalog.setBadge = function (count) {
    var badge = document.getElementById('cartBadge');
    if (badge) {
        badge.textContent = count;
    }
};

// Показ toast-уведомления (лёгкий уровень).
// Используем Bootstrap-компонент Toast из собственного JS-кода.
Catalog.showToast = function (text) {
    var toastEl = document.getElementById('cartToast');
    var toastText = document.getElementById('cartToastText');
    if (!toastEl) return;

    if (toastText) toastText.textContent = text;

    if (window.bootstrap && bootstrap.Toast) {
        var toast = new bootstrap.Toast(toastEl, { delay: 3000 });
        toast.show();
    }
};

// Флаг: активен ли режим поиска (нужен, чтобы не мешать бесконечной подгрузке)
Catalog.searchActive = false;

// Задание 5: при загрузке любой страницы спрашиваем у сервера реальное количество
// товаров в корзине. Корзина живёт в сессии, а страница может прийти из кэша —
// поэтому бейдж синхронизируем отдельным запросом.
document.addEventListener('DOMContentLoaded', async function () {
    try {
        const response = await fetch('/Catalog/GetCartCount');
        if (!response.ok) throw new Error('HTTP ' + response.status);

        const data = await response.json();
        Catalog.setBadge(data.count > 0 ? data.count : 0);
    } catch (error) {
        console.error('Не удалось получить количество товаров:', error);
    }
});
