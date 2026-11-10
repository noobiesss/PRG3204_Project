-- Local test accounts for development and the demo. Run after the schema script.
-- Public registration only creates Student accounts, so Staff and Admin come from here.
-- Password for all three accounts: Test@12345
-- Never use these accounts or this password on a real system.

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE SchoolEmail = N'admin@campus.edu.my' OR MatricOrStaffNo = N'ADMIN001')
    INSERT INTO dbo.Users (MatricOrStaffNo, FullName, SchoolEmail, PasswordHash, Role)
    VALUES (N'ADMIN001', N'Test Admin', N'admin@campus.edu.my', N'PBKDF2$100000$RS5eO67zJK5Y0+WIjhk9Hg==$qk0EM+kyn/KbpvHHZwr9g62BIdGBynKZvL1qlgKHUNQ=', N'Admin');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE SchoolEmail = N'staff@campus.edu.my' OR MatricOrStaffNo = N'STAFF001')
    INSERT INTO dbo.Users (MatricOrStaffNo, FullName, SchoolEmail, PasswordHash, Role)
    VALUES (N'STAFF001', N'Test Staff', N'staff@campus.edu.my', N'PBKDF2$100000$TC00Z45WKUgDnHYmf7F8sw==$4toKFg1456tyHMx6aQgho8DLqzRneQrXfqmGsLZDAEk=', N'Staff');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE SchoolEmail = N'student@campus.edu.my' OR MatricOrStaffNo = N'STUDENT001')
    INSERT INTO dbo.Users (MatricOrStaffNo, FullName, SchoolEmail, PasswordHash, Role)
    VALUES (N'STUDENT001', N'Test Student', N'student@campus.edu.my', N'PBKDF2$100000$VYW4SKYsTH1w0gH0ZQ78Mg==$vfZzYzSj8gUHLxOG7nJDrdgz86YqfuJaa2iA+oxRplU=', N'Student');
