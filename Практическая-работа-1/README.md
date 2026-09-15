# Практическая работа №1 — ASP.NET Web Forms

Приложение ASP.NET Web Forms (.NET Framework 4.7.2) к практической работе №1
«Знакомство с ASP.NET Web Forms: событийная модель и ViewState».

## Что реализовано

| Шаг | Содержание | Где |
|-----|------------|-----|
| 1–2 | Проект Web Forms и структура файлов | `WebFormsApp/`, разбор в `ОТЧЁТ.md` |
| 3–5 | Форма приветствия (`TextBox` + `Button` + `Label`), обработчик `btnOk_Click` | `Default.aspx`, `Default.aspx.cs` |
| 6   | Обработчики жизненного цикла (`Init/Load/PreRender/Unload`), вывод в `Literal` | `Default.aspx.cs` |
| 7–8 | Наблюдение `__VIEWSTATE`, `GridView` на 3 строки, сравнение размеров | `Default.aspx`, `ОТЧЁТ.md` |
| 9   | Кнопка «Очистить» | `btnClear_Click` |
| 10  | Мини-калькулятор с `DropDownList` и обработкой деления на ноль (`try/catch`) | `btnCalc_Click` |

Отчёт по работе — в файле [`ОТЧЁТ.md`](ОТЧЁТ.md).

## Запуск в Visual Studio (штатный способ)

1. Открыть `WebFormsApp.sln` в Visual Studio 2019/2022
   (компонент «Разработка ASP.NET и веб-приложений»).
2. Нажать **F5** — проект запустится в IIS Express, откроется `Default.aspx`.

## Запуск без Windows (Docker + Mono/XSP)

На машине без Visual Studio приложение можно поднять кросс-платформенно через
Mono и веб-сервер XSP. Из папки `Практическая-работа-1`:

```bash
docker run --rm -v "$PWD":/app -w /app -p 9000:9000 mono:6.12 bash -c '
  printf "deb http://archive.debian.org/debian buster main\ndeb http://archive.debian.org/debian-security buster/updates main\n" > /etc/apt/sources.list
  apt-get -o Acquire::Check-Valid-Until=false update >/dev/null 2>&1
  apt-get install -y --no-install-recommends procps mono-xsp4 >/dev/null 2>&1
  cd WebFormsApp && mkdir -p bin
  mcs -target:library -out:bin/WebFormsApp.dll \
      -r:System.dll -r:System.Core.dll -r:System.Web.dll -r:System.Data.dll -r:System.Xml.dll \
      Default.aspx.cs Default.aspx.designer.cs Site.Master.cs Site.Master.designer.cs Global.asax.cs Properties/AssemblyInfo.cs
  xsp4 --port 9000 --nonstop
'
```

Затем открыть <http://localhost:9000/Default.aspx>.

> Web Forms — технология .NET Framework, штатно работающая на Windows/IIS.
> Вариант с Mono/XSP использован для проверки работоспособности на macOS: он
> компилирует код-behind в `bin/WebFormsApp.dll` и обслуживает страницы. Именно на
> нём получены значения `__VIEWSTATE` (384 → 1408 символов) из отчёта.

## Структура

```
Практическая-работа-1/
├── WebFormsApp.sln
├── ОТЧЁТ.md
├── README.md
└── WebFormsApp/
    ├── Default.aspx / .aspx.cs / .aspx.designer.cs
    ├── Site.Master / .Master.cs / .Master.designer.cs
    ├── Global.asax / .asax.cs
    ├── Web.config
    ├── packages.config
    ├── WebFormsApp.csproj
    └── Properties/AssemblyInfo.cs
```
