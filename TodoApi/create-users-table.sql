-- יצירת טבלת Users
USE my_db;

CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserName VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL
);

-- בדיקה
DESCRIBE Users;
SELECT * FROM Users;
f

