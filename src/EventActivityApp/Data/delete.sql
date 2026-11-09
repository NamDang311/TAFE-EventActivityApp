USE ActivitiesDb;


SET @id = 20;

DELETE FROM FitnessActivities       WHERE ActivityID = @id;
DELETE FROM EntertainmentActivities WHERE ActivityID = @id;
DELETE FROM Activities              WHERE ActivityID = @id;
