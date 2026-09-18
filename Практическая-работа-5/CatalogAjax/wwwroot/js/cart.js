// ===== Задание 4. Добавление в корзину через AJAX =====

(function () {
    'use strict';

    // Карточки появляются динамически (поиск, бесконечная подгрузка), поэтому
    // вешаем ОДИН обработчик на document и ловим клик по всплытию (делегирование).
    // Если бы мы через querySelectorAll подписались только на существующие кнопки,
    // на догруженных карточках кнопка не работала бы.
    document.addEventListener('click', function (e) {
        const button = e.target.closest('.add-to-cart');
        if (!button) return;

        // data-product-id="5" -> dataset.productId === "5"
        const productId = button.dataset.productId;
        addToCart(productId, button);
    });

    async function addToCart(productId, button) {
        const originalText = button.textContent;

        // Блокируем кнопку, чтобы двойной клик не отправил два запроса,
        // и показываем спиннер как индикацию процесса.
        button.disabled = true;
        button.innerHTML =
            '<span class="spinner-border spinner-border-sm"></span> Добавление...';

        try {
            // POST, а не GET: запрос изменяет состояние на сервере (кладёт товар в корзину).
            const response = await fetch('/Catalog/AddToCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: 'id=' + encodeURIComponent(productId)
            });

            if (!response.ok) {
                throw new Error('HTTP ' + response.status);
            }

            const data = await response.json();

            if (data.success) {
                // cartCount — актуальное количество товаров, посчитанное сервером.
                // Не увеличиваем счётчик на клиенте: сервер — единственный источник правды.
                Catalog.setBadge(data.cartCount);

                // Лёгкий уровень: вместо смены текста кнопки показываем toast
                Catalog.showToast('Товар «' + data.productName + '» добавлен в корзину');

                // Кратковременная зелёная подсветка кнопки
                button.innerHTML = 'Добавлено ✓';
                button.classList.remove('btn-primary');
                button.classList.add('btn-success');

                setTimeout(function () {
                    button.textContent = originalText;
                    button.classList.remove('btn-success');
                    button.classList.add('btn-primary');
                    button.disabled = false;
                }, 2000);
            } else {
                Catalog.showToast('Ошибка: ' + data.message);
                button.textContent = originalText;
                button.disabled = false;
            }
        } catch (error) {
            console.error(error);
            button.textContent = 'Ошибка';
            button.disabled = false;

            setTimeout(function () {
                button.textContent = originalText;
            }, 2000);
        }
    }
})();
