// ===== Скрипты витрины (собираются в site.min.js) =====

(function () {
    "use strict";

    // 1. Автоматическое скрытие alert через 4 секунды.
    //    Кнопка закрытия работает штатными средствами Bootstrap (data-bs-dismiss).
    document.addEventListener("DOMContentLoaded", function () {
        var autoAlerts = document.querySelectorAll(".alert-auto");
        autoAlerts.forEach(function (el) {
            setTimeout(function () {
                if (window.bootstrap && bootstrap.Alert) {
                    bootstrap.Alert.getOrCreateInstance(el).close();
                } else {
                    el.style.display = "none";
                }
            }, 4000);
        });
    });

    // 2. Тег-фильтр: мгновенная визуальная подсветка выбранного тега.
    //    Реальная фильтрация выполняется на сервере (ссылка всё равно сработает).
    document.addEventListener("DOMContentLoaded", function () {
        var tagButtons = document.querySelectorAll(".tag-filter .btn");
        tagButtons.forEach(function (btn) {
            btn.addEventListener("click", function () {
                tagButtons.forEach(function (b) {
                    b.classList.remove("active", "btn-primary");
                    b.classList.add("btn-outline-secondary");
                });
                btn.classList.remove("btn-outline-secondary");
                btn.classList.add("active", "btn-primary");
            });
        });
    });
})();
