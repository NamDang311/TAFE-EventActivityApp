-- DROP DATABASE ActivitiesDb;
USE ActivitiesDb;

-- Create the Activities base table
CREATE TABLE Activities (
    ActivityID INT AUTO_INCREMENT PRIMARY KEY,
    DateStartTime DATETIME NOT NULL,
    ActivityDate DATE AS (DATE (DateStartTime)) STORED,
    Title VARCHAR(50) NOT NULL,
    Cost DECIMAL(10, 2) NOT NULL,
    CONSTRAINT uq_one_per_day UNIQUE (ActivityDate), -- Enforces one activity per day
    CONSTRAINT chk_title_len CHECK (CHAR_LENGTH(Title) >= 3),
    CONSTRAINT chk_cost CHECK (Cost >= 0)
);

-- Create the EntertainmentActivities table
CREATE TABLE EntertainmentActivities (
    ActivityID INT PRIMARY KEY,
    MinParticipants INT NOT NULL,
    CONSTRAINT chk_min_participants CHECK (MinParticipants >= 2),
    FOREIGN KEY (ActivityID) REFERENCES Activities (ActivityID)
);

-- Create the FitnessActivities table
CREATE TABLE FitnessActivities (
    ActivityID INT PRIMARY KEY,
    Location VARCHAR(100) NOT NULL,
    FOREIGN KEY (ActivityID) REFERENCES Activities (ActivityID)
);

-- DELIMITER is a client command (mariadb CLI, HeidiSQL, DBeaver, etc.)
DELIMITER $$
-- Stored procedure to check whether an activity already exists on a given date
CREATE PROCEDURE CheckActivityExistsByDate (IN p_DateToCheck DATE)
BEGIN
SELECT
    EXISTS (
        SELECT
            1
        FROM
            Activities
        WHERE
            ActivityDate = p_DateToCheck
    ) AS ActivityExists;

END $$
-- Stored procedure to add a new fitness activity
CREATE PROCEDURE AddFitnessActivity (
    IN p_DateStartTime DATETIME,
    IN p_Title VARCHAR(50),
    IN p_Cost DECIMAL(10, 2),
    IN p_Location VARCHAR(100)
)
BEGIN DECLARE EXIT
HANDLER FOR SQLEXCEPTION
BEGIN
ROLLBACK;

RESIGNAL;

-- Re-throw the error for the caller to handle
END;

START TRANSACTION;

INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    (p_DateStartTime, p_Title, p_Cost);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), p_Location);

COMMIT;

END $$
-- Stored procedure to add a new entertainment activity
CREATE PROCEDURE AddEntertainmentActivity (
    IN p_DateStartTime DATETIME,
    IN p_Title VARCHAR(50),
    IN p_Cost DECIMAL(10, 2),
    IN p_MinParticipants INT
)
BEGIN DECLARE EXIT
HANDLER FOR SQLEXCEPTION
BEGIN
ROLLBACK;

RESIGNAL;

END;

START TRANSACTION;

INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    (p_DateStartTime, p_Title, p_Cost);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), p_MinParticipants);

COMMIT;

END $$
-- Stored procedure to update an activity
-- p_NewType is 'Fitness' or 'Entertainment'; pass NULL for the unused Location / MinParticipants
CREATE PROCEDURE UpdateActivityWithTypeCheck (
    IN p_ActivityID INT,
    IN p_NewDateStartTime DATETIME,
    IN p_NewTitle VARCHAR(50),
    IN p_NewCost DECIMAL(10, 2),
    IN p_NewType VARCHAR(20),
    IN p_Location VARCHAR(100),
    IN p_MinParticipants INT
)
BEGIN DECLARE v_CurrentType VARCHAR(20) DEFAULT NULL;

DECLARE EXIT
HANDLER FOR SQLEXCEPTION
BEGIN
ROLLBACK;

RESIGNAL;

END;

START TRANSACTION;

-- Get current type
IF EXISTS (
    SELECT
        1
    FROM
        FitnessActivities
    WHERE
        ActivityID = p_ActivityID
) THEN
SET
    v_CurrentType = 'Fitness';

ELSEIF EXISTS (
    SELECT
        1
    FROM
        EntertainmentActivities
    WHERE
        ActivityID = p_ActivityID
) THEN
SET
    v_CurrentType = 'Entertainment';

END IF;

-- Update Activities table
UPDATE Activities
SET
    DateStartTime = p_NewDateStartTime,
    Title = p_NewTitle,
    Cost = p_NewCost
WHERE
    ActivityID = p_ActivityID;

-- If type has changed, delete from old table
IF v_CurrentType IS NOT NULL
AND v_CurrentType <> p_NewType THEN IF v_CurrentType = 'Fitness' THEN
DELETE FROM FitnessActivities
WHERE
    ActivityID = p_ActivityID;

ELSEIF v_CurrentType = 'Entertainment' THEN
DELETE FROM EntertainmentActivities
WHERE
    ActivityID = p_ActivityID;

END IF;

END IF;

-- Insert or update into the correct table
IF p_NewType = 'Fitness' THEN IF EXISTS (
    SELECT
        1
    FROM
        FitnessActivities
    WHERE
        ActivityID = p_ActivityID
) THEN
UPDATE FitnessActivities
SET
    Location = p_Location
WHERE
    ActivityID = p_ActivityID;

ELSE
INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (p_ActivityID, p_Location);

END IF;

ELSEIF p_NewType = 'Entertainment' THEN IF EXISTS (
    SELECT
        1
    FROM
        EntertainmentActivities
    WHERE
        ActivityID = p_ActivityID
) THEN
UPDATE EntertainmentActivities
SET
    MinParticipants = p_MinParticipants
WHERE
    ActivityID = p_ActivityID;

ELSE
INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (p_ActivityID, p_MinParticipants);

END IF;

END IF;

COMMIT;

END $$
-- Stored procedure to retrieve all fitness activities
CREATE PROCEDURE GetAllFitnessActivities ()
BEGIN
SELECT
    A.ActivityID,
    A.DateStartTime,
    A.Title,
    A.Cost,
    F.Location
FROM
    Activities A
    INNER JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
ORDER BY
    A.DateStartTime;

END $$
-- Stored procedure to retrieve all entertainment activities
CREATE PROCEDURE GetAllEntertainmentActivities ()
BEGIN
SELECT
    A.ActivityID,
    A.DateStartTime,
    A.Title,
    A.Cost,
    E.MinParticipants
FROM
    Activities A
    INNER JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
ORDER BY
    A.DateStartTime;

END $$
-- Stored procedure to retrieve all activities
CREATE PROCEDURE GetAllActivities ()
BEGIN
SELECT
    A.ActivityID,
    A.DateStartTime,
    A.Title,
    A.Cost,
    CASE
        WHEN F.ActivityID IS NOT NULL THEN 'Fitness'
        WHEN E.ActivityID IS NOT NULL THEN 'Entertainment'
        ELSE 'Unknown'
    END AS ActivityType,
    F.Location,
    E.MinParticipants
FROM
    Activities A
    LEFT JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
    LEFT JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
ORDER BY
    A.DateStartTime;

END $$
-- Stored procedure to search activities by date
-- p_Operator is 'before', 'on' or 'after'
CREATE PROCEDURE SearchActivitiesByDate (IN p_SearchDate DATE, IN p_Operator VARCHAR(10))
BEGIN IF p_Operator NOT IN ('before', 'on', 'after')
OR p_Operator IS NULL THEN
SIGNAL SQLSTATE '45000'
SET
    MESSAGE_TEXT = 'Invalid operator. Use "before", "on", or "after".';

END IF;

SELECT
    A.ActivityID,
    A.DateStartTime,
    A.Title,
    A.Cost,
    F.Location,
    E.MinParticipants
FROM
    Activities A
    LEFT JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
    LEFT JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
WHERE
    (
        p_Operator = 'before'
        AND A.ActivityDate < p_SearchDate
    )
    OR (
        p_Operator = 'on'
        AND A.ActivityDate = p_SearchDate
    )
    OR (
        p_Operator = 'after'
        AND A.ActivityDate > p_SearchDate
    )
ORDER BY
    A.DateStartTime;

END $$
-- Restore the default statement delimiter for the remaining statements
DELIMITER ;

-- Seed data
-- 01/03/2025 14:00:00, Walking, 0, 30, Petersham Park
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-01 14:00:00', 'Walking', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Petersham Park');

-- 02/03/2025 09:00:00, Running, 0, 30, Sydney Park
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-02 09:00:00', 'Running', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Sydney Park');

-- 08/03/2025 10:15:00, Cycling, 0, 30, Bicentennial Park
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-08 10:15:00', 'Cycling', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Bicentennial Park');

-- 09/03/2025 08:00:00, Yoga, 10, 60, Bondi Beach
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-09 08:00:00', 'Yoga', 10);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Bondi Beach');

-- 22/03/2025 09:00:00, Swimming, 0, 45, Bondi Beach
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-22 09:00:00', 'Swimming', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Bondi Beach');

-- 23/03/2025 09:00:00, Swimming, 10, 120, Sydney Olympic Park Aquatic Centre
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-23 09:00:00', 'Swimming', 10);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (
        LAST_INSERT_ID(),
        'Sydney Olympic Park Aquatic Centre'
    );

-- 25/03/2025 18:00:00, Walking, 0, 30, Petersham Park
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-25 18:00:00', 'Walking', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Petersham Park');

-- 29/03/2025 10:30:00, Walking, 0, 40, Sydney Park
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-29 10:30:00', 'Walking', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Sydney Park');

-- 03/03/2025 19:00:00, Monopoly, 0, 2
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-03 19:00:00', 'Monopoly', 0);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), 2);

-- 10/03/2025 19:30:00, Painting, 5, 1
-- (original script inserted 2 here because MinParticipants must be >= 2)
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-10 19:30:00', 'Painting', 5);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), 2);

-- 16/03/2025 19:30:00, Scrabble, 0, 2
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-16 19:30:00', 'Scrabble', 0);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), 2);

-- 27/03/2025 20:00:00, Charades, 0, 4
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-27 20:00:00', 'Charades', 0);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), 4);

-- 28/03/2025 20:00:00, Cluedo, 0, 3
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-28 20:00:00', 'Cluedo', 0);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), 3);

USE ActivitiesDb;

-- Create the Activities base table
CREATE TABLE Activities (
    ActivityID INT AUTO_INCREMENT PRIMARY KEY,
    DateStartTime DATETIME NOT NULL,
    ActivityDate DATE AS (DATE (DateStartTime)) STORED,
    Title VARCHAR(50) NOT NULL,
    Cost DECIMAL(10, 2) NOT NULL,
    CONSTRAINT uq_one_per_day UNIQUE (ActivityDate), -- Enforces one activity per day
    CONSTRAINT chk_title_len CHECK (CHAR_LENGTH(Title) >= 3),
    CONSTRAINT chk_cost CHECK (Cost >= 0)
);

-- Create the EntertainmentActivities table
CREATE TABLE EntertainmentActivities (
    ActivityID INT PRIMARY KEY,
    MinParticipants INT NOT NULL,
    CONSTRAINT chk_min_participants CHECK (MinParticipants >= 2),
    FOREIGN KEY (ActivityID) REFERENCES Activities (ActivityID)
);

-- Create the FitnessActivities table
CREATE TABLE FitnessActivities (
    ActivityID INT PRIMARY KEY,
    Location VARCHAR(100) NOT NULL,
    FOREIGN KEY (ActivityID) REFERENCES Activities (ActivityID)
);

-- DELIMITER is a client command (mariadb CLI, HeidiSQL, DBeaver, etc.)
DELIMITER $$
-- Stored procedure to check whether an activity already exists on a given date
CREATE PROCEDURE CheckActivityExistsByDate (IN p_DateToCheck DATE)
BEGIN
SELECT
    EXISTS (
        SELECT
            1
        FROM
            Activities
        WHERE
            ActivityDate = p_DateToCheck
    ) AS ActivityExists;

END $$
-- Stored procedure to add a new fitness activity
CREATE PROCEDURE AddFitnessActivity (
    IN p_DateStartTime DATETIME,
    IN p_Title VARCHAR(50),
    IN p_Cost DECIMAL(10, 2),
    IN p_Location VARCHAR(100)
)
BEGIN DECLARE EXIT
HANDLER FOR SQLEXCEPTION
BEGIN
ROLLBACK;

RESIGNAL;

-- Re-throw the error for the caller to handle
END;

START TRANSACTION;

INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    (p_DateStartTime, p_Title, p_Cost);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), p_Location);

COMMIT;

END $$
-- Stored procedure to add a new entertainment activity
CREATE PROCEDURE AddEntertainmentActivity (
    IN p_DateStartTime DATETIME,
    IN p_Title VARCHAR(50),
    IN p_Cost DECIMAL(10, 2),
    IN p_MinParticipants INT
)
BEGIN DECLARE EXIT
HANDLER FOR SQLEXCEPTION
BEGIN
ROLLBACK;

RESIGNAL;

END;

START TRANSACTION;

INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    (p_DateStartTime, p_Title, p_Cost);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), p_MinParticipants);

COMMIT;

END $$
-- Stored procedure to update an activity
-- p_NewType is 'Fitness' or 'Entertainment'; pass NULL for the unused Location / MinParticipants
CREATE PROCEDURE UpdateActivityWithTypeCheck (
    IN p_ActivityID INT,
    IN p_NewDateStartTime DATETIME,
    IN p_NewTitle VARCHAR(50),
    IN p_NewCost DECIMAL(10, 2),
    IN p_NewType VARCHAR(20),
    IN p_Location VARCHAR(100),
    IN p_MinParticipants INT
)
BEGIN DECLARE v_CurrentType VARCHAR(20) DEFAULT NULL;

DECLARE EXIT
HANDLER FOR SQLEXCEPTION
BEGIN
ROLLBACK;

RESIGNAL;

END;

START TRANSACTION;

-- Get current type
IF EXISTS (
    SELECT
        1
    FROM
        FitnessActivities
    WHERE
        ActivityID = p_ActivityID
) THEN
SET
    v_CurrentType = 'Fitness';

ELSEIF EXISTS (
    SELECT
        1
    FROM
        EntertainmentActivities
    WHERE
        ActivityID = p_ActivityID
) THEN
SET
    v_CurrentType = 'Entertainment';

END IF;

-- Update Activities table
UPDATE Activities
SET
    DateStartTime = p_NewDateStartTime,
    Title = p_NewTitle,
    Cost = p_NewCost
WHERE
    ActivityID = p_ActivityID;

-- If type has changed, delete from old table
IF v_CurrentType IS NOT NULL
AND v_CurrentType <> p_NewType THEN IF v_CurrentType = 'Fitness' THEN
DELETE FROM FitnessActivities
WHERE
    ActivityID = p_ActivityID;

ELSEIF v_CurrentType = 'Entertainment' THEN
DELETE FROM EntertainmentActivities
WHERE
    ActivityID = p_ActivityID;

END IF;

END IF;

-- Insert or update into the correct table
IF p_NewType = 'Fitness' THEN IF EXISTS (
    SELECT
        1
    FROM
        FitnessActivities
    WHERE
        ActivityID = p_ActivityID
) THEN
UPDATE FitnessActivities
SET
    Location = p_Location
WHERE
    ActivityID = p_ActivityID;

ELSE
INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (p_ActivityID, p_Location);

END IF;

ELSEIF p_NewType = 'Entertainment' THEN IF EXISTS (
    SELECT
        1
    FROM
        EntertainmentActivities
    WHERE
        ActivityID = p_ActivityID
) THEN
UPDATE EntertainmentActivities
SET
    MinParticipants = p_MinParticipants
WHERE
    ActivityID = p_ActivityID;

ELSE
INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (p_ActivityID, p_MinParticipants);

END IF;

END IF;

COMMIT;

END $$
-- Stored procedure to retrieve all fitness activities
CREATE PROCEDURE GetAllFitnessActivities ()
BEGIN
SELECT
    A.ActivityID,
    A.DateStartTime,
    A.Title,
    A.Cost,
    F.Location
FROM
    Activities A
    INNER JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
ORDER BY
    A.DateStartTime;

END $$
-- Stored procedure to retrieve all entertainment activities
CREATE PROCEDURE GetAllEntertainmentActivities ()
BEGIN
SELECT
    A.ActivityID,
    A.DateStartTime,
    A.Title,
    A.Cost,
    E.MinParticipants
FROM
    Activities A
    INNER JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
ORDER BY
    A.DateStartTime;

END $$
-- Stored procedure to retrieve all activities
CREATE PROCEDURE GetAllActivities ()
BEGIN
SELECT
    A.ActivityID,
    A.DateStartTime,
    A.Title,
    A.Cost,
    CASE
        WHEN F.ActivityID IS NOT NULL THEN 'Fitness'
        WHEN E.ActivityID IS NOT NULL THEN 'Entertainment'
        ELSE 'Unknown'
    END AS ActivityType,
    F.Location,
    E.MinParticipants
FROM
    Activities A
    LEFT JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
    LEFT JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
ORDER BY
    A.DateStartTime;

END $$
-- Stored procedure to search activities by date
-- p_Operator is 'before', 'on' or 'after'
CREATE PROCEDURE SearchActivitiesByDate (IN p_SearchDate DATE, IN p_Operator VARCHAR(10))
BEGIN IF p_Operator NOT IN ('before', 'on', 'after')
OR p_Operator IS NULL THEN
SIGNAL SQLSTATE '45000'
SET
    MESSAGE_TEXT = 'Invalid operator. Use "before", "on", or "after".';

END IF;

SELECT
    A.ActivityID,
    A.DateStartTime,
    A.Title,
    A.Cost,
    F.Location,
    E.MinParticipants
FROM
    Activities A
    LEFT JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
    LEFT JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
WHERE
    (
        p_Operator = 'before'
        AND A.ActivityDate < p_SearchDate
    )
    OR (
        p_Operator = 'on'
        AND A.ActivityDate = p_SearchDate
    )
    OR (
        p_Operator = 'after'
        AND A.ActivityDate > p_SearchDate
    )
ORDER BY
    A.DateStartTime;

END $$
-- Restore the default statement delimiter for the remaining statements
DELIMITER ;

-- Seed data
-- 01/03/2025 14:00:00, Walking, 0, 30, Petersham Park
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-01 14:00:00', 'Walking', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Petersham Park');

-- 02/03/2025 09:00:00, Running, 0, 30, Sydney Park
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-02 09:00:00', 'Running', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Sydney Park');

-- 08/03/2025 10:15:00, Cycling, 0, 30, Bicentennial Park
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-08 10:15:00', 'Cycling', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Bicentennial Park');

-- 09/03/2025 08:00:00, Yoga, 10, 60, Bondi Beach
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-09 08:00:00', 'Yoga', 10);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Bondi Beach');

-- 22/03/2025 09:00:00, Swimming, 0, 45, Bondi Beach
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-22 09:00:00', 'Swimming', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Bondi Beach');

-- 23/03/2025 09:00:00, Swimming, 10, 120, Sydney Olympic Park Aquatic Centre
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-23 09:00:00', 'Swimming', 10);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (
        LAST_INSERT_ID(),
        'Sydney Olympic Park Aquatic Centre'
    );

-- 25/03/2025 18:00:00, Walking, 0, 30, Petersham Park
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-25 18:00:00', 'Walking', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Petersham Park');

-- 29/03/2025 10:30:00, Walking, 0, 40, Sydney Park
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-29 10:30:00', 'Walking', 0);

INSERT INTO
    FitnessActivities (ActivityID, Location)
VALUES
    (LAST_INSERT_ID(), 'Sydney Park');

-- 03/03/2025 19:00:00, Monopoly, 0, 2
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-03 19:00:00', 'Monopoly', 0);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), 2);

-- 10/03/2025 19:30:00, Painting, 5, 1
-- (original script inserted 2 here because MinParticipants must be >= 2)
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-10 19:30:00', 'Painting', 5);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), 2);

-- 16/03/2025 19:30:00, Scrabble, 0, 2
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-16 19:30:00', 'Scrabble', 0);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), 2);

-- 27/03/2025 20:00:00, Charades, 0, 4
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-27 20:00:00', 'Charades', 0);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), 4);

-- 28/03/2025 20:00:00, Cluedo, 0, 3
INSERT INTO
    Activities (DateStartTime, Title, Cost)
VALUES
    ('2025-03-28 20:00:00', 'Cluedo', 0);

INSERT INTO
    EntertainmentActivities (ActivityID, MinParticipants)
VALUES
    (LAST_INSERT_ID(), 3);