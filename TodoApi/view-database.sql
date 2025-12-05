-- ======================================
-- תצוגת מסד הנתונים - my_db
-- ======================================

-- הצגת כל הטבלאות
SHOW TABLES FROM my_db;

-- הצגת מבנה הטבלה
DESCRIBE my_db.my_table;

-- הצגת כל המשימות
SELECT 
    Id AS 'מזהה',
    Name AS 'שם המשימה',
    CASE 
        WHEN IsComplete = 1 THEN 'הושלמה ✓'
        ELSE 'טרם הושלמה ○'
    END AS 'סטטוס'
FROM my_db.my_table
ORDER BY Id;

-- ספירת משימות
SELECT 
    COUNT(*) AS 'סה"כ משימות',
    SUM(IsComplete) AS 'הושלמו',
    COUNT(*) - SUM(IsComplete) AS 'טרם הושלמו'
FROM my_db.my_table;
