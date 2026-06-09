# FitTrack

Уеб приложение за проследяване на физическа активност и тренировки, изградено с **ASP.NET Core 9 MVC**.

## Технологии

| | |
|---|---|
| **Backend** | ASP.NET Core 9 MVC |
| **База данни** | SQL Server Express + Entity Framework Core 9 |
| **Frontend** | Razor Views, Bootstrap 5, Chart.js |
| **Автентикация** | ASP.NET Core Identity |
| **Тестове** | xUnit, Moq, EF Core InMemory (11 теста) |

## Функционалности

- Регистрация, вход и настройка на профил (ръст, тегло, фитнес цел)
- Разглеждане и записване в тренировъчни програми
- Седмичен план с дни и упражнения
- Календар с автоматично генерирани и ръчно добавяни тренировки
- Проследяване на тегло и тренировки с графики
- Система за отзиви и рейтинг на програми
- Администраторски панел за управление на всичко

## Изисквания

| Изискване | Версия |
|---|---|
| .NET SDK | 9.0 или по-нова |
| SQL Server | Express / Developer / Standard |

## Инсталация

**1. Клонирай проекта**

```bash
git clone https://github.com/alex1234bg/FitTrack.git
```

или свали като ZIP → **Code → Download ZIP** и разархивирай.

---

**2. База данни — избери един от двата начина**

**Вариант А — Автоматично (препоръчително)**

Базата `FitTrackDb` се създава автоматично при първото стартиране на приложението. Не се изисква никакво допълнително действие.

**Вариант Б — Ръчен импорт през SSMS**

1. Отвори **SSMS** и се свържи към `localhost\SQLEXPRESS`
2. **File → Open → File...** → избери `FitTrackDb.sql`
3. Натисни **Execute (F5)**

---

**3. Провери connection string (при нужда)**

Отвори `FitTrack/appsettings.json`. По подразбиране е:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=FitTrackDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Ако SQL Server инстанцията е различна (напр. `MSSQLSERVER`), смени `localhost\\SQLEXPRESS` с правилното име.

---

**4. Стартирай приложението**

```bash
cd FitTrack
dotnet run
```

или отвори `.sln` файла във **Visual Studio** и натисни **F5**.

---

**4. Отвори в браузър**

След стартиране адресът се показва в терминала (обикновено `https://localhost:5001`).

## Данни за вход

| Роля | Имейл | Парола |
|---|---|---|
| **Admin** | `admin@fittrack.com` | `Admin123!` |
| **Потребител** | Регистрирай се от формата | — |

## Тестове

```bash
dotnet test
```
