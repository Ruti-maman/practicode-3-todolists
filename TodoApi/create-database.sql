-- ======================================
-- יצירת מסד נתונים וטבלה למשימות
-- ======================================

-- יצירת מסד הנתונים (אם לא קיים)
CREATE DATABASE IF NOT EXISTS my_db;

-- שימוש במסד הנתונים
USE my_db;

-- יצירת הטבלה (אם לא קיימת)
CREATE TABLE IF NOT EXISTS my_table (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100),
    IsComplete TINYINT(1) DEFAULT 0
);

-- יצירת טבלת משתמשים
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserName VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL
);

-- הצגת המבנה
DESCRIBE my_table;
DESCRIBE Users;

-- הצגת כל הנתונים
SELECT * FROM my_table;
SELECT * FROM Users;

-- הוספת משימות לדוגמא (אם הטבלה ריקה)
-- INSERT INTO my_table (Name, IsComplete) VALUES ('לסיים את הפרויקט', 0);
-- INSERT INTO my_table (Name, IsComplete) VALUES ('לקנות חלב', 0);
-- INSERT INTO my_table (Name, IsComplete) VALUES ('להתקשר לסבתא', 1);
