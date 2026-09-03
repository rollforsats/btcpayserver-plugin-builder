INSERT INTO settings (key, value)
VALUES ('RegistrationEnabled', 'true'),
       ('NewBuildsEnabled', 'true')
ON CONFLICT (key) DO NOTHING
