-- One-time update for databases created before 2026-11-12.
-- Makes Users.Gender and Users.DateOfBirth required (NOT NULL).
-- Fills the three seed test accounts first. Any other account with an empty
-- gender or date of birth must be fixed by hand, or the script stops.

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

UPDATE dbo.Users SET Gender = COALESCE(Gender, N'Male'), DateOfBirth = COALESCE(DateOfBirth, '1985-03-15') WHERE MatricOrStaffNo = N'ADMIN001';
UPDATE dbo.Users SET Gender = COALESCE(Gender, N'Female'), DateOfBirth = COALESCE(DateOfBirth, '1990-07-20') WHERE MatricOrStaffNo = N'STAFF001';
UPDATE dbo.Users SET Gender = COALESCE(Gender, N'Male'), DateOfBirth = COALESCE(DateOfBirth, '2004-05-10') WHERE MatricOrStaffNo = N'STUDENT001';

IF EXISTS (SELECT 1 FROM dbo.Users WHERE Gender IS NULL OR DateOfBirth IS NULL)
BEGIN
    ROLLBACK TRANSACTION;
    SELECT UserId, MatricOrStaffNo, SchoolEmail FROM dbo.Users WHERE Gender IS NULL OR DateOfBirth IS NULL;
    RAISERROR(N'Fill in Gender and DateOfBirth for the accounts listed, then run this script again.', 16, 1);
    RETURN;
END

ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_Gender;
ALTER TABLE dbo.Users ALTER COLUMN Gender nvarchar(10) NOT NULL;
ALTER TABLE dbo.Users ALTER COLUMN DateOfBirth date NOT NULL;
ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Gender CHECK (Gender IN ('Male', 'Female'));

COMMIT TRANSACTION;
GO
