# Todo List - Fullstack עם JWT Authentication

## סיכום הפרויקט

פרויקט Fullstack מלא עם:

- **Server**: .NET 8 Minimal API
- **Client**: React
- **Database**: MySQL
- **Authentication**: JWT (JSON Web Tokens)

---

## מה השתנה באתגר JWT?

### 1. שדרוג ל-.NET 8

- הפרויקט שודרג מ-NET 9 ל-NET 8 כדי לתמוך ב-JWT
- כל החבילות עודכנו לגרסאות תואמות

### 2. צד השרת (API)

#### קבצים חדשים:

- `User.cs` - מודל משתמש
- `UserDto.cs` - DTO להרשמה והתחברות

#### עדכונים:

- `appsettings.json` - הוספת הגדרות JWT (Key, Issuer, Audience)
- `ToDoDbContext.cs` - הוספת `DbSet<User>` וקונפיגורציה לטבלת Users
- `Program.cs` - הוספת:
  - Authentication & Authorization middleware
  - `/auth/register` - הרשמת משתמש חדש
  - `/auth/login` - התחברות וקבלת JWT token
- `create-database.sql` - הוספת טבלת Users

### 3. צד הקליינט (React)

#### קבצים חדשים:

- `Login.js` - קומפוננטת התחברות/הרשמה

#### עדכונים:

- `service.js` - הוספת:
  - פונקציות `register` ו-`login`
  - ניהול token ב-localStorage
  - הוספת Authorization header אוטומטית
  - Interceptor שתופס 401 ומעביר ללוגין
- `App.js` - הוספת:
  - מסך לוגין/הרשמה
  - כפתור התנתקות
  - בדיקת מצב התחברות

---

## איך להריץ?

### 1. יצירת טבלת Users ב-MySQL

פתחי MySQL Workbench והריצי:

\`\`\`sql
USE my_db;

CREATE TABLE IF NOT EXISTS Users (
Id INT AUTO_INCREMENT PRIMARY KEY,
UserName VARCHAR(100) NOT NULL UNIQUE,
PasswordHash VARCHAR(255) NOT NULL
);
\`\`\`

או פשוט הריצי את `create-database.sql`.

### 2. הפעלת השרת

\`\`\`powershell
cd TodoApi
dotnet run
\`\`\`

השרת יעלה על: http://localhost:5140

### 3. הפעלת הקליינט

\`\`\`powershell
cd ToDoListReact
npm start
\`\`\`

הקליינט יעלה על: http://localhost:3000

---

## תהליך השימוש

1. **הרשמה**:

   - בפעם הראשונה, תראי מסך התחברות
   - לחצי "אין לי חשבון - הירשם"
   - הכניסי שם משתמש וסיסמה
   - לחצי "הירשם"

2. **התחברות**:

   - לאחר ההרשמה, הכניסי שוב את הפרטים
   - לחצי "התחבר"
   - תקבלי JWT token שיישמר ב-localStorage

3. **שימוש באפליקציה**:

   - כל הבקשות ל-API ישלחו עם Authorization header
   - תוכלי להוסיף, לערוך ולמחוק משימות

4. **התנתקות**:
   - לחצי על כפתור "התנתק" למעלה מימין
   - ה-token יימחק ותחזרי למסך התחברות

---

## אבטחה

⚠️ **שימו לב**: בפרויקט זה הסיסמאות נשמרות כטקסט רגיל (לא hash) לצורך הפשטות.

בפרויקט אמיתי, יש להשתמש ב:

- BCrypt או SHA256 להצפנת סיסמאות
- HTTPS
- Refresh Tokens
- הגנה מפני CSRF

---

## דרישות האתגר שמומשו

✅ הוספת טבלת Users עם: מזהה, שם משתמש, סיסמה
✅ מנגנון הזדהות עם JWT ב-API
✅ דף הרשמה והתחברות בקליינט
✅ Interceptor שתופס 401 ומעביר ללוגין

---

## Swagger

לבדיקת ה-API ישירות:
http://localhost:5140/swagger

שם ניתן לבדוק:

- POST /auth/register - הרשמה
- POST /auth/login - התחברות
- כל ה-CRUD של המשימות

---

**בהצלחה! 🚀**
