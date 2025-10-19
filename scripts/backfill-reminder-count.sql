-- Backfill ReminderCount and LastReminderDate from FineActionHistories
-- Usage: run against your database (make a backup first)

-- Count ReminderSent actions per fine and update ReminderCount
UPDATE f
SET f.ReminderCount = fh.ReminderCount
FROM Fines f
JOIN (
    SELECT FineId, COUNT(*) AS ReminderCount, MAX(ActionDate) AS LastReminderDate
    FROM FineActionHistories
    WHERE ActionType = 0 -- ReminderSent (enum ordinal)
    GROUP BY FineId
) fh ON f.Id = fh.FineId;

-- Update LastReminderDate where applicable
UPDATE f
SET f.LastReminderDate = fh.LastReminderDate
FROM Fines f
JOIN (
    SELECT FineId, MAX(ActionDate) AS LastReminderDate
    FROM FineActionHistories
    WHERE ActionType = 0
    GROUP BY FineId
) fh ON f.Id = fh.FineId;

-- For fines without history, ensure ReminderCount is at least 0
UPDATE Fines SET ReminderCount = 0 WHERE ReminderCount IS NULL;

-- Done
SELECT 'Backfill complete' AS Message;