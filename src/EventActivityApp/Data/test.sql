USE ActivitiesDb;

CALL CheckActivityExistsByDate ('2025-03-01');

CALL CheckActivityExistsByDate ('2030-01-10');

CALL AddFitnessActivity (
    '2030-01-10 07:00:00',
    'Test Swimming',
    3.00,
    'Liverpool Center'
);

CALL CheckActivityExistsByDate ('2030-01-10');

CALL AddEntertainmentActivity (
    '2030-01-11 19:30:00',
    'Test Movie Night',
    12.50,
    4
);

CALL GetAllFitnessActivities ();

CALL GetAllEntertainmentActivities ();

CALL GetAllActivities ();