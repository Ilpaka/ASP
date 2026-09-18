// ===== Сложный уровень. Модалка с параллельной загрузкой через Promise.all =====

(function () {
    'use strict';

    // Экранирование текста, пришедшего с сервера: собираем HTML строками,
    // поэтому кавычки и угловые скобки нужно обезвредить.
    function esc(value) {
        return String(value == null ? '' : value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function stars(rating) {
        const r = Math.max(0, Math.min(5, Number(rating) || 0));
        return '★'.repeat(r) + '☆'.repeat(5 - r);
    }

    // Делегирование: карточки создаются динамически
    document.addEventListener('click', function (e) {
        const trigger = e.target.closest('.product-open');
        if (!trigger) return;

        // Клик по кнопке «В корзину» не должен открывать модалку
        if (e.target.closest('.add-to-cart')) return;

        openProductModal(trigger.dataset.productId);
    });

    async function openProductModal(productId) {
        const modalEl = document.getElementById('productModal');
        const modalBody = document.getElementById('modalBody');
        if (!modalEl || !modalBody) return;

        // 1. Сразу открываем модалку со спиннером — пользователь видит реакцию на клик
        modalBody.innerHTML =
            '<div class="text-center py-5">' +
            '<div class="spinner-border text-primary" role="status">' +
            '<span class="visually-hidden">Загрузка…</span></div></div>';

        bootstrap.Modal.getOrCreateInstance(modalEl).show();

        try {
            // 2. Два запроса параллельно: Promise.all ждёт оба сразу,
            //    а не последовательно — это вдвое быстрее последовательных await.
            const [detailsResp, reviewsResp] = await Promise.all([
                fetch('/Catalog/GetProductDetails?id=' + encodeURIComponent(productId)),
                fetch('/Catalog/GetProductReviews?id=' + encodeURIComponent(productId))
            ]);

            // 3. Детали товара обязательны — без них показывать нечего
            if (!detailsResp.ok) throw new Error('HTTP ' + detailsResp.status);
            const product = await detailsResp.json();

            // 4. Отзывы необязательны: если они не загрузились, товар всё равно покажем
            let reviews = [];
            try {
                if (reviewsResp.ok) {
                    reviews = await reviewsResp.json();
                }
            } catch (reviewsError) {
                // отзывы не пришли — не критично, товар показываем без них
                console.warn('Отзывы не загрузились:', reviewsError);
            }

            // 5. Собираем HTML из JSON
            let html = '<div class="row g-4">';

            html += '<div class="col-12 col-md-5">' +
                '<img src="' + esc(product.imageUrl) + '" class="img-fluid rounded" alt="' + esc(product.name) + '" />' +
                '</div>';

            html += '<div class="col-12 col-md-7">';
            html += '<span class="badge text-bg-light text-muted mb-2">' + esc(product.category) + '</span>';
            html += '<h3>' + esc(product.name) + '</h3>';
            html += '<p class="fw-bold fs-4 text-primary mb-1">' + esc(product.price) + '</p>';

            if (product.discountPercent > 0) {
                html += '<span class="badge text-bg-success mb-2">Скидка ' +
                    esc(product.discountPercent) + '%</span>';
            }

            html += '<p class="text-muted">' + esc(product.description) + '</p>';

            html += product.inStock
                ? '<p class="mb-3"><span class="badge text-bg-success">В наличии</span> ' +
                  '<span class="text-muted small">' + esc(product.stock) + ' шт.</span></p>'
                : '<p class="mb-3"><span class="badge text-bg-danger">Нет в наличии</span></p>';

            if (product.inStock) {
                html += '<button type="button" class="btn btn-primary add-to-cart" ' +
                    'data-product-id="' + esc(product.id) + '">В корзину</button>';
            }

            html += '</div></div>';

            // Отзывы
            html += '<hr /><h5>Отзывы</h5>';

            if (reviews.length > 0) {
                html += '<ul class="list-group">';
                reviews.forEach(function (r) {
                    html += '<li class="list-group-item">' +
                        '<div class="d-flex justify-content-between align-items-center">' +
                        '<strong>' + esc(r.author) + '</strong>' +
                        '<span class="text-warning">' + stars(r.rating) + '</span>' +
                        '</div>' +
                        '<div class="text-muted small mt-1">' + esc(r.text) + '</div>' +
                        '</li>';
                });
                html += '</ul>';
            } else {
                html += '<p class="text-muted">Отзывов пока нет</p>';
            }

            modalBody.innerHTML = html;
        } catch (error) {
            console.error(error);
            modalBody.innerHTML =
                '<div class="alert alert-danger mb-0">Не удалось загрузить данные</div>';
        }
    }
})();
